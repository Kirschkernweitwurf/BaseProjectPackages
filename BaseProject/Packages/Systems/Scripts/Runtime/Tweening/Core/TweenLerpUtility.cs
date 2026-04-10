using UnityEngine;

namespace Systems.Tweening.Core
{
    /// <summary>
    /// Unclamped linear interpolation helpers for common Unity types.
    /// These functions intentionally do not clamp <c>t</c> to support overshooting easings.
    /// </summary>
    public static class TweenLerpUtility
    {
        /// <summary>
        /// Unclamped linear interpolation for <see cref="float"/>.
        /// </summary>
        public static float LerpFloatUnclamped(float a, float b, float t) => a + (b - a) * t;

        /// <summary>
        /// Unclamped linear interpolation for <see cref="Vector3"/>.
        /// </summary>
        public static Vector3 LerpVector3Unclamped(Vector3 a, Vector3 b, float t) => a + (b - a) * t;

        /// <summary>
        /// Unclamped linear interpolation for <see cref="Color"/>.
        /// </summary>
        public static Color LerpColorUnclamped(Color a, Color b, float t) => a + (b - a) * t;

        /// <summary>
        /// Unclamped spherical-linear interpolation for <see cref="Quaternion"/>.
        /// Uses Unity's <see cref="Quaternion.LerpUnclamped(Quaternion, Quaternion, float)"/>.
        /// </summary>
        public static Quaternion LerpQuaternionUnclamped(Quaternion a, Quaternion b, float t)
        {
            return Quaternion.LerpUnclamped(a, b, t);
        }
    }
}