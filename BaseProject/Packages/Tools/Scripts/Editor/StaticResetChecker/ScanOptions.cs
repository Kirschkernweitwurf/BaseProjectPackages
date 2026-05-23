namespace Base.ToolPackage.Editor.StaticResetChecker
{
    /// <summary>
    /// Options for scanning the project for static fields that are not reset on Enter Play Mode.
    /// </summary>
    public class ScanOptions
    {
        public string RootFolder = "Assets";
        public string[] ResetAttributes = { "InitializeOnEnterPlayMode", "RuntimeInitializeOnLoadMethod" };
        public string IgnoreMarker = "reset-ignore";
        public bool IncludeEvents = true;
        public bool IncludeAutoProperties = true;
        public bool SkipEditorFolders = true;
        public bool ExpandHelpers = true;
    }
}