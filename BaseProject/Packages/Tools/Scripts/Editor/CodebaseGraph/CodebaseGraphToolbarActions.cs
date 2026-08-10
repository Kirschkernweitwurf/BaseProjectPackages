using System;

namespace Base.ToolPackage.Editor.CodebaseGraph
{
    /// <summary>
    /// What the toolbar can ask the window to do. Gathered into one object because a control row that
    /// took nine separate callbacks would be read as nine unrelated things rather than as one contract.
    /// </summary>
    internal sealed class CodebaseGraphToolbarActions
    {
        /// <summary>Raised when anything the view depends on has changed.</summary>
        public Action FilterChanged;

        /// <summary>Raised when only the line drawing changed, which needs no rebuild.</summary>
        public Action EdgeModeChanged;

        /// <summary>Raised when the neighbor depth changed, which only matters while focused.</summary>
        public Action NeighborChanged;

        /// <summary>Raised on every keystroke in the search box, which the window debounces.</summary>
        public Action SearchChanged;

        /// <summary>Raised to go up one level.</summary>
        public Action Back;

        /// <summary>Raised to scan the project again.</summary>
        public Action Rescan;

        /// <summary>Raised to write the findings report.</summary>
        public Action Export;

        /// <summary>Raised to read dismissals back in.</summary>
        public Action Import;

        /// <summary>Raised to write a report about one namespace or assembly.</summary>
        public Action ExportScope;

        /// <summary>Raised to open the list of dismissals.</summary>
        public Action OpenDismissals;
    }
}