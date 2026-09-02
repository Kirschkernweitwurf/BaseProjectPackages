using UnityEngine;

namespace Base.ToolsPackage.Editor.MenuManagerWindows
{
    /// <summary>Cell rects of a single entry row, measured once and then drawn into.</summary>
    internal struct MenuRowColumns
    {
        /// <summary>Drag handle on the far left.</summary>
        internal Rect Grip;

        /// <summary>Enabled toggle.</summary>
        internal Rect Toggle;

        /// <summary>Editable menu path.</summary>
        internal Rect Path;

        /// <summary>Editable default asset file name, only used by the create asset window.</summary>
        internal Rect File;

        /// <summary>Priority value and its override button.</summary>
        internal Rect Priority;

        /// <summary>Open or Remove button on the far right.</summary>
        internal Rect Status;
    }
}