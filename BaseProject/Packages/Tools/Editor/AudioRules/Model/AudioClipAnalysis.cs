namespace Base.ToolPackage.Editor.AudioRules.Model
{
    /// <summary>
    /// What reading the sample data of one clip turned up. Built by the analyzer, cached between
    /// runs, and only filled in once the deep pass has actually run over the clip.
    /// </summary>
    public sealed class AudioClipAnalysis
    {
        /// <summary>True once the sample data was readable and the numbers below are meaningful.</summary>
        public bool HasData { get; set; }

        /// <summary>Loudest absolute sample, 1 being full scale.</summary>
        public float Peak { get; set; }

        /// <summary>Average level over the whole clip, 1 being full scale.</summary>
        public float Rms { get; set; }

        /// <summary>How far the average sample sits away from zero.</summary>
        public float DcOffset { get; set; }

        /// <summary>Seconds of near silence at the start.</summary>
        public float LeadingSilence { get; set; }

        /// <summary>Seconds of near silence at the end.</summary>
        public float TrailingSilence { get; set; }

        /// <summary>How many samples sit at or above full scale.</summary>
        public int ClippedSamples { get; set; }

        /// <summary>Largest difference between the two channels, 0 meaning they are identical.</summary>
        public float ChannelDifference { get; set; }

        /// <summary>True when the clip has two channels that carry the same signal.</summary>
        public bool IsStereo { get; set; }
    }
}