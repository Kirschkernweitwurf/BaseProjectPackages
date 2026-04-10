using System;
using UnityEngine;
// ReSharper disable MemberCanBePrivate.Global

namespace Systems.Tweening.Core.Data
{
    /// <summary>
    /// Provides easing function delegates for tween interpolation.
    /// See https://easings.net/ for visualizations of these functions.
    /// </summary>
    public static class Easings
    {
        /// <summary>
        /// Gets the easing function corresponding to the specified <see cref="EEasingType"/>.
        /// </summary>
        /// <param name="type">The type of easing function to retrieve.</param>
        /// <returns><see cref="Func{T, TResult}"/> delegate representing the easing function.</returns>
        public static Func<float, float> Get(EEasingType type)
        {
            return type switch
            {
                EEasingType.Linear => Linear,
                EEasingType.EaseInQuad => EaseInQuad,
                EEasingType.EaseOutQuad => EaseOutQuad,
                EEasingType.EaseInOutQuad => EaseInOutQuad,
                EEasingType.EaseOutBack => EaseOutBack,
                EEasingType.EaseInBounce => EaseInBounce,
                EEasingType.EaseOutBounce => EaseOutBounce,
                EEasingType.EaseInExpo => EaseInExpo,
                EEasingType.EaseOutExpo => EaseOutExpo,
                EEasingType.EaseInOut => EaseInOut,
                EEasingType.EaseInOutCubic => EaseInOutCubic,
                EEasingType.EaseInOutExpo => EaseInOutExpo,
                EEasingType.EaseInElastic => EaseInElastic,
                EEasingType.EaseOutElastic => EaseOutElastic,
                _ => Linear
            };
        }

        public static float Linear(float t) => t;

        public static float EaseInQuad(float t) => t * t;

        public static float EaseOutQuad(float t) => t * (2f - t);

        public static float EaseInOutQuad(float t)
        {
            return t < 0.5f
                ? 2f * t * t
                : -1f + (4f - 2f * t) * t;
        }

        public static float EaseInExpo(float t)
        {
            return t == 0f
                ? 0f
                : Mathf.Pow(2f, 10f * (t - 1f));
        }

        public static float EaseOutExpo(float t)
        {
            return Mathf.Approximately(t, 1f)
                ? 1f
                : 1f - Mathf.Pow(2f, -10f * t);
        }

        public static float EaseInOutExpo(float t)
        {
            if (t == 0f)
                return 0f;

            if (Mathf.Approximately(t, 1f))
                return 1f;

            return t < 0.5f
                ? Mathf.Pow(2f, 20f * t - 10f) / 2f
                : (2f - Mathf.Pow(2f, -20f * t + 10f)) / 2f;
        }

        public static float EaseInOut(float t) => t * t * (3f - 2f * t);

        public static float EaseInOutCubic(float t)
        {
            return t < 0.5f
                ? 4f * t * t * t
                : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
        }

        public static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        public static float EaseInElastic(float t)
        {
            if (t == 0f)
                return 0f;

            if (Mathf.Approximately(t, 1f))
                return 1f;

            const float c4 = 2f * Mathf.PI / 3f;
            return -Mathf.Pow(2f, 10f * t - 10f) * Mathf.Sin((t * 10f - 10.75f) * c4);
        }

        public static float EaseOutElastic(float t)
        {
            if (t == 0f)
                return 0f;

            if (Mathf.Approximately(t, 1f))
                return 1f;

            const float c4 = 2f * Mathf.PI / 3f;
            return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
        }

        /// <summary>
        /// Easing out with a bouncing motion.
        /// </summary>
        public static float EaseOutBounce(float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;

            switch (t)
            {
                case < 1f / d1:
                    return n1 * t * t;
                case < 2f / d1:
                    t -= 1.5f / d1;
                    return n1 * t * t + 0.75f;
                case < 2.5f / d1:
                    t -= 2.25f / d1;
                    return n1 * t * t + 0.9375f;
                default:
                    t -= 2.625f / d1;
                    return n1 * t * t + 0.984375f;
            }
        }

        /// <summary>
        /// Easing in with a bouncing motion.
        /// </summary>
        public static float EaseInBounce(float t) => 1f - EaseOutBounce(1f - t);
    }
}