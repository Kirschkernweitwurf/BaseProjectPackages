using System;
using System.Collections.Generic;
using Base.ToolPackage.Editor.AudioRules.Data;
using Base.ToolPackage.Editor.AudioRules.Model;
using UnityEngine;

namespace Base.ToolPackage.Editor.AudioRules.Scanning
{
    /// <summary>
    /// Reads the sample data of a clip and reports what is in it rather than what the importer was
    /// told. Finds the two channels of a fake stereo file, silence nobody trimmed, clipping, a DC
    /// offset and clips that will sit too low in the mix.
    /// <para>
    /// Sample data is read in chunks so a long track does not allocate a hundred megabyte array.
    /// Streaming clips refuse to hand their data over, so those come back without a result.
    /// </para>
    /// </summary>
    internal static class AudioClipAnalyzer
    {
        private const int ChunkFrames = 65536;
        private const int StereoChannels = 2;

        /// <summary>Reads one clip and measures it.</summary>
        /// <param name="clip">The clip to read.</param>
        /// <param name="settings">The thresholds that decide what counts as silence.</param>
        /// <returns>The measurements, with <c>HasData</c> false when the clip refused to be read.</returns>
        public static AudioClipAnalysis Analyze(AudioClip clip, AudioAnalysisSettings settings)
        {
            AudioClipAnalysis analysis = new();

            if (clip == null
                || clip.samples <= 0
                || clip.channels <= 0)
                return analysis;

            analysis.IsStereo = clip.channels == StereoChannels;

            if (!clip.LoadAudioData())
                return analysis;

            return Measure(clip, settings, analysis);
        }

        /// <summary>Turns the measurements into the findings shown in the window.</summary>
        /// <param name="analysis">The measurements of one clip.</param>
        /// <param name="settings">The thresholds to judge by.</param>
        /// <returns>Everything worth reporting about the clip.</returns>
        public static List<EAudioFinding> Evaluate(AudioClipAnalysis analysis, AudioAnalysisSettings settings)
        {
            List<EAudioFinding> findings = new();

            if (analysis == null
                || !analysis.HasData)
                return findings;

            if (analysis.IsStereo
                && analysis.ChannelDifference <= settings.StereoTolerance)
                findings.Add(EAudioFinding.FakeStereo);

            if (analysis.ClippedSamples > settings.ClippedSampleBudget)
                findings.Add(EAudioFinding.Clipping);

            if (analysis.DcOffset > settings.DcOffsetLimit)
                findings.Add(EAudioFinding.DcOffset);

            if (analysis.LeadingSilence > settings.SilenceSeconds)
                findings.Add(EAudioFinding.LeadingSilence);

            if (analysis.TrailingSilence > settings.SilenceSeconds)
                findings.Add(EAudioFinding.TrailingSilence);

            if (analysis.Peak > 0f
                && analysis.Peak < settings.LowPeakLevel)
                findings.Add(EAudioFinding.LowPeak);

            return findings;
        }

        private static AudioClipAnalysis Measure(AudioClip clip, AudioAnalysisSettings settings,
            AudioClipAnalysis analysis)
        {
            int channels = clip.channels;
            int frames = clip.samples;
            float[] buffer = new float[ChunkFrames * channels];

            double sum = 0d;
            double sumOfSquares = 0d;
            long samples = 0L;

            float peak = 0f;
            float channelDifference = 0f;
            int clipped = 0;
            int firstLoudFrame = -1;
            int lastLoudFrame = -1;

            for (int offset = 0; offset < frames; offset += ChunkFrames)
            {
                int frameCount = Mathf.Min(ChunkFrames, frames - offset);

                float[] chunk = frameCount == ChunkFrames
                    ? buffer
                    : new float[frameCount * channels];

                if (!clip.GetData(chunk, offset))
                    return analysis;

                for (int frame = 0; frame < frameCount; frame++)
                {
                    float framePeak = 0f;

                    for (int channel = 0; channel < channels; channel++)
                    {
                        float sample = chunk[frame * channels + channel];
                        float magnitude = Mathf.Abs(sample);

                        sum += sample;
                        sumOfSquares += sample * (double)sample;
                        samples++;

                        if (magnitude > framePeak)
                            framePeak = magnitude;

                        if (magnitude >= settings.ClipLevel)
                            clipped++;
                    }

                    if (channels == StereoChannels)
                    {
                        float difference = Mathf.Abs(chunk[frame * channels] - chunk[frame * channels + 1]);

                        if (difference > channelDifference)
                            channelDifference = difference;
                    }

                    if (framePeak > peak)
                        peak = framePeak;

                    if (framePeak <= settings.SilenceLevel)
                        continue;

                    if (firstLoudFrame < 0)
                        firstLoudFrame = offset + frame;

                    lastLoudFrame = offset + frame;
                }
            }

            return Fill(analysis, clip, samples, sum, sumOfSquares, peak, channelDifference, clipped,
                firstLoudFrame, lastLoudFrame);
        }

        private static AudioClipAnalysis Fill(AudioClipAnalysis analysis, AudioClip clip, long samples, double sum,
            double sumOfSquares, float peak, float channelDifference, int clipped, int firstLoudFrame,
            int lastLoudFrame)
        {
            if (samples == 0L)
                return analysis;

            float rate = Mathf.Max(1, clip.frequency);

            analysis.HasData = true;
            analysis.Peak = peak;
            analysis.Rms = (float)Math.Sqrt(sumOfSquares / samples);
            analysis.DcOffset = (float)Math.Abs(sum / samples);
            analysis.ClippedSamples = clipped;
            analysis.ChannelDifference = channelDifference;

            // A clip that never rose above the silence level counts as silent from end to end.
            analysis.LeadingSilence = firstLoudFrame < 0
                ? clip.length
                : firstLoudFrame / rate;

            analysis.TrailingSilence = firstLoudFrame < 0
                ? 0f
                : (clip.samples - 1 - lastLoudFrame) / rate;

            return analysis;
        }
    }
}