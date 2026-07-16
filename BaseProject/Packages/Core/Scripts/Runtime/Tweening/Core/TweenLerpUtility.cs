using System;
using UnityEngine;

namespace Base.CorePackage.Tweening.Core
{
    /// <summary>
    /// Unclamped linear interpolation helpers for common Unity types.
    /// These functions intentionally do not clamp <c>t</c> to support overshooting easings.
    /// </summary>
    public static class TweenLerpUtility
    {
        private static readonly Func<float, float, float, float> FloatLerp = LerpFloatUnclamped;
        private static readonly Func<Vector2, Vector2, float, Vector2> Vector2Lerp = LerpVector2Unclamped;
        private static readonly Func<Vector3, Vector3, float, Vector3> Vector3Lerp = LerpVector3Unclamped;
        private static readonly Func<Color, Color, float, Color> ColorLerp = LerpColorUnclamped;
        private static readonly Func<Quaternion, Quaternion, float, Quaternion> QuaternionLerp
            = LerpQuaternionUnclamped;

        /// <summary>
        /// Unclamped linear interpolation for <see cref="float"/>.
        /// </summary>
        public static float LerpFloatUnclamped(float a, float b, float t) => a + (b - a) * t;

        /// <summary>
        /// Unclamped linear interpolation for <see cref="Vector2"/>.
        /// </summary>
        public static Vector2 LerpVector2Unclamped(Vector2 a, Vector2 b, float t) => a + (b - a) * t;

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
            => Quaternion.LerpUnclamped(a, b, t);

        /// <summary>
        /// Returns the matching unclamped interpolation function for <typeparamref name="T"/>, or
        /// <c>null</c> if the type is not supported out of the box. Reflection free, so it stays
        /// IL2CPP safe.
        /// </summary>
        /// <typeparam name="T">The value type being tweened.</typeparam>
        public static Func<T, T, float, T> Resolve<T>()
        {
            if (typeof(T) == typeof(float))
                return (Func<T, T, float, T>)(object)FloatLerp;

            if (typeof(T) == typeof(Vector2))
                return (Func<T, T, float, T>)(object)Vector2Lerp;

            if (typeof(T) == typeof(Vector3))
                return (Func<T, T, float, T>)(object)Vector3Lerp;

            if (typeof(T) == typeof(Color))
                return (Func<T, T, float, T>)(object)ColorLerp;

            if (typeof(T) == typeof(Quaternion))
                return (Func<T, T, float, T>)(object)QuaternionLerp;

            return null;
        }
    }
}