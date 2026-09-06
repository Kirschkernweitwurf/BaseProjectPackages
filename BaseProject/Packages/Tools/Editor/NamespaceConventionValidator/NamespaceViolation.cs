namespace Base.ToolsPackage.Editor.NamespaceConventionValidator
{
    /// <summary>
    /// One broken namespace rule. Immutable, built by the <see cref="NamespaceConventionScanner"/>
    /// and only read by the window.
    /// </summary>
    internal sealed class NamespaceViolation
    {
        /// <summary>Rule that was broken.</summary>
        internal ENamespaceViolationType Type { get; }

        /// <summary>Asset path of the script, for example "Assets/Scripts/Player/Health.cs".</summary>
        internal string Path { get; }

        /// <summary>Reason in plain words, shown as the second column in the window.</summary>
        internal string Message { get; }

        /// <summary>Creates a violation for the given script.</summary>
        /// <param name="type">The rule that was broken.</param>
        /// <param name="path">The asset path of the script.</param>
        /// <param name="message">The reason in plain words.</param>
        public NamespaceViolation(ENamespaceViolationType type, string path, string message)
        {
            Type = type;
            Path = path;
            Message = message;
        }
    }
}