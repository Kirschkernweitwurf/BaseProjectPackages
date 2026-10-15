using Base.EditorUIPackage.Editor;

namespace Base.DataHubPackage.Editor
{
    /// <summary>
    /// What the Data Hub needs on top of the shared Base window look: its pane widths and minimum size.
    /// Colors, rows, badges and the card all come from <see cref="EditorWindowStyles"/>.
    /// </summary>
    internal sealed class DataHubStyles : EditorWindowStyles
    {
        /// <summary>Space between a cell's edge and the control or text in it.</summary>
        internal const float CellPadding = 4f;

        /// <summary>Width the inspector panel starts at before the user drags it.</summary>
        internal const float DefaultInspectorWidth = 340f;

        /// <summary>Width the type tree starts at before the user drags it.</summary>
        internal const float DefaultTreeWidth = 200f;

        /// <summary>Smallest width of any of the three panes.</summary>
        internal const float MinPaneWidth = 140f;

        /// <summary>Smallest height of the window.</summary>
        internal const float MinWindowHeight = 320f;

        /// <summary>Smallest width of the window, enough for three panes.</summary>
        internal const float MinWindowWidth = 760f;
    }
}