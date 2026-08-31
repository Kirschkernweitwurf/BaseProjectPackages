using System;
using UnityEditor;
using UnityEngine;

namespace Base.UtilityPackage.Editor
{
    /// <summary>
    /// Drives the editor progress bar for the length of a scan and takes it down again afterwards.
    /// </summary>
    /// <remarks>
    /// The bar has to be cleared on every way out of a loop, the early return and the thrown
    /// exception included, or it stays on screen and blocks the editor. Disposing is what guarantees
    /// that, which is why this is used with a <c>using</c> statement rather than called by hand.
    /// <para>
    /// Showing the bar is expensive enough to matter in a loop over thousands of assets, so it is only
    /// refreshed every so many items. A cancel is sticky: once the user has pressed it, every later
    /// report says stop as well, including the ones that fall between two refreshes.
    /// </para>
    /// </remarks>
    public sealed class EditorScanProgress : IDisposable
    {
        private const int DefaultReportInterval = 25;
        private const float FullSpan = 1f;
        private const int MinimumInterval = 1;
        private const float PhaseStart = 0f;

        private readonly bool _isCancelable;
        private readonly int _reportInterval;

        private string _title;
        private float _start;
        private float _span;
        private bool _isCanceled;

        /// <summary>True once the user has pressed cancel.</summary>
        public bool IsCanceled => _isCanceled;

        /// <summary>Shows a bar for the duration of the scope.</summary>
        /// <param name="title">The window title of the bar.</param>
        /// <param name="isCancelable">
        /// False for work that cannot be stopped halfway without leaving a mess behind, such as a
        /// batch of import settings being written.
        /// </param>
        /// <param name="reportInterval">
        /// How many items pass between two refreshes. Ignored for a run shorter than that, which is
        /// refreshed on every item so the bar still moves.
        /// </param>
        public EditorScanProgress(string title, bool isCancelable = true,
            int reportInterval = DefaultReportInterval)
        {
            _title = title;
            _isCancelable = isCancelable;
            _reportInterval = Mathf.Max(MinimumInterval, reportInterval);
            _start = PhaseStart;
            _span = FullSpan;
        }

        /// <summary>
        /// Points the following reports at one slice of the bar, for a scan that runs through several
        /// phases and wants the bar to fill once rather than once per phase.
        /// </summary>
        /// <param name="title">The title for this phase.</param>
        /// <param name="start">Where the phase begins on the whole bar, from zero to one.</param>
        /// <param name="span">How much of the whole bar the phase covers, from zero to one.</param>
        public void BeginPhase(string title, float start = PhaseStart, float span = FullSpan)
        {
            _title = title;
            _start = start;
            _span = span;
        }

        /// <summary>
        /// Reports one step and answers whether the loop should carry on.
        /// </summary>
        /// <param name="done">How many items are finished.</param>
        /// <param name="total">How many items there are in all.</param>
        /// <param name="label">What the scan is working on, shown under the title.</param>
        /// <returns>False once the user has canceled, so the caller can break out of its loop.</returns>
        public bool Report(int done, int total, string label)
        {
            if (_isCanceled)
                return false;

            if (total <= 0)
                return true;

            // A run shorter than the interval would only ever report its first item and leave the bar
            // sitting at zero until it is gone again.
            int interval = total <= _reportInterval
                ? MinimumInterval
                : _reportInterval;

            if (done % interval != 0)
                return true;

            float fraction = Mathf.Clamp01(_start + _span * (done / (float)total));

            if (!_isCancelable)
            {
                EditorUtility.DisplayProgressBar(_title, label, fraction);
                return true;
            }

            _isCanceled = EditorUtility.DisplayCancelableProgressBar(_title, label, fraction);

            return !_isCanceled;
        }

        /// <summary>Takes the bar down. Safe to call more than once.</summary>
        public void Dispose() => EditorUtility.ClearProgressBar();
    }
}