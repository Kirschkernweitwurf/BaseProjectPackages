namespace Base.AttributePackage.Editor.Windows.AttributeTroubleshoot
{
    /// <summary>Which view the troubleshoot window is showing.</summary>
    internal enum ETroubleshootTab : byte
    {
        /// <summary>Findings from the actual project.</summary>
        Project = 0,

        /// <summary>Findings from the deliberately broken sample types.</summary>
        Samples = 1,

        /// <summary>A live inspector of a demo object, showing the attributes as they render.</summary>
        Showcase = 2
    }
}