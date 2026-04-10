using UnityEngine;

namespace Utility
{
    /// <summary>
    /// Utility methods for working with rotations and angles.
    /// </summary>
    public static class RotationUtility
    {
        /// <summary>
        /// Normalizes an angle to [-180, 180] degrees.
        /// </summary>
        public static float NormalizeAngle(float angle)
        {
            angle %= 360f;

            if (angle > 180f)
                angle -= 360f;

            if (angle < -180f)
                angle += 360f;

            return angle;
        }

        /// <summary>
        /// Compares two rotations for near equality using dot product precision.
        /// </summary>
        public static bool ApproximatelyEqual(Quaternion a, Quaternion b)
        {
            return Mathf.Abs(Quaternion.Dot(a, b)) > 0.9999f;
        }
    }
}