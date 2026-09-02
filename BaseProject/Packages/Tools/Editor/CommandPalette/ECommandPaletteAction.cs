namespace Base.ToolsPackage.Editor.CommandPalette
{
    /// <summary>
    /// Something the user asked the palette to do. Keyboard, mouse and context menu all express
    /// themselves through this, so every action is implemented exactly once.
    /// </summary>
    internal enum ECommandPaletteAction : byte
    {
        /// <summary>Nothing was requested.</summary>
        None = 0,

        /// <summary>Close the palette without running anything.</summary>
        Close = 1,

        /// <summary>Edit the tags of the selected entry.</summary>
        EditTags = 2,

        /// <summary>Move the selection one entry down.</summary>
        MoveDown = 3,

        /// <summary>Move the selection one entry up.</summary>
        MoveUp = 4,

        /// <summary>Open the script that declares the selected entry.</summary>
        OpenScript = 5,

        /// <summary>Move the selection one page down.</summary>
        PageDown = 6,

        /// <summary>Move the selection one page up.</summary>
        PageUp = 7,

        /// <summary>Build the command index again.</summary>
        Rescan = 8,

        /// <summary>Run the selected entry.</summary>
        Run = 9,

        /// <summary>Open the context menu of the selected entry.</summary>
        ShowMenu = 10,

        /// <summary>Pin or unpin the selected entry.</summary>
        TogglePin = 11
    }
}