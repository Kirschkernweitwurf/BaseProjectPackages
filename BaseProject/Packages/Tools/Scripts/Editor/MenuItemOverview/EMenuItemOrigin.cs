namespace Base.ToolPackage.Editor.MenuItemOverview
{
    /// <summary>Where a menu item's defining script lives.</summary>
    public enum EMenuItemOrigin : byte
    {
        /// <summary>Inside the project's Assets folder.</summary>
        Project = 0,

        /// <summary>Inside an imported package.</summary>
        Package = 1,

        /// <summary>Built into Unity, with no editable source file.</summary>
        BuiltIn = 2
    }
}