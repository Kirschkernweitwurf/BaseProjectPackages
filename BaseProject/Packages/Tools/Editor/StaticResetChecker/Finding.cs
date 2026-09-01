namespace Base.ToolPackage.Editor.StaticResetChecker
{
    /// <summary>
    /// Represents a finding of a static field that is not reset on Enter Play Mode.
    /// </summary>
    internal class Finding
    {
        internal string AssetPath;
        internal string AbsolutePath;
        internal int Line;
        internal string Name;
        internal string Kind;
        internal string Snippet;
    }
}