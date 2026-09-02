using System;
using Base.AttributesPackage;
using UnityEngine;

namespace Base.ControllerSupportPackage.Haptics
{
    /// <summary>
    /// Curve-driven description of a single haptic. Both motors are authored over normalized time, so the
    /// same shape stretches to any duration. Serializable on its own, so a pattern can sit inline on a
    /// component, be built in code for a one-off burst, or be shared as a <see cref="RumblePattern"/> asset.
    /// </summary>
    [Serializable]
    public sealed class RumblePatternData
    {
        private const float CurveEnd = 1f;
        private const float CurveStart = 0f;
        private const float DefaultDuration = 0.2f;
        private const float FullStrength = 1f;
        private const float MinimumDuration = 0.01f;

        [Title("Timing")]
        [Tooltip("How long one pass over the curves takes, in seconds.")]
        [Min(MinimumDuration)]
        [SerializeField] private float duration = DefaultDuration;

        [Tooltip("If true, the pattern repeats until it is stopped explicitly.")]
        [SerializeField] private bool loop;

        [Tooltip("If true, the pattern ignores the timescale and keeps playing while the game is paused.")]
        [SerializeField] private bool useUnscaledTime = true;

        [Title("Motors")]
        [Tooltip("Low frequency (left) motor over normalized time. The heavy, rolling rumble.")]
        [CurveRange(CurveStart, CurveEnd, EColor.Blue)]
        [SerializeField] private AnimationCurve lowFrequency =
            AnimationCurve.Constant(CurveStart, CurveEnd, FullStrength);

        [Tooltip("High frequency (right) motor over normalized time. The light, buzzing rumble.")]
        [CurveRange(CurveStart, CurveEnd, EColor.Orange)]
        [SerializeField] private AnimationCurve highFrequency =
            AnimationCurve.Constant(CurveStart, CurveEnd, FullStrength);

        /// <summary>How long one pass over the curves takes, in seconds. Never shorter than a single tick.</summary>
        public float Duration => Mathf.Max(duration, MinimumDuration);

        /// <summary>Whether the pattern repeats until it is stopped explicitly.</summary>
        public bool Loop => loop;

        /// <summary>Whether the pattern ignores the timescale, so it survives a paused game.</summary>
        public bool UseUnscaledTime => useUnscaledTime;

        /// <summary>Required by serialization. Creates a pattern holding the authored defaults.</summary>
        public RumblePatternData() { }

        /// <summary>Creates a pattern from explicit curves.</summary>
        /// <param name="duration">How long one pass over the curves takes, in seconds.</param>
        /// <param name="lowFrequency">Low frequency motor strength over normalized time.</param>
        /// <param name="highFrequency">High frequency motor strength over normalized time.</param>
        /// <param name="loop">Whether the pattern repeats until it is stopped.</param>
        /// <param name="useUnscaledTime">Whether the pattern ignores the timescale.</param>
        public RumblePatternData(float duration, AnimationCurve lowFrequency, AnimationCurve highFrequency,
            bool loop = false, bool useUnscaledTime = true)
        {
            this.duration = duration;
            this.lowFrequency = lowFrequency;
            this.highFrequency = highFrequency;
            this.loop = loop;
            this.useUnscaledTime = useUnscaledTime;
        }

        /// <summary>
        /// Creates a pattern that holds both motors at a fixed strength for a fixed time. The cheap way to
        /// fire a hit or a UI click without authoring an asset for it.
        /// </summary>
        /// <param name="low">Low frequency motor strength, 0 to 1.</param>
        /// <param name="high">High frequency motor strength, 0 to 1.</param>
        /// <param name="duration">How long the burst lasts, in seconds.</param>
        /// <param name="useUnscaledTime">Whether the burst ignores the timescale.</param>
        /// <returns>A non-looping pattern holding both motors steady.</returns>
        public static RumblePatternData Constant(float low, float high, float duration, bool useUnscaledTime = true)
            => new(duration, AnimationCurve.Constant(CurveStart, CurveEnd, Mathf.Clamp01(low)),
                AnimationCurve.Constant(CurveStart, CurveEnd, Mathf.Clamp01(high)), false, useUnscaledTime);

        /// <summary>Samples both motors at a normalized time between 0 and 1.</summary>
        /// <param name="normalizedTime">Progress through the pattern, 0 to 1.</param>
        /// <param name="low">Resulting low frequency motor strength, 0 to 1.</param>
        /// <param name="high">Resulting high frequency motor strength, 0 to 1.</param>
        public void Evaluate(float normalizedTime, out float low, out float high)
        {
            low = Mathf.Clamp01(lowFrequency.Evaluate(normalizedTime));
            high = Mathf.Clamp01(highFrequency.Evaluate(normalizedTime));
        }
    }
}