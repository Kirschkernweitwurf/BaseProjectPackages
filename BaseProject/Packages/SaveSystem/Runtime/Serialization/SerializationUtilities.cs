using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.SaveSystemPackage.Serialization
{
    /// <summary>
    /// Converts Unity types that <see cref="JsonUtility"/> cannot round-trip cleanly into plain arrays
    /// and back.
    /// </summary>
    public static class SerializationUtilities
    {
        private const int VectorLength = 3;

        /// <summary>
        /// Flattens a vector into a serializable float array.
        /// </summary>
        /// <param name="vector">The vector to flatten.</param>
        /// <returns>An array holding x, y and z in that order.</returns>
        public static float[] ToArray(Vector3 vector) => new[]
        {
            vector.x,
            vector.y,
            vector.z
        };

        /// <summary>
        /// Rebuilds a vector from a float array.
        /// </summary>
        /// <param name="values">Exactly three elements, as produced by <see cref="ToArray"/>.</param>
        /// <returns>The rebuilt vector, or zero when the array has the wrong shape.</returns>
        public static Vector3 ToVector3(float[] values)
        {
            if (values != null && values.Length == VectorLength)
                return new Vector3(values[0], values[1], values[2]);

            CustomLogger.LogWarning($"A float array needs exactly {VectorLength} elements to become a "
                + $"{nameof(Vector3)}. Returning {nameof(Vector3)}.{nameof(Vector3.zero)}.", null);

            return Vector3.zero;
        }
    }
}