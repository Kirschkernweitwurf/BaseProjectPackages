namespace Base.AttributePackage.Editor.Windows.RequiredReferenceWindow
{
    /// <summary>A single missing required reference.</summary>
    internal sealed class RequiredReferenceEntry
    {
        /// <summary>Display text shown in the UI.</summary>
        public string DisplayName => $"{ComponentName}.{Path}";

        /// <summary>Name of the component that owns the missing reference.</summary>
        private string ComponentName { get; }

        /// <summary>Path of the missing reference.</summary>
        private string Path { get; }

        /// <summary>Creates an entry for one missing reference.</summary>
        public RequiredReferenceEntry(string componentName, string path)
        {
            ComponentName = componentName;
            Path = path;
        }
    }
}