using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.AudioRules.Model
{
    /// <summary>
    /// The seven import settings the tool cares about, in one place. Used both for what a clip has
    /// today and for what the rules want it to have, so the two can be compared setting by setting.
    /// </summary>
    public sealed class AudioSettingValues
    {
        /// <summary>How the clip lives in memory at runtime.</summary>
        public AudioClipLoadType LoadType { get; set; }

        /// <summary>The codec the clip is stored with.</summary>
        public AudioCompressionFormat CompressionFormat { get; set; }

        /// <summary>Encoder quality of the lossy formats, from 0 to 1.</summary>
        public float Quality { get; set; }

        /// <summary>How the sample rate is handled.</summary>
        public AudioSampleRateSetting SampleRateSetting { get; set; }

        /// <summary>The forced sample rate in Hz, only meaningful with an override.</summary>
        public int SampleRateOverride { get; set; }

        /// <summary>Whether the clip is downmixed to one channel on import.</summary>
        public bool ForceToMono { get; set; }

        /// <summary>Whether the clip is loaded on a worker thread.</summary>
        public bool LoadInBackground { get; set; }

        /// <summary>Whether the audio data is loaded together with its scene.</summary>
        public bool PreloadAudioData { get; set; }

        /// <summary>Creates an independent copy, used as the starting point of a resolve.</summary>
        /// <returns>A copy holding the same values.</returns>
        public AudioSettingValues Clone() => new()
        {
            LoadType = LoadType,
            CompressionFormat = CompressionFormat,
            Quality = Quality,
            SampleRateSetting = SampleRateSetting,
            SampleRateOverride = SampleRateOverride,
            ForceToMono = ForceToMono,
            LoadInBackground = LoadInBackground,
            PreloadAudioData = PreloadAudioData
        };
    }
}