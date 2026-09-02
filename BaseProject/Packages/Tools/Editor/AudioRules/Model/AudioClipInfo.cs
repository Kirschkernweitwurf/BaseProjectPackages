namespace Base.ToolsPackage.Editor.AudioRules.Model
{
    /// <summary>
    /// Everything a rule can ask about one clip, gathered once per scan. The numbers describe the
    /// clip as it is imported today, not the source file, because that is what ships and what the
    /// memory estimate has to work from.
    /// </summary>
    internal sealed class AudioClipInfo
    {
        private const float BytesPerKilobyte = 1024f;

        /// <summary>Project relative path of the clip.</summary>
        public string AssetPath { get; }

        /// <summary>GUID of the clip.</summary>
        public string Guid { get; }

        /// <summary>File name without the extension.</summary>
        public string Name { get; }

        /// <summary>Length in seconds.</summary>
        public float LengthSeconds { get; }

        /// <summary>Channel count as imported.</summary>
        public int Channels { get; }

        /// <summary>Sample rate as imported, in Hz.</summary>
        public int SampleRate { get; }

        /// <summary>Size of the source file on disk in bytes.</summary>
        public long FileSizeBytes { get; }

        /// <summary>Category a container referenced this clip with, empty when none did.</summary>
        public string Category { get; }

        /// <summary>True when a container plays this clip as a loop.</summary>
        public bool IsLooping { get; }

        /// <summary>True when at least one container references this clip.</summary>
        public bool HasContainer { get; }

        /// <summary>The import settings the clip has right now for the target being viewed.</summary>
        public AudioSettingValues Current { get; }

        /// <summary>Size of the source file on disk in kilobytes.</summary>
        public float FileSizeKilobytes => FileSizeBytes / BytesPerKilobyte;

        /// <summary>Total sample frames, used by the memory estimate.</summary>
        public int Frames => (int)(LengthSeconds * SampleRate);

        /// <summary>Creates the facts about one clip.</summary>
        /// <param name="assetPath">Project relative path.</param>
        /// <param name="guid">GUID of the clip.</param>
        /// <param name="name">File name without the extension.</param>
        /// <param name="lengthSeconds">Length in seconds.</param>
        /// <param name="channels">Channel count as imported.</param>
        /// <param name="sampleRate">Sample rate as imported.</param>
        /// <param name="fileSizeBytes">Size of the source file on disk.</param>
        /// <param name="category">Category a container referenced the clip with.</param>
        /// <param name="isLooping">Whether a container loops the clip.</param>
        /// <param name="hasContainer">Whether any container references the clip.</param>
        /// <param name="current">The import settings the clip has right now.</param>
        public AudioClipInfo(string assetPath, string guid, string name, float lengthSeconds, int channels,
            int sampleRate, long fileSizeBytes, string category, bool isLooping, bool hasContainer,
            AudioSettingValues current)
        {
            AssetPath = assetPath;
            Guid = guid;
            Name = name;
            LengthSeconds = lengthSeconds;
            Channels = channels;
            SampleRate = sampleRate;
            FileSizeBytes = fileSizeBytes;
            Category = category;
            IsLooping = isLooping;
            HasContainer = hasContainer;
            Current = current;
        }
    }
}