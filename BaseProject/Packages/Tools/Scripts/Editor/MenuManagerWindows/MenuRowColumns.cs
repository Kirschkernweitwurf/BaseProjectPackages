using UnityEngine;

namespace Base.ToolPackage.Editor.MenuManagerWindows
{
    /// <summary>Cell rects of a single entry row, measured once and then drawn into.</summary>
    internal struct MenuRowColumns
    {
        /// <summary>Drag handle on the far left.</summary>
        public Rect Grip;

        /// <summary>Enabled toggle.</summary>
        public Rect Toggle;

        /// <summary>Editable menu path.</summary>
        public Rect Path;

        /// <summary>Editable default asset file name, only used by the create asset window.</summary>
        public Rect File;

        /// <summary>Priority value and its override button.</summary>
        public Rect Priority;

        /// <summary>Open or Remove button on the far right.</summary>
        public Rect Status;
    }
}