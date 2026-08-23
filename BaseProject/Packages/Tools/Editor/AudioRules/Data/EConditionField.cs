namespace Base.ToolPackage.Editor.AudioRules.Data
{
    /// <summary>
    /// The fact about a clip a condition looks at. Numeric fields accept the comparison operators,
    /// text fields accept the text operators, and the looping flag only accepts equality.
    /// </summary>
    public enum EConditionField : byte
    {
        /// <summary>Category the clip was referenced with, empty when no container references it.</summary>
        Category = 0,

        /// <summary>Channel count of the clip as it is imported today.</summary>
        Channels = 1,

        /// <summary>Length of the clip in seconds.</summary>
        DurationSeconds = 2,

        /// <summary>Size of the source file on disk in kilobytes.</summary>
        FileSizeKilobytes = 3,

        /// <summary>Whether a container plays the clip as a loop.</summary>
        IsLooping = 4,

        /// <summary>File name of the clip without the extension.</summary>
        Name = 5,

        /// <summary>Project relative path of the clip.</summary>
        Path = 6,

        /// <summary>Sample rate of the clip as it is imported today, in Hz.</summary>
        SampleRate = 7
    }
}