using System;
using UnityEngine;

namespace Base.ToolPackage.Editor.AudioRules.Data
{
    /// <summary>
    /// The thresholds the deep analysis judges by. They live in the rule set so a project can
    /// decide for itself what counts as too quiet or as too much silence at the head of a clip.
    /// </summary>
    [Serializable]
    internal sealed class AudioAnalysisSettings
    {
        private const float DefaultClipLevel = 0.999f;
        private const int DefaultClippedSamples = 8;
        private const float DefaultDcOffset = 0.01f;
        private const float DefaultLowPeak = 0.3f;
        private const float DefaultSilenceLevel = 0.001f;
        private const float DefaultSilenceSeconds = 0.05f;
        private const float DefaultStereoTolerance = 0.0005f;

        [field: Tooltip("Below this level a sample counts as silence. 0.001 is about -60 dB.")]
        [field: SerializeField] public float SilenceLevel { get; set; } = DefaultSilenceLevel;

        [field: Tooltip("Silence at the head or tail longer than this is reported, in seconds.")]
        [field: SerializeField] public float SilenceSeconds { get; set; } = DefaultSilenceSeconds;

        [field: Tooltip("A clip whose loudest sample stays below this is reported as quiet.")]
        [field: Range(0f, 1f)]
        [field: SerializeField] public float LowPeakLevel { get; set; } = DefaultLowPeak;

        [field: Tooltip("At or above this level a sample counts as clipped.")]
        [field: Range(0.9f, 1f)]
        [field: SerializeField] public float ClipLevel { get; set; } = DefaultClipLevel;

        [field: Tooltip("How many clipped samples are tolerated before the clip is reported.")]
        [field: Min(0)]
        [field: SerializeField] public int ClippedSampleBudget { get; set; } = DefaultClippedSamples;

        [field: Tooltip("An average further from zero than this is reported as a DC offset.")]
        [field: SerializeField] public float DcOffsetLimit { get; set; } = DefaultDcOffset;

        [field: Tooltip("Two channels that never differ by more than this count as fake stereo.")]
        [field: SerializeField] public float StereoTolerance { get; set; } = DefaultStereoTolerance;

        /// <summary>Creates the default thresholds. Needed by the serializer.</summary>
        public AudioAnalysisSettings() { }
    }
}