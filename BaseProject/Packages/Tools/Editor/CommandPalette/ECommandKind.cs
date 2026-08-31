namespace Base.ToolPackage.Editor.CommandPalette
{
    /// <summary>What executing a palette command actually does.</summary>
    internal enum ECommandKind : byte
    {
        /// <summary>Creates a new ScriptableObject asset in the project window.</summary>
        CreateAsset = 0,

        /// <summary>Invokes an editor menu item.</summary>
        MenuItem = 1,

        /// <summary>Opens a page of the project settings or of the preferences.</summary>
        Settings = 2
    }
}