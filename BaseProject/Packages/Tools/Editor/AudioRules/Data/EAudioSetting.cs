namespace Base.ToolPackage.Editor.AudioRules.Data
{
    /// <summary>
    /// One import setting a rule can write. Used as the key of the decision trace, so the window
    /// can show which rule won each setting.
    /// </summary>
    internal enum EAudioSetting : byte
    {
        /// <summary>The codec the clip is stored with.</summary>
        CompressionFormat = 0,

        /// <summary>Whether the clip is downmixed to one channel on import.</summary>
        ForceToMono = 1,

        /// <summary>Whether the clip is loaded on a worker thread.</summary>
        LoadInBackground = 2,

        /// <summary>How the clip lives in memory at runtime.</summary>
        LoadType = 3,

        /// <summary>Whether the audio data is loaded together with its scene.</summary>
        PreloadAudioData = 4,

        /// <summary>Encoder quality of the lossy formats.</summary>
        Quality = 5,

        /// <summary>How the sample rate is handled, including the forced rate.</summary>
        SampleRate = 6
    }
}