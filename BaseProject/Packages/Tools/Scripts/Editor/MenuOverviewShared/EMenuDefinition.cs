namespace Base.ToolPackage.Editor.MenuOverview
{
    /// <summary>How a menu entry gets its place in the editor menus.</summary>
    public enum EMenuDefinition : byte
    {
        /// <summary>Declared by a Unity attribute and fixed at compile time.</summary>
        Static = 0,

        /// <summary>Declared by a dynamic attribute and arranged in the menu manager.</summary>
        Dynamic = 1
    }
}