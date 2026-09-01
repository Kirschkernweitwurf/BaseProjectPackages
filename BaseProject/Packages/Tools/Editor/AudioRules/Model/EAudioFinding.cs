namespace Base.ToolPackage.Editor.AudioRules.Model
{
    /// <summary>
    /// Something the deep analysis found in the sample data itself. These are content problems, so
    /// most of them are reported rather than repaired.
    /// </summary>
    internal enum EAudioFinding : byte
    {
        /// <summary>Samples sit at full scale, so the clip is distorted before the mixer sees it.</summary>
        Clipping = 0,

        /// <summary>The waveform is not centered, which wastes headroom and can click on start.</summary>
        DcOffset = 1,

        /// <summary>Both channels carry the same signal, so the second one costs memory for nothing.</summary>
        FakeStereo = 2,

        /// <summary>The clip starts with silence, which delays a one shot and wastes memory.</summary>
        LeadingSilence = 3,

        /// <summary>The loudest sample is far below full scale, so the clip will sit low in the mix.</summary>
        LowPeak = 4,

        /// <summary>The clip ends with silence that nothing plays.</summary>
        TrailingSilence = 5
    }
}