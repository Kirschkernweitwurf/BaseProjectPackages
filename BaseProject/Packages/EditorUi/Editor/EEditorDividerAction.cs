namespace Base.EditorUiPackage
{
    /// <summary>
    /// What a divider did during one event, which is what tells a table when to recompute a column
    /// width and when to write it back to disk.
    /// </summary>
    public enum EEditorDividerAction : byte
    {
        /// <summary>Nothing happened to this divider. Zero, so an unset value means idle.</summary>
        None = 0,

        /// <summary>The divider moved. Recompute the width from the reported mouse position.</summary>
        Dragged = 1,

        /// <summary>The drag ended. Recompute once more and persist the result.</summary>
        Released = 2
    }
}