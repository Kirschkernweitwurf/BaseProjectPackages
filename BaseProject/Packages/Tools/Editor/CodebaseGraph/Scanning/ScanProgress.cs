using System;

namespace Base.ToolPackage.Editor.CodebaseGraph.Scanning
{
    /// <summary>
    /// Reports progress from inside one phase of the scan and carries the answer back. A long loop that
    /// cannot say where it is leaves the bar frozen, and one that cannot be told to stop leaves the
    /// cancel button lying about what it does.
    /// </summary>
    internal sealed class ScanProgress
    {
        private const int ReportInterval = 50;

        private readonly Func<float, string, bool> _callback;
        private readonly float _start;
        private readonly float _span;

        /// <summary>Creates a reporter for one phase.</summary>
        /// <param name="callback">Receives normalized progress and returns false to cancel.</param>
        /// <param name="start">Where this phase begins on the overall bar.</param>
        /// <param name="span">How much of the overall bar this phase covers.</param>
        public ScanProgress(Func<float, string, bool> callback, float start, float span)
        {
            _callback = callback;
            _start = start;
            _span = span;
        }

        /// <summary>Reports a step and says whether the phase should carry on.</summary>
        /// <param name="done">How many items are finished.</param>
        /// <param name="total">How many there are in all.</param>
        /// <param name="label">What the phase is doing.</param>
        /// <returns>False when the scan was canceled.</returns>
        internal bool Report(int done, int total, string label)
        {
            if (_callback == null || total == 0 || done % ReportInterval != 0)
                return true;

            float fraction = _start + _span * (done / (float)total);
            return _callback(fraction, $"{label} {done} / {total}");
        }
    }
}