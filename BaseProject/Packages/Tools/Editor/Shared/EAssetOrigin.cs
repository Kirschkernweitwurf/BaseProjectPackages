namespace Base.ToolPackage.Editor.Shared
{
    /// <summary>Where the source file behind an asset lives.</summary>
    internal enum EAssetOrigin : byte
    {
        /// <summary>Inside the project's Assets folder.</summary>
        Project = 0,

        /// <summary>Inside an imported package.</summary>
        Package = 1,

        /// <summary>Built into Unity, with no editable source file.</summary>
        BuiltIn = 2
    }
}