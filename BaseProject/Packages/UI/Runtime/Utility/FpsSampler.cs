using UnityEngine;

namespace Base.UIPackage.Utility
{
    /// <summary>
    /// Turns a stream of frame times into the number a counter shows.
    /// <para>
    /// Two things sit between the raw frame time and the label. The reading is smoothed, so a single
    /// long frame does not swing the number to that frame's rate, and it is only offered twice a
    /// second and only when it actually changed, so the text is readable rather than flickering.
    /// </para>
    /// </summary>
    internal sealed class FpsSampler
    {
        // A low factor is a long memory. Each frame moves the reading a tenth of the way toward the
        // rate that frame ran at, so a hitch shows as a dip rather than as the hitch's own rate.
        private const float SmoothingFactor = 0.1f;

        // Below any real frame rate, so the first reading is always different from it and always shown.
        private const int UnsetFps = -1;

        private const float UpdateInterval = 0.5f;

        private float _smoothedDelta;
        private float _timer;
        private int _lastFps = UnsetFps;

        /// <summary>
        /// Takes one frame and reports whether the counter has something new to show.
        /// </summary>
        /// <param name="unscaledDeltaTime">How long the frame took, unaffected by the time scale.</param>
        /// <param name="fps">The number to show, valid only when this returns true.</param>
        /// <returns>True when the interval elapsed and the number changed.</returns>
        internal bool TryRead(float unscaledDeltaTime, out int fps)
        {
            fps = _lastFps;

            _smoothedDelta += (unscaledDeltaTime - _smoothedDelta) * SmoothingFactor;
            _timer += unscaledDeltaTime;

            if (_timer < UpdateInterval)
                return false;

            _timer = 0f;

            int reading = Mathf.CeilToInt(1f / _smoothedDelta);

            if (reading == _lastFps)
                return false;

            _lastFps = reading;
            fps = reading;

            return true;
        }
    }
}