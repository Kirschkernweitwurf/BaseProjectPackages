using Base.ToolPackage.Editor.AudioRules.Model;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.AudioRules.Scanning
{
    /// <summary>
    /// Estimates what a clip costs in memory at runtime and in the build, from its length, channel
    /// count and settings.
    /// <para>
    /// These are estimates, not measurements. The decompressed size is exact arithmetic, the
    /// encoded sizes are the published ratio for ADPCM and a bitrate curve for the lossy formats.
    /// They are meant for sorting the list by what is worth fixing first, not for a memory budget
    /// report.
    /// </para>
    /// </summary>
    internal static class AudioMemoryEstimator
    {
        private const int BytesPerSample = 2;
        private const float AdpcmRatio = 3.5f;
        private const int MaxLossyBitrate = 256000;
        private const int MinLossyBitrate = 32000;
        private const int StreamingBufferBytes = 65536;
        private const int BitsPerByte = 8;

        /// <summary>What the clip occupies in memory while it is loaded.</summary>
        /// <param name="info">The facts about the clip.</param>
        /// <param name="settings">The settings to estimate for.</param>
        /// <returns>The estimated runtime bytes.</returns>
        internal static long EstimateRuntimeBytes(AudioClipInfo info, AudioSettingValues settings)
        {
            if (settings.LoadType == AudioClipLoadType.Streaming)
                return StreamingBufferBytes;

            if (settings.LoadType == AudioClipLoadType.CompressedInMemory)
                return EstimateBuildBytes(info, settings);

            return DecompressedBytes(info, settings);
        }

        /// <summary>What the clip occupies in the build, whatever its load type is.</summary>
        /// <param name="info">The facts about the clip.</param>
        /// <param name="settings">The settings to estimate for.</param>
        /// <returns>The estimated build bytes.</returns>
        internal static long EstimateBuildBytes(AudioClipInfo info, AudioSettingValues settings)
        {
            long raw = DecompressedBytes(info, settings);

            switch (settings.CompressionFormat)
            {
                case AudioCompressionFormat.PCM:
                    return raw;

                case AudioCompressionFormat.ADPCM:
                    return (long)(raw / AdpcmRatio);

                default:
                    return LossyBytes(info, settings);
            }
        }

        private static int TargetChannels(AudioClipInfo info, AudioSettingValues settings) => settings.ForceToMono
            ? 1
            : Mathf.Max(1, info.Channels);

        private static int TargetSampleRate(AudioClipInfo info, AudioSettingValues settings)
            => settings.SampleRateSetting == AudioSampleRateSetting.OverrideSampleRate
                ? Mathf.Max(1, settings.SampleRateOverride)
                : Mathf.Max(1, info.SampleRate);

        private static long DecompressedBytes(AudioClipInfo info, AudioSettingValues settings)
        {
            long frames = (long)(info.LengthSeconds * TargetSampleRate(info, settings));

            return frames * TargetChannels(info, settings) * BytesPerSample;
        }

        // Unity does not publish the quality to bitrate curve, so this is a straight line between a
        // heavily compressed and a near transparent stream, scaled by the channel count.
        private static long LossyBytes(AudioClipInfo info, AudioSettingValues settings)
        {
            float quality = Mathf.Clamp01(settings.Quality);
            float bitrate = Mathf.Lerp(MinLossyBitrate, MaxLossyBitrate, quality) * TargetChannels(info, settings);

            return (long)(bitrate * info.LengthSeconds / BitsPerByte);
        }
    }
}