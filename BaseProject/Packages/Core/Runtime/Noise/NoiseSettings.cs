using System;
using Base.AttributesPackage;
using Base.UtilityPackage.Randomization;
using UnityEngine;

namespace Base.CorePackage.Noise
{
    /// <summary>
    /// The shape of one noise pattern: how coarse it is, how many layers it stacks, how it is
    /// shaped and which seed it sits at. Serializable, so a pattern is tuned in the inspector and
    /// passed around as a single object instead of a handful of loose floats.
    /// </summary>
    [Serializable]
    public sealed class NoiseSettings
    {
        private const float DefaultAmplitude = 1f;
        private const float DefaultFrequency = 1f;
        private const float DefaultLacunarity = 2f;
        private const int DefaultOctaves = 1;
        private const float DefaultPersistence = 0.5f;
        private const int MaxOctaves = 8;

        // Perlin noise has no seed, so a seed becomes a position in the field instead. The range is
        // kept modest on purpose: further out a float can no longer tell neighboring samples apart
        // and the finest layers turn into steps.
        private const float MaxOffset = 1000f;

        private const float MaxPersistence = 1f;
        private const float MinFrequency = 0.0001f;
        private const float MinLacunarity = 1f;
        private const int MinOctaves = 1;
        private const float MinPersistence = 0f;

        [field: Title("Shape")]
        [field: Tooltip("How the raw samples are shaped. Perlin is plain rolling noise, Ridged carves sharp"
            + " crests for mountains, Turbulence creases the pattern for smoke and marble.")]
        [field: SerializeField] public ENoiseType NoiseType { get; private set; }

        [field: Tooltip("How fast the pattern changes across the sample space. Higher is finer and busier,"
            + " lower is broader and smoother.")]
        [field: Min(MinFrequency)]
        [field: SerializeField] public float Frequency { get; private set; } = DefaultFrequency;

        [field: Tooltip("How many layers are stacked. Each one is finer and quieter than the last, which is"
            + " what turns a smooth blob into something with detail. Costs one sample per layer.")]
        [field: MinMax(MinOctaves, MaxOctaves)]
        [field: SerializeField] public int Octaves { get; private set; } = DefaultOctaves;

        [field: Tooltip("How much finer each layer is than the one before it. 2 doubles the detail per layer.")]
        [field: Min(MinLacunarity)]
        [field: SerializeField] public float Lacunarity { get; private set; } = DefaultLacunarity;

        [field: Tooltip("How much quieter each layer is than the one before it. Low keeps the broad shape in"
            + " charge, high lets the fine detail take over.")]
        [field: MinMax(MinPersistence, MaxPersistence)]
        [field: SerializeField] public float Persistence { get; private set; } = DefaultPersistence;

        [field: Title("Output")]
        [field: Tooltip("Scales the result. A sample runs from 0 up to this value.")]
        [field: SerializeField] public float Amplitude { get; private set; } = DefaultAmplitude;

        [field: Title("Seed")]
        [field: Tooltip("Picks which part of the noise field is sampled. The same seed always gives the same"
            + " pattern, a different one gives an unrelated pattern of the same character.")]
        [field: SerializeField] public int Seed { get; private set; }

        /// <summary>
        /// Where in the noise field this pattern is sampled, derived from <see cref="Seed"/>. Built
        /// again whenever the seed changes, which is what makes an edit in the inspector show up
        /// without anything having to notice it.
        /// </summary>
        public Vector3 Offset
        {
            get
            {
                if (_hasOffset
                    && _offsetSeed == Seed)
                    return _offset;

                _offset = BuildOffset(Seed);
                _offsetSeed = Seed;
                _hasOffset = true;

                return _offset;
            }
        }

        private Vector3 _offset;
        private int _offsetSeed;
        private bool _hasOffset;

        /// <summary>
        /// Creates settings with the default shape at seed zero. Declared explicitly because
        /// Unity's serializer builds instances through the parameterless constructor.
        /// </summary>
        public NoiseSettings() { }

        /// <summary>Creates plain gradient noise with the values that get tuned most often.</summary>
        /// <param name="seed">The part of the noise field to sample.</param>
        /// <param name="frequency">How fast the pattern changes across the sample space.</param>
        /// <param name="octaves">How many layers are stacked.</param>
        public NoiseSettings(int seed, float frequency, int octaves) :
            this(seed, frequency, octaves, ENoiseType.Perlin) { }

        /// <summary>
        /// Creates settings with the values that get tuned most often. Frequency and octaves are
        /// clamped to the same range the inspector offers, so code and inspector cannot disagree.
        /// </summary>
        /// <param name="seed">The part of the noise field to sample.</param>
        /// <param name="frequency">How fast the pattern changes across the sample space.</param>
        /// <param name="octaves">How many layers are stacked. Clamped to at most eight.</param>
        /// <param name="noiseType">How the raw samples are shaped.</param>
        public NoiseSettings(int seed, float frequency, int octaves, ENoiseType noiseType)
        {
            Seed = seed;
            Frequency = Mathf.Max(MinFrequency, frequency);
            Octaves = Mathf.Clamp(octaves, MinOctaves, MaxOctaves);
            NoiseType = noiseType;
        }

        /// <summary>Moves the pattern to a different seed at runtime.</summary>
        /// <param name="seed">The new seed.</param>
        public void SetSeed(int seed) => Seed = seed;

        /// <summary>Samples the pattern along a single axis.</summary>
        /// <param name="x">The position to sample at.</param>
        /// <returns>A value from 0 up to <see cref="Amplitude"/>.</returns>
        public float Evaluate(float x) => NoiseUtility.Sample(x, this);

        /// <summary>Samples the pattern on a plane.</summary>
        /// <param name="point">The position to sample at.</param>
        /// <returns>A value from 0 up to <see cref="Amplitude"/>.</returns>
        public float Evaluate(Vector2 point) => NoiseUtility.Sample(point, this);

        /// <summary>Samples the pattern in space.</summary>
        /// <param name="point">The position to sample at.</param>
        /// <returns>A value from 0 up to <see cref="Amplitude"/>.</returns>
        public float Evaluate(Vector3 point) => NoiseUtility.Sample(point, this);

        private static Vector3 BuildOffset(int seed)
        {
            SeededRandom random = new(seed);

            return new Vector3(random.Range(-MaxOffset, MaxOffset), random.Range(-MaxOffset, MaxOffset),
                random.Range(-MaxOffset, MaxOffset));
        }
    }
}