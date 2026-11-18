using System.Collections.Generic;
using Base.AttributesPackage;
using UnityEngine;
using UnityEngine.UI;

namespace Base.UIPackage.Effects
{
    /// <summary>
    /// Sparkles for rewarding screens: a field of stars that twinkle all over an area while enabled, plus bursts
    /// that spray outward from a point on demand. Runs on unscaled time, because the screens using it often pause
    /// the game. The sparkles are pooled images animated here in one loop instead of one tween each, so dozens of them
    /// cost little and never fill up the tween pool.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UISparkles : MonoBehaviour
    {
        private const float BurstJitter = 0.5f;
        private const float FullTurn = 360f;
        private const float HalfTurn = 180f;

        [Title("References")]
        [Required]
        [Tooltip("The area the twinkles appear in. Bursts spawn here too, so it should cover the whole screen.")]
        [SerializeField] private RectTransform area;

        [Required]
        [Tooltip("Image cloned for every sparkle. Its sprite and material set the look; the color is rolled.")]
        [SerializeField] private Image sparklePrefab;

        [Tooltip("Colors the sparkles are tinted in, rolled per sparkle.")]
        [SerializeField] private Color[] colors = { Color.white };

        [Title("Twinkle")]
        [Min(0)]
        [Tooltip("How many sparkles twinkle at the same time while enabled.")]
        [SerializeField] private int twinkleCount = 28;

        [Tooltip("Smallest and largest size of a twinkle in pixels.")]
        [SerializeField] private Vector2 twinkleSize = new(14f, 42f);

        [Min(0.01f)]
        [Tooltip("Seconds one twinkle takes to grow and fade again.")]
        [SerializeField] private float twinkleDuration = 1.1f;

        [Title("Burst")]
        [Min(0)]
        [Tooltip("Sparkles sprayed by one burst.")]
        [SerializeField] private int burstCount = 18;

        [Tooltip("Smallest and largest size of a burst sparkle in pixels.")]
        [SerializeField] private Vector2 burstSize = new(20f, 56f);

        [Tooltip("Smallest and largest distance a burst sparkle flies in pixels.")]
        [SerializeField] private Vector2 burstDistance = new(120f, 360f);

        [Min(0.01f)]
        [Tooltip("Seconds a burst sparkle flies before it has faded out.")]
        [SerializeField] private float burstDuration = 0.8f;

        private readonly List<SparkleParticle> _live = new();
        private readonly Stack<Image> _pool = new();

#region Unity Callbacks
        private void OnEnable()
        {
            // Spread over one twinkle length, so the field fades in instead of popping all at once.
            for (int index = 0; index < twinkleCount; index++)
                _live.Add(NewTwinkle(Take(), -Random.value * twinkleDuration));
        }

        private void Update()
        {
            float deltaTime = Time.unscaledDeltaTime;

            // Backwards, so finished bursts can be removed while looping.
            for (int index = _live.Count - 1; index >= 0; index--)
            {
                SparkleParticle sparkle = _live[index];
                sparkle.Age += deltaTime;

                if (sparkle.Age >= sparkle.Lifetime)
                {
                    if (sparkle.IsTwinkle)
                        sparkle = NewTwinkle(sparkle.Image, 0f);
                    else
                    {
                        Release(sparkle.Image);
                        _live.RemoveAt(index);
                        continue;
                    }
                }

                Show(sparkle);
                _live[index] = sparkle;
            }
        }

        private void OnDisable()
        {
            foreach (SparkleParticle sparkle in _live)
                Release(sparkle.Image);

            _live.Clear();
        }
#endregion

        /// <summary>Sprays a burst of sparkles outward from <paramref name="worldPosition"/>.</summary>
        public void Burst(Vector3 worldPosition)
        {
            if (!isActiveAndEnabled)
                return;

            Vector2 center = area.InverseTransformPoint(worldPosition);

            for (int index = 0; index < burstCount; index++)
                _live.Add(NewBurst(Take(), center, index));
        }

        /// <summary>
        /// Sprays a burst at <paramref name="normalizedPosition"/> inside the area, where (0, 0) is the bottom left
        /// corner and (1, 1) the top right one.
        /// </summary>
        public void BurstAt(Vector2 normalizedPosition)
        {
            Rect bounds = area.rect;
            Vector2 local = bounds.min + Vector2.Scale(bounds.size, normalizedPosition);
            Burst(area.TransformPoint(local));
        }

        private static float Roll(Vector2 range) => Random.Range(range.x, range.y);

        private static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);

        // A twinkle grows and shrinks on a sine arc; a burst sparkle starts full and shrinks as it flies out.
        private static void Show(SparkleParticle sparkle)
        {
            RectTransform rect = sparkle.Image.rectTransform;
            float t = Mathf.Clamp01(sparkle.Age / sparkle.Lifetime);

            float size = sparkle.IsTwinkle
                ? Mathf.Sin(t * Mathf.PI)
                : 1f - t * t * t;

            float travel = sparkle.IsTwinkle
                ? 0f
                : EaseOutCubic(t);

            rect.anchoredPosition = Vector2.LerpUnclamped(sparkle.From, sparkle.To, travel);
            rect.localScale = Vector3.one * (sparkle.Scale * size);
            rect.localEulerAngles = new Vector3(0f, 0f, sparkle.StartAngle + sparkle.Spin * t);
        }

        /// <summary>
        /// A twinkle at a random spot. A negative <paramref name="startAge"/> delays it by that many seconds.
        /// </summary>
        private SparkleParticle NewTwinkle(Image image, float startAge)
        {
            Rect bounds = area.rect;
            Vector2 spot = new(Random.Range(bounds.xMin, bounds.xMax), Random.Range(bounds.yMin, bounds.yMax));
            image.color = RollColor();

            SparkleParticle twinkle = new()
            {
                Image = image,
                From = spot,
                To = spot,
                Scale = Roll(twinkleSize) / image.rectTransform.rect.width,
                StartAngle = Random.Range(0f, FullTurn),
                Spin = HalfTurn,
                Age = startAge,
                Lifetime = twinkleDuration,
                IsTwinkle = true
            };

            Show(twinkle);

            return twinkle;
        }

        // Evenly spread angles with a little jitter, so a burst reads as a ring and never as a clump.
        private SparkleParticle NewBurst(Image image, Vector2 center, int index)
        {
            float angle = (index + Random.value * BurstJitter) / burstCount * Mathf.PI * 2f;
            Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));
            image.color = RollColor();

            SparkleParticle sparkle = new()
            {
                Image = image,
                From = center,
                To = center + direction * Roll(burstDistance),
                Scale = Roll(burstSize) / image.rectTransform.rect.width,
                StartAngle = Random.Range(0f, FullTurn),
                Spin = FullTurn,
                Lifetime = burstDuration
            };

            Show(sparkle);

            return sparkle;
        }

        private Color RollColor() => colors.Length == 0
            ? Color.white
            : colors[Random.Range(0, colors.Length)];

        private Image Take()
        {
            Image sparkle = _pool.Count > 0
                ? _pool.Pop()
                : Instantiate(sparklePrefab, area);

            sparkle.gameObject.SetActive(true);

            return sparkle;
        }

        private void Release(Image sparkle)
        {
            sparkle.gameObject.SetActive(false);
            _pool.Push(sparkle);
        }
    }
}