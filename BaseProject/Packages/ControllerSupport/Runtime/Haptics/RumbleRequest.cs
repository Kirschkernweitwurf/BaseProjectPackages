using UnityEngine;

namespace Base.ControllerSupportPackage.Haptics
{
    /// <summary>
    /// One live playback owned by a single caller. Keeps its own clock, so a request that is outranked
    /// still expires on schedule and resumes where it left off instead of restarting.
    /// </summary>
    public sealed class RumbleRequest
    {
        private const float LoopLength = 1f;

        /// <summary>The pattern being played.</summary>
        public RumblePatternData Pattern { get; }

        /// <summary>The object that started the playback. Used as the key for stopping it again.</summary>
        public object Caller { get; }

        /// <summary>Strength multiplier for this request alone, on top of the service's main intensity.</summary>
        public float Intensity { get; }

        /// <summary>True once a non-looping pattern has run past its duration.</summary>
        public bool IsFinished => !Pattern.Loop && _elapsed >= Pattern.Duration;

        private float _elapsed;

        /// <summary>Creates a request for a pattern on behalf of a caller.</summary>
        /// <param name="pattern">The curves and timing to play.</param>
        /// <param name="caller">The object requesting the rumble.</param>
        /// <param name="intensity">Strength multiplier for this request, 0 to 1.</param>
        public RumbleRequest(RumblePatternData pattern, object caller, float intensity)
        {
            Pattern = pattern;
            Caller = caller;
            Intensity = Mathf.Clamp01(intensity);
        }

        /// <summary>Advances this request's own clock by one frame.</summary>
        public void Advance() => _elapsed += Pattern.UseUnscaledTime
            ? Time.unscaledDeltaTime
            : Time.deltaTime;

        /// <summary>Samples both motors at the request's current time, already scaled by its intensity.</summary>
        /// <param name="low">Resulting low frequency motor strength, 0 to 1.</param>
        /// <param name="high">Resulting high frequency motor strength, 0 to 1.</param>
        public void Sample(out float low, out float high)
        {
            Pattern.Evaluate(NormalizedTime(), out low, out high);

            low *= Intensity;
            high *= Intensity;
        }

        private float NormalizedTime()
        {
            float progress = _elapsed / Pattern.Duration;

            return Pattern.Loop
                ? Mathf.Repeat(progress, LoopLength)
                : Mathf.Clamp01(progress);
        }
    }
}