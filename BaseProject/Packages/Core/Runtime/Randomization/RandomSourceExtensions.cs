using System.Collections.Generic;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.CorePackage.Randomization
{
    /// <summary>
    /// Everything drawn from an <see cref="IRandomSource"/>: ranges, chances, shuffles, points on
    /// circles and spheres and normally distributed values. Written once against the raw bits, so
    /// a source only has to supply <see cref="IRandomSource.NextUInt"/> to get all of it.
    /// </summary>
    public static class RandomSourceExtensions
    {
        private const string EmptyListFormat = "{0} was called with an empty list.";
        private const int FloatShift = 8;
        private const float FloatUnit = 1f / (1 << 24);
        private const int HighBit = 31;

        // Both loops below throw a draw away and try again, and both accept well over half of what
        // they see, so a working source clears them in a handful of tries. A source that never
        // varies would otherwise spin here forever and freeze the editor, so they give up instead.
        private const int MaxAttempts = 64;

        private const string NullListFormat = "{0} was called with a null list.";
        private const string NullSourceFormat = "{0} was called on a null {1}.";
        private const float SphereExponent = 1f / 3f;
        private const string StuckSourceFormat = "{0} gave up after {1} draws. The {2} is not producing varied "
            + "values.";
        private const float TwoPi = Mathf.PI * 2f;

        /// <summary>Draws a value from 0 inclusive to 1 exclusive.</summary>
        /// <param name="source">The generator to draw from.</param>
        /// <returns>The drawn value, or 0 when the source is missing.</returns>
        public static float NextFloat(this IRandomSource source)
        {
            if (!IsValid(source, nameof(NextFloat)))
                return 0f;

            // Only the top 24 bits are used, which is exactly what a float can tell apart. Using
            // all 32 would make some values round onto their neighbors and come up twice as often.
            return (source.NextUInt() >> FloatShift) * FloatUnit;
        }

        /// <summary>Draws a value from the lower bound up to but excluding the upper one.</summary>
        /// <param name="source">The generator to draw from.</param>
        /// <param name="minInclusive">The lowest value that can come out.</param>
        /// <param name="maxExclusive">The bound the result stays below.</param>
        /// <returns>The drawn value, or the lower bound when the range is empty.</returns>
        public static float Range(this IRandomSource source, float minInclusive, float maxExclusive)
        {
            if (maxExclusive <= minInclusive)
                return minInclusive;

            return minInclusive + source.NextFloat() * (maxExclusive - minInclusive);
        }

        /// <summary>Draws an integer from the lower bound up to but excluding the upper one.</summary>
        /// <param name="source">The generator to draw from.</param>
        /// <param name="minInclusive">The lowest value that can come out.</param>
        /// <param name="maxExclusive">The bound the result stays below.</param>
        /// <returns>The drawn value, or the lower bound when the range is empty.</returns>
        public static int Range(this IRandomSource source, int minInclusive, int maxExclusive)
        {
            if (!IsValid(source, nameof(Range)))
                return minInclusive;

            if (maxExclusive <= minInclusive)
                return minInclusive;

            unchecked
            {
                return minInclusive + (int)NextBelow(source, (uint)(maxExclusive - minInclusive));
            }
        }

        /// <summary>Draws true or false with an even chance.</summary>
        /// <param name="source">The generator to draw from.</param>
        /// <returns>The drawn value, or false when the source is missing.</returns>
        public static bool NextBool(this IRandomSource source)
        {
            if (!IsValid(source, nameof(NextBool)))
                return false;

            return (source.NextUInt() >> HighBit) != 0u;
        }

        /// <summary>Draws minus one or one with an even chance.</summary>
        /// <param name="source">The generator to draw from.</param>
        /// <returns>Either minus one or one.</returns>
        public static int NextSign(this IRandomSource source) => source.NextBool()
            ? 1
            : -1;

        /// <summary>Draws true with the given probability.</summary>
        /// <param name="source">The generator to draw from.</param>
        /// <param name="probability">The chance of a hit, from 0 for never to 1 for always.</param>
        /// <returns>True when the draw landed inside the probability.</returns>
        public static bool Chance(this IRandomSource source, float probability)
        {
            if (probability <= 0f)
                return false;

            if (probability >= 1f)
                return true;

            return source.NextFloat() < probability;
        }

        /// <summary>
        /// Draws from a normal distribution, so values near the mean come up far more often than
        /// values far from it. Use this for variation that should look natural rather than even.
        /// </summary>
        /// <param name="source">The generator to draw from.</param>
        /// <param name="mean">The value the results center on.</param>
        /// <param name="standardDeviation">How far the results typically stray from the mean.</param>
        /// <returns>The drawn value, or the mean when the source is missing.</returns>
        public static float NextGaussian(this IRandomSource source, float mean = 0f, float standardDeviation = 1f)
        {
            if (!IsValid(source, nameof(NextGaussian)))
                return mean;

            // Marsaglia polar method: draw points in a square until one lands inside the unit
            // circle, then reshape it. The second value it yields is dropped rather than cached, so
            // the result stays a pure function of the source and two calls never share a draw.
            for (int attempt = 0; attempt < MaxAttempts; attempt++)
            {
                float x = source.NextFloat() * 2f - 1f;
                float y = source.NextFloat() * 2f - 1f;
                float squared = x * x + y * y;

                if (squared >= 1f
                    || squared <= 0f)
                    continue;

                return mean + standardDeviation * x * Mathf.Sqrt(-2f * Mathf.Log(squared) / squared);
            }

            CustomLogger.LogError(string.Format(StuckSourceFormat, nameof(NextGaussian), MaxAttempts,
                nameof(IRandomSource)), null);

            return mean;
        }

        /// <summary>Picks one element with an even chance. Arrays bind here as well.</summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="source">The generator to draw from.</param>
        /// <param name="items">The list to pick from.</param>
        /// <returns>The picked element, or the default value when there is nothing to pick.</returns>
        public static T Pick<T>(this IRandomSource source, IReadOnlyList<T> items)
        {
            if (!IsValid(source, nameof(Pick)))
                return default(T);

            if (items == null)
            {
                CustomLogger.LogWarning(string.Format(NullListFormat, nameof(Pick)), null);
                return default(T);
            }

            if (items.Count == 0)
            {
                CustomLogger.LogWarning(string.Format(EmptyListFormat, nameof(Pick)), null);
                return default(T);
            }

            return items[source.Range(0, items.Count)];
        }

        /// <summary>Shuffles a list in place. Arrays bind here as well.</summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="source">The generator to draw from.</param>
        /// <param name="items">The list to shuffle.</param>
        public static void Shuffle<T>(this IRandomSource source, IList<T> items)
        {
            if (!IsValid(source, nameof(Shuffle)))
                return;

            if (items == null)
            {
                CustomLogger.LogWarning(string.Format(NullListFormat, nameof(Shuffle)), null);
                return;
            }

            // Fisher-Yates, walking from the back. Each element is swapped with one out of the part
            // that is not settled yet, which is what makes every ordering equally likely.
            for (int index = items.Count - 1; index > 0; index--)
            {
                int target = source.Range(0, index + 1);

                (items[index], items[target]) = (items[target], items[index]);
            }
        }

        /// <summary>Draws a point spread evenly along the edge of a unit circle.</summary>
        /// <param name="source">The generator to draw from.</param>
        /// <returns>A point one unit from the origin.</returns>
        public static Vector2 OnUnitCircle(this IRandomSource source)
        {
            float angle = source.NextFloat() * TwoPi;

            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        /// <summary>Draws a point spread evenly over the area of a unit circle.</summary>
        /// <param name="source">The generator to draw from.</param>
        /// <returns>A point no further than one unit from the origin.</returns>
        public static Vector2 InsideUnitCircle(this IRandomSource source)
        {
            // The square root is what spreads the points over the area. Without it every radius
            // would take the same share of the draws and the points would pile up in the middle.
            float radius = Mathf.Sqrt(source.NextFloat());

            return source.OnUnitCircle() * radius;
        }

        /// <summary>Draws a point spread evenly over the surface of a unit sphere.</summary>
        /// <param name="source">The generator to draw from.</param>
        /// <returns>A point one unit from the origin.</returns>
        public static Vector3 OnUnitSphere(this IRandomSource source)
        {
            // Height first, angle second. The surface of a sphere between two heights depends only
            // on the height difference, so an even draw for the height already spreads evenly.
            float height = source.NextFloat() * 2f - 1f;
            float angle = source.NextFloat() * TwoPi;
            float radius = Mathf.Sqrt(Mathf.Max(0f, 1f - height * height));

            return new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, height);
        }

        /// <summary>Draws a point spread evenly through the volume of a unit sphere.</summary>
        /// <param name="source">The generator to draw from.</param>
        /// <returns>A point no further than one unit from the origin.</returns>
        public static Vector3 InsideUnitSphere(this IRandomSource source)
        {
            float radius = Mathf.Pow(source.NextFloat(), SphereExponent);

            return source.OnUnitSphere() * radius;
        }

        // Rejection sampling. The raw range rarely divides evenly into the target range, so the
        // values in the incomplete last block are thrown away rather than folded back in, which
        // would otherwise make the lowest outcomes come up slightly more often than the rest.
        private static uint NextBelow(IRandomSource source, uint span)
        {
            if (span == 0u)
                return 0u;

            uint threshold = unchecked(0u - span) % span;
            uint value = 0u;

            for (int attempt = 0; attempt < MaxAttempts; attempt++)
            {
                value = source.NextUInt();

                if (value >= threshold)
                    return value % span;
            }

            CustomLogger.LogError(string.Format(StuckSourceFormat, nameof(Range), MaxAttempts,
                nameof(IRandomSource)), null);

            return value % span;
        }

        private static bool IsValid(IRandomSource source, string caller)
        {
            if (source != null)
                return true;

            CustomLogger.LogError(string.Format(NullSourceFormat, caller, nameof(IRandomSource)), null);

            return false;
        }
    }
}