using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.SaveSystemPackage.Savable
{
    /// <summary>
    /// Utility class for converting non-serializable types to serializable formats and vice versa.
    /// </summary>
    public static class SerializationUtilities
    {
        /// <summary>
        /// Converts a Vector3 to a float array.
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static float[] ToArray(Vector3 vec) => new[]
        {
            vec.x,
            vec.y,
            vec.z
        };

        /// <summary>
        /// Converts a float array to a Vector3.
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static Vector3 ToVector3(float[] arr)
        {
            if (arr is
                {
                    Length: 3
                })
                return new Vector3(arr[0], arr[1], arr[2]);

            CustomLogger.Log("Array must have exactly 3 elements to convert to Vector3. Returning Vector3.zero", null);
            return Vector3.zero;
        }
    }
}