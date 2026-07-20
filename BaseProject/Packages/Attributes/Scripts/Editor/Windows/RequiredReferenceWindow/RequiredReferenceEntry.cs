namespace Base.AttributePackage.Editor.Windows.RequiredReferenceWindow
{
    /// <summary>A single missing required reference.</summary>
    public sealed class RequiredReferenceEntry
    {
        /// <summary>Name of the component that owns the missing reference.</summary>
        public string ComponentName { get; }

        /// <summary>Path of the missing reference.</summary>
        public string Path { get; }

        /// <summary>Display text shown in the UI.</summary>
        public string DisplayName => $"{ComponentName}.{Path}";

        public RequiredReferenceEntry(string componentName, string path)
        {
            ComponentName = componentName;
            Path = path;
        }
    }
}