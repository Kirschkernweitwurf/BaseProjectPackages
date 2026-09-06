using System.Collections.Generic;
using Base.ToolsPackage.Editor.MenuManagerModel;

namespace Base.ToolsPackage.Editor.MenuManagerWindows
{
    /// <summary>
    /// Deep copy of both menu trees and the start priority, taken before a change so the undo
    /// stack can put the window back exactly as it was.
    /// </summary>
    internal sealed class MenuUndoState
    {
        /// <summary>Copy of the shipped tree.</summary>
        internal List<MenuNode> Package;

        /// <summary>Copy of the project overlay tree.</summary>
        internal List<MenuNode> Overlay;

        /// <summary>Priority the automatic numbering starts at.</summary>
        internal int Start;
    }
}