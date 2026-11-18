using Base.AttributesPackage;
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

namespace Base.UIPackage.Effects
{
    /// <summary>
    /// Hundreds of small sprites in one mesh, for UI effects too big for <see cref="UISparkles"/>, like fire over the
    /// whole screen. One graphic instead of one Image each, so it costs a single draw call. Runs on scaled time, so
    /// it freezes with the game when a menu pauses it.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class UIParticleField : MaskableGraphic
    {
        private const int ColorSteps = 32;
        private const float Half = 0.5f;
        private const float StillSpeed = 0.01f;
        private const float TwoPi = Mathf.PI * 2f;

        [Title("Look")]
        [Required]
        [Tooltip("The sprite every particle draws.")]
        [SerializeField] private Sprite sprite;

        [Tooltip("Color and alpha of a particle from birth (left) to death (right). Multiplied by its tint.")]
        [SerializeField] private Gradient colorOverLife = new();

        [Title("Motion")]
        [Tooltip("Units per second squared a particle falls. 0 leaves it on its course.")]
        [SerializeField] private float gravity;

        [Min(0f)]
        [Tooltip("How fast a particle loses its speed.")]
        [SerializeField] private float drag;

        [Min(0f)]
        [Tooltip("Units per second a particle wanders from side to side, so fire flickers instead of rising straight.")]
        [SerializeField] private float sway;

        [Min(0f)]
        [Tooltip("How many times a second it wanders from side to side.")]
        [SerializeField] private float swaySpeed = 1f;

        [Min(1f)]
        [Tooltip("Length of a particle along its motion, relative to its width. Above 1 it points where it flies, so "
            + "soft dots read as flames. 1 keeps it round and it turns by its spin.")]
        [SerializeField] private float stretch = 1f;

        [Min(1)]
        [Tooltip("Most particles alive at once. New ones are dropped above it.")]
        [SerializeField] private int capacity = 800;

        private UIParticle[] _particles;
        private Color32[] _colors;
        private int _count;

        /// <inheritdoc/>
        public override Texture mainTexture => sprite == null
            ? s_WhiteTexture
            : sprite.texture;

        /// <summary>While true nothing moves or ages, for a frozen moment.</summary>
        public bool IsFrozen { get; set; }

#region Unity Callbacks
        private void Update()
        {
            float deltaTime = Time.deltaTime;
            if (_count == 0
                || IsFrozen
                || deltaTime <= 0f)
                return;

            Simulate(deltaTime);
            SetVerticesDirty();
        }
#endregion

        /// <summary>
        /// Sprays <paramref name="count"/> particles out of <paramref name="worldOrigin"/> in a cone around
        /// <paramref name="direction"/> degrees (90 is up), <paramref name="spread"/> degrees wide. Each gets a random
        /// speed and lifetime out of <paramref name="speedRange"/> and <paramref name="lifetimeRange"/>, shrinks from
        /// the x to the y of <paramref name="sizeOverLife"/> and spins up to <paramref name="maxSpin"/> degrees a
        /// second.
        /// </summary>
        public void Spray(Vector3 worldOrigin, int count, float direction, float spread, Vector2 speedRange,
            Vector2 lifetimeRange, Vector2 sizeOverLife, float maxSpin)
        {
            Vector2 from = rectTransform.InverseTransformPoint(worldOrigin);

            for (int index = 0; index < count; index++)
            {
                float angle = (direction + Random.Range(-spread, spread) * Half) * Mathf.Deg2Rad;
                Vector2 velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle))
                    * Random.Range(speedRange.x, speedRange.y);
                float spin = Random.Range(-maxSpin, maxSpin) * Mathf.Deg2Rad;

                Emit(from, velocity, Random.Range(lifetimeRange.x, lifetimeRange.y), sizeOverLife.x, sizeOverLife.y,
                    Color.white, spin);
            }
        }

        /// <summary>Adds one particle, in this graphic's local units. Dropped when the field is full.</summary>
        public void Emit(Vector2 position, Vector2 velocity, float lifetime, float startSize, float endSize,
            Color32 tint, float spin = 0f)
        {
            EnsureBuffers();

            if (_count >= capacity
                || lifetime <= 0f)
                return;

            _particles[_count++] = new UIParticle
            {
                Position = position,
                Velocity = velocity,
                Tint = tint,
                Lifetime = lifetime,
                StartSize = startSize,
                EndSize = endSize,
                Rotation = Random.Range(0f, TwoPi),
                Spin = spin,
                SwayPhase = Random.Range(0f, TwoPi)
            };
        }

        /// <inheritdoc/>
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (_count == 0
                || sprite == null)
                return;

            Vector4 uv = DataUtility.GetOuterUV(sprite);
            Color32 own = color;

            for (int i = 0; i < _count; i++)
                AddQuad(vh, _particles[i], uv, own);
        }

        private static byte MultiplyChannel(byte first, byte second) => (byte)(first * second / byte.MaxValue);

        private void EnsureBuffers()
        {
            if (_particles != null)
                return;

            _particles = new UIParticle[capacity];
            _colors = new Color32[ColorSteps];

            for (int i = 0; i < ColorSteps; i++)
                _colors[i] = colorOverLife.Evaluate(i / (ColorSteps - 1f));
        }

        private void Simulate(float deltaTime)
        {
            float keep = Mathf.Exp(-drag * deltaTime);

            // Backwards, because a dead particle takes the place of the last one.
            for (int i = _count - 1; i >= 0; i--)
            {
                ref UIParticle particle = ref _particles[i];
                particle.Age += deltaTime;

                if (particle.Age >= particle.Lifetime)
                {
                    _particles[i] = _particles[--_count];
                    continue;
                }

                particle.Velocity = particle.Velocity * keep + Vector2.down * (gravity * deltaTime);
                float wander = Mathf.Sin(particle.Age * swaySpeed * TwoPi + particle.SwayPhase) * sway;
                particle.Position += (particle.Velocity + Vector2.right * wander) * deltaTime;
                particle.Rotation += particle.Spin * deltaTime;
            }
        }

        private Vector2 GetDirection(in UIParticle particle)
        {
            if (stretch > 1f
                && particle.Velocity.sqrMagnitude > StillSpeed)
                return particle.Velocity.normalized;

            return new Vector2(Mathf.Cos(particle.Rotation), Mathf.Sin(particle.Rotation));
        }

        private void AddQuad(VertexHelper vh, in UIParticle particle, Vector4 uv, Color32 own)
        {
            float life = particle.Age / particle.Lifetime;
            float halfSize = Mathf.Lerp(particle.StartSize, particle.EndSize, life) * Half;
            Color32 over = _colors[Mathf.Min((int)(life * ColorSteps), ColorSteps - 1)];
            Color32 tint = new(
                MultiplyChannel(MultiplyChannel(over.r, particle.Tint.r), own.r),
                MultiplyChannel(MultiplyChannel(over.g, particle.Tint.g), own.g),
                MultiplyChannel(MultiplyChannel(over.b, particle.Tint.b), own.b),
                MultiplyChannel(MultiplyChannel(over.a, particle.Tint.a), own.a));

            Vector2 along = GetDirection(particle);
            Vector2 right = along * (halfSize * stretch);
            Vector2 up = new Vector2(-along.y, along.x) * halfSize;
            Vector2 center = particle.Position;
            int first = vh.currentVertCount;

            vh.AddVert(center - right - up, tint, new Vector2(uv.x, uv.y));
            vh.AddVert(center - right + up, tint, new Vector2(uv.x, uv.w));
            vh.AddVert(center + right + up, tint, new Vector2(uv.z, uv.w));
            vh.AddVert(center + right - up, tint, new Vector2(uv.z, uv.y));
            vh.AddTriangle(first, first + 1, first + 2);
            vh.AddTriangle(first + 2, first + 3, first);
        }
    }
}