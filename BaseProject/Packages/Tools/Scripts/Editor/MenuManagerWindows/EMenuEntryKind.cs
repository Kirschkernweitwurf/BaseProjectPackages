namespace Base.ToolPackage.Editor.MenuManagerWindows
{
    /// <summary>Kind of a managed menu entry.</summary>
    public enum EMenuEntryKind : byte
    {
        /// <summary>A method invoked from an editor menu.</summary>
        MenuItem = 0,

        /// <summary>A ScriptableObject asset created from the Assets/Create menu.</summary>
        CreateAsset = 1
    }
}