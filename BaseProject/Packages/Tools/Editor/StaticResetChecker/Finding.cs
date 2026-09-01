namespace Base.ToolPackage.Editor.StaticResetChecker
{
    /// <summary>
    /// Represents a finding of a static field that is not reset on Enter Play Mode.
    /// </summary>
    internal sealed class Finding
    {
        /// <summary>Project-relative path of the file, which is what pings the asset.</summary>
        internal string AssetPath;

        /// <summary>Full path on disk, for opening the file in an external editor.</summary>
        internal string AbsolutePath;

        /// <summary>Line the field is declared on.</summary>
        internal int Line;

        /// <summary>Name of the field, property or event.</summary>
        internal string Name;

        /// <summary>What kind of member it is, so the report can group by field, event or property.</summary>
        internal string Kind;

        /// <summary>The declaration as written, shown so the finding can be judged without opening the file.</summary>
        internal string Snippet;
    }
}