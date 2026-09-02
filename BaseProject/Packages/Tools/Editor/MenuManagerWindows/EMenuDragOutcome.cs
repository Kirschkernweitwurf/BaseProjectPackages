namespace Base.ToolsPackage.Editor.MenuManagerWindows
{
    /// <summary>What a window still has to do after the drag controller handled an event.</summary>
    internal enum EMenuDragOutcome : byte
    {
        /// <summary>Nothing is being dragged, or the event did not concern the drag.</summary>
        None = 0,

        /// <summary>The drag moved on and the window has to repaint.</summary>
        Repaint = 1,

        /// <summary>A node was dropped in its new place, so the trees have to be persisted.</summary>
        Moved = 2
    }
}