namespace Base.ToolsPackage.Editor.MenuManagerWindows
{
    /// <summary>Live state of a menu entry as reported by its source.</summary>
    internal enum EMenuEntryState : byte
    {
        /// <summary>The entry is registered and reachable from the menu.</summary>
        Active = 0,

        /// <summary>The entry exists but is switched off in the menu manager.</summary>
        Disabled = 1,

        /// <summary>The code behind the entry no longer exists.</summary>
        Missing = 2
    }
}