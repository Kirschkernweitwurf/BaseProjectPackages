using System;
using UnityEngine;

namespace Base.CorePackage.Tweening.Core.Data
{
    /// <summary>
    /// Provides easing function delegates for tween interpolation.
    /// See https://easings.net/ for visualizations of these functions.
    /// </summary>
    public static class Easings
    {
        /// <summary>
        /// Overshoot factor of the back easings, taken from the reference curves on easings.net.
        /// </summary>
        private const float BackOvershoot = 1.70158f;

        /// <summary>
        /// Return factor that pulls the back easing back onto its end value after the overshoot.
        /// </summary>
        private const float BackReturnFactor = BackOvershoot + 1f;

        /// <summary>
        /// Amplitude factor of the bounce easings.
        /// </summary>
        private const float BounceAmplitude = 7.5625f;

        /// <summary>
        /// Divisor that splits the bounce curve into its four segments.
        /// </summary>
        private const float BounceSegments = 2.75f;

        /// <summary>
        /// Period of the elastic easings, taken from the reference curves on easings.net.
        /// </summary>
        private const float ElasticPeriod = 2f * Mathf.PI / 3f;

        /// <summary>
        /// Gets the easing function corresponding to the specified <see cref="EEasingType"/>.
        /// </summary>
        /// <param name="type">The type of easing function to retrieve.</param>
        /// <returns>A delegate representing the easing function.</returns>
        public static Func<float, float> Get(EEasingType type) => type switch
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

        /// <summary>
        /// Constant speed from start to end.
        /// </summary>
        private static float Linear(float t) => t;

        /// <summary>
        /// Quadratic acceleration from a standstill.
        /// </summary>
        private static float EaseInQuad(float t) => t * t;

        /// <summary>
        /// Quadratic deceleration into the end value.
        /// </summary>
        private static float EaseOutQuad(float t) => t * (2f - t);

        /// <summary>
        /// Quadratic acceleration followed by quadratic deceleration.
        /// </summary>
        private static float EaseInOutQuad(float t) => t < 0.5f
            ? 2f * t * t
            : -1f + (4f - 2f * t) * t;

        /// <summary>
        /// Exponential acceleration from a standstill.
        /// </summary>
        private static float EaseInExpo(float t) => IsZero(t)
            ? 0f
            : Mathf.Pow(2f, 10f * (t - 1f));

        /// <summary>
        /// Exponential deceleration into the end value.
        /// </summary>
        private static float EaseOutExpo(float t) => IsOne(t)
            ? 1f
            : 1f - Mathf.Pow(2f, -10f * t);

        /// <summary>
        /// Exponential acceleration followed by exponential deceleration.
        /// </summary>
        private static float EaseInOutExpo(float t)
        {
            if (IsZero(t))
                return 0f;

            if (IsOne(t))
                return 1f;

            return t < 0.5f
                ? Mathf.Pow(2f, 20f * t - 10f) / 2f
                : (2f - Mathf.Pow(2f, -20f * t + 10f)) / 2f;
        }

        /// <summary>
        /// Smooth acceleration and deceleration, the classic smoothstep curve.
        /// </summary>
        private static float EaseInOut(float t) => t * t * (3f - 2f * t);

        /// <summary>
        /// Cubic acceleration followed by cubic deceleration.
        /// </summary>
        private static float EaseInOutCubic(float t) => t < 0.5f
            ? 4f * t * t * t
            : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;

        /// <summary>
        /// Deceleration that overshoots the end value before settling back onto it.
        /// </summary>
        private static float EaseOutBack(float t)
            => 1f + BackReturnFactor * Mathf.Pow(t - 1f, 3f) + BackOvershoot * Mathf.Pow(t - 1f, 2f);

        /// <summary>
        /// Acceleration with a rubber-band wind-up before the motion starts.
        /// </summary>
        private static float EaseInElastic(float t)
        {
            if (IsZero(t))
                return 0f;

            if (IsOne(t))
                return 1f;

            return -Mathf.Pow(2f, 10f * t - 10f) * Mathf.Sin((t * 10f - 10.75f) * ElasticPeriod);
        }

        /// <summary>
        /// Deceleration that wobbles around the end value before settling.
        /// </summary>
        private static float EaseOutElastic(float t)
        {
            if (IsZero(t))
                return 0f;

            if (IsOne(t))
                return 1f;

            return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * ElasticPeriod) + 1f;
        }

        /// <summary>
        /// Easing out with a bouncing motion.
        /// </summary>
        private static float EaseOutBounce(float t)
        {
            switch (t)
            {
                case < 1f / BounceSegments:
                    return BounceAmplitude * t * t;
                case < 2f / BounceSegments:
                    t -= 1.5f / BounceSegments;
                    return BounceAmplitude * t * t + 0.75f;
                case < 2.5f / BounceSegments:
                    t -= 2.25f / BounceSegments;
                    return BounceAmplitude * t * t + 0.9375f;
                default:
                    t -= 2.625f / BounceSegments;
                    return BounceAmplitude * t * t + 0.984375f;
            }
        }

        /// <summary>
        /// Easing in with a bouncing motion.
        /// </summary>
        private static float EaseInBounce(float t) => 1f - EaseOutBounce(1f - t);

        private static bool IsZero(float t) => Mathf.Approximately(t, 0f);

        private static bool IsOne(float t) => Mathf.Approximately(t, 1f);
    }
}