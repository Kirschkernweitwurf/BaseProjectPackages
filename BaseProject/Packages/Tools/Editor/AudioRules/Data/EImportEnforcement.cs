namespace Base.ToolPackage.Editor.AudioRules.Data
{
    /// <summary>When the rules are applied automatically as clips are imported.</summary>
    internal enum EImportEnforcement : byte
    {
        /// <summary>Rules apply on every reimport, so hand made changes never survive.</summary>
        Always = 0,

        /// <summary>A clip is set up the first time it is imported and left alone afterwards.</summary>
        FirstImportOnly = 1,

        /// <summary>Nothing happens on import. The window is the only place rules are applied.</summary>
        Never = 2
    }
}