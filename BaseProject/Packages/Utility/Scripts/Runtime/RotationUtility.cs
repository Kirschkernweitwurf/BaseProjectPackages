using UnityEngine;

namespace Base.UtilityPackage
{
    /// <summary>
    /// Utility methods for working with rotations and angles.
    /// </summary>
    public static class RotationUtility
    {
        private const float FullCircle = 360f;
        private const float HalfCircle = 180f;
        private const float RotationDotThreshold = 0.9999f;

        /// <summary>
        /// Normalizes an angle to [-180, 180] degrees.
        /// </summary>
        /// <param name="angle">The angle in degrees.</param>
        /// <returns>The normalized angle.</returns>
        public static float NormalizeAngle(float angle)
        {
            angle %= FullCircle;

            if (angle > HalfCircle)
                angle -= FullCircle;

            if (angle < -HalfCircle)
                angle += FullCircle;

            return angle;
        }

        /// <summary>
        /// Compares two rotations for near equality using dot product precision.
        /// </summary>
        /// <param name="a">The first rotation.</param>
        /// <param name="b">The second rotation.</param>
        /// <returns>True if both rotations point the same way; otherwise, false.</returns>
        public static bool ApproximatelyEqual(Quaternion a, Quaternion b)
            => Mathf.Abs(Quaternion.Dot(a, b)) > RotationDotThreshold;
    }
}
