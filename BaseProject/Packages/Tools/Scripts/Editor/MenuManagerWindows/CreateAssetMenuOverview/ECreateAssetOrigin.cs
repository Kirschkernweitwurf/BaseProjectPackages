namespace Base.ToolPackage.Editor.MenuManagerWindows.CreateAssetMenuOverview
{
    /// <summary>Where a CreateAssetMenu type's defining script lives.</summary>
    public enum ECreateAssetOrigin : byte
    {
        /// <summary>Inside the project's Assets folder.</summary>
        Project = 0,

        /// <summary>Inside an imported package.</summary>
        Package = 1,

        /// <summary>Built into Unity, with no editable source file.</summary>
        BuiltIn = 2
    }
}