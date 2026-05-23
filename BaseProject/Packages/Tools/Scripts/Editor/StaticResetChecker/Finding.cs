namespace Base.ToolPackage.Editor.StaticResetChecker
{
    /// <summary>
    /// Represents a finding of a static field that is not reset on Enter Play Mode.
    /// </summary>
    public class Finding
    {
        public string AssetPath;
        public string AbsolutePath;
        public int Line;
        public string Name;
        public string Kind;
        public string Snippet;
    }
}