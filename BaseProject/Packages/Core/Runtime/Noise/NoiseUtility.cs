using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.CorePackage.Noise
{
    /// <summary>
    /// Layered Perlin noise. Unity ships a single smooth two dimensional layer with no seed and no
    /// shaping. This stacks layers on top of it, shapes them into ridges or turbulence, shifts the
    /// sample point by a seed derived offset and normalizes the result back into 0 to 1.
    /// </summary>
    public static class NoiseUtility
    {
        private const string MissingSettingsFormat = "{0} was called without settings.";
        private const int SliceCount = 6;

        // One dimensional noise is a horizontal line cut out of the two dimensional field. The line
        // is placed away from a whole coordinate on purpose, because Perlin returns exactly one
        // half all along a grid line. The line moves per layer, so the layers do not correlate.
        private const float SlicePosition = 0.372f;

        // Returned instead of a fresh allocation on the paths that have nothing to fill. Safe to
        // hand out because a grid with no cells cannot be written to.
        private static readonly float[,] EmptyMap = new float[0, 0];

        /// <summary>Samples a pattern along a single axis.</summary>
        /// <param name="x">The position to sample at.</param>
        /// <param name="settings">The pattern to sample.</param>
        /// <returns>A value from 0 up to the pattern's amplitude, or 0 when settings are missing.</returns>
        public static float Sample(float x, NoiseSettings settings)
        {
            if (!IsValid(settings, nameof(Sample)))
                return 0f;

            return Fractal(new Vector3(x, SlicePosition, 0f), settings.Offset, settings, useThirdAxis: false);
        }

        /// <summary>Samples a pattern on a plane.</summary>
        /// <param name="point">The position to sample at.</param>
        /// <param name="settings">The pattern to sample.</param>
        /// <returns>A value from 0 up to the pattern's amplitude, or 0 when settings are missing.</returns>
        public static float Sample(Vector2 point, NoiseSettings settings)
        {
            if (!IsValid(settings, nameof(Sample)))
                return 0f;

            return Fractal(new Vector3(point.x, point.y, 0f), settings.Offset, settings, useThirdAxis: false);
        }

        /// <summary>Samples a pattern in space.</summary>
        /// <param name="point">The position to sample at.</param>
        /// <param name="settings">The pattern to sample.</param>
        /// <returns>A value from 0 up to the pattern's amplitude, or 0 when settings are missing.</returns>
        public static float Sample(Vector3 point, NoiseSettings settings)
        {
            if (!IsValid(settings, nameof(Sample)))
                return 0f;

            return Fractal(point, settings.Offset, settings, useThirdAxis: true);
        }

        /// <summary>
        /// Fills a grid with one sample per cell, indexed by column and then row. Handy for height
        /// maps, spawn masks and anything else that wants the whole field at once.
        /// </summary>
        /// <param name="width">The number of columns.</param>
        /// <param name="height">The number of rows.</param>
        /// <param name="settings">The pattern to sample.</param>
        /// <returns>The filled grid, or an empty grid when the size or the settings are unusable.</returns>
        public static float[,] CreateMap(int width, int height, NoiseSettings settings)
        {
            if (!IsValid(settings, nameof(CreateMap))
                || width <= 0
                || height <= 0)
                return EmptyMap;

            float[,] map = new float[width, height];

            // Goes to the layered sampler directly rather than through Sample, so the settings
            // check and the offset lookup happen once for the map instead of once per cell.
            Vector3 offset = settings.Offset;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                    map[x, y] = Fractal(new Vector3(x, y, 0f), offset, settings, useThirdAxis: false);
            }

            return map;
        }

        /// <summary>
        /// Three dimensional Perlin noise, assembled from the two dimensional generator Unity ships
        /// by averaging every pair of coordinates in both orders.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Averaging six samples pulls the result hard toward the middle of the range, so this has
        /// far less contrast than a single two dimensional sample. Expect a soft pattern and
        /// stretch the result yourself when hard extremes are wanted.
        /// </para>
        /// <para>
        /// That also hits the shaping: <see cref="ENoiseType.Ridged"/> maps the middle to the top
        /// and <see cref="ENoiseType.Turbulence"/> maps it to the bottom, so both come out pressed
        /// against one end in three dimensions. Shape in two dimensions where the look matters.
        /// </para>
        /// </remarks>
        /// <param name="x">The first coordinate.</param>
        /// <param name="y">The second coordinate.</param>
        /// <param name="z">The third coordinate.</param>
        /// <returns>A value from 0 to 1.</returns>
        public static float Perlin3D(float x, float y, float z)
        {
            float total = Mathf.PerlinNoise(x, y)
                + Mathf.PerlinNoise(y, z)
                + Mathf.PerlinNoise(z, x)
                + Mathf.PerlinNoise(y, x)
                + Mathf.PerlinNoise(z, y)
                + Mathf.PerlinNoise(x, z);

            return total / SliceCount;
        }

        private static bool IsValid(NoiseSettings settings, string caller)
        {
            if (settings != null)
                return true;

            CustomLogger.LogError(string.Format(MissingSettingsFormat, caller), null);

            return false;
        }

        // Each layer samples finer and contributes less. The sum is divided by what the layers
        // actually contributed rather than by their count, so changing persistence moves the detail
        // around without moving the overall brightness with it.
        private static float Fractal(Vector3 point, Vector3 offset, NoiseSettings settings, bool useThirdAxis)
        {
            // Read once up front rather than per layer: a full map runs this loop for every cell.
            ENoiseType noiseType = settings.NoiseType;
            int octaves = settings.Octaves;
            float lacunarity = settings.Lacunarity;
            float persistence = settings.Persistence;

            float amplitude = 1f;
            float frequency = settings.Frequency;
            float total = 0f;
            float normalization = 0f;

            for (int octave = 0; octave < octaves; octave++)
            {
                Vector3 sample = point * frequency + offset;

                float raw = useThirdAxis
                    ? Perlin3D(sample.x, sample.y, sample.z)
                    : Mathf.PerlinNoise(sample.x, sample.y);

                total += Shape(Mathf.Clamp01(raw), noiseType) * amplitude;
                normalization += amplitude;

                amplitude *= persistence;
                frequency *= lacunarity;
            }

            if (normalization <= 0f)
                return 0f;

            return total / normalization * settings.Amplitude;
        }

        private static float Shape(float raw, ENoiseType noiseType) => noiseType switch
        {
            ENoiseType.Ridged => 1f - Mathf.Abs(raw * 2f - 1f),
            ENoiseType.Turbulence => Mathf.Abs(raw * 2f - 1f),
            _ => raw
        };
    }
}