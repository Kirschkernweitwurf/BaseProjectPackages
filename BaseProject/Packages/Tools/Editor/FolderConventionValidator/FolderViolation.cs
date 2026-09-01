namespace Base.ToolPackage.Editor.FolderConventionValidator
{
    /// <summary>
    /// One broken folder rule. Immutable, built by the <see cref="FolderConventionScanner"/>
    /// and only read by the window.
    /// </summary>
    internal sealed class FolderViolation
    {
        /// <summary>Rule that was broken.</summary>
        internal EFolderViolationType Type { get; }

        /// <summary>Asset path the violation points at, for example "Assets/art/Textures".</summary>
        internal string Path { get; }

        /// <summary>Reason in plain words, shown as the second column in the window.</summary>
        internal string Message { get; }

        /// <summary>True when the window can repair the violation by creating the folder.</summary>
        internal bool IsFixable => Type == EFolderViolationType.MissingFolder;

        /// <summary>Creates a violation for the given path.</summary>
        public FolderViolation(EFolderViolationType type, string path, string message)
        {
            Type = type;
            Path = path;
            Message = message;
        }
    }
}