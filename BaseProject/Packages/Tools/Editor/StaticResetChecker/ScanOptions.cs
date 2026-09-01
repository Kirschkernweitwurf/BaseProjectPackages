namespace Base.ToolPackage.Editor.StaticResetChecker
{
    /// <summary>
    /// Options for scanning the project for static fields that are not reset on Enter Play Mode.
    /// </summary>
    internal class ScanOptions
    {
        internal string RootFolder = "Assets";
        internal string[] ResetAttributes =
        {
            "InitializeOnEnterPlayMode",
            "RuntimeInitializeOnLoadMethod"
        };
        internal string IgnoreMarker = "reset-ignore";
        internal bool IncludeEvents = true;
        internal bool IncludeAutoProperties = true;
        internal bool SkipEditorFolders = true;
        internal bool ExpandHelpers = true;
        internal bool IgnoreReadonly = true;
    }
}