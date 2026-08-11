namespace Base.ToolPackage.Editor.NamingConventions.Window
{
    /// <summary>What the window still has to do after a deferred edit was applied.</summary>
    internal enum EAssetNamingEditOutcome : byte
    {
        /// <summary>Nothing was pending, or the edit changed nothing the window shows.</summary>
        None = 0,

        /// <summary>The drawn state changed and the window has to repaint.</summary>
        Repaint = 1,

        /// <summary>The project has to be scanned again before the next repaint.</summary>
        Rescan = 2
    }
}