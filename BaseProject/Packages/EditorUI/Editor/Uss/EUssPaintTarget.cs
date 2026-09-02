namespace Base.EditorUIPackage.Editor
{
    /// <summary>
    /// Which part of an element a painted color lands on.
    /// </summary>
    public enum EUssPaintTarget : byte
    {
        /// <summary>The fill behind the element.</summary>
        Background = 0,

        /// <summary>The outline of the element, on all four sides.</summary>
        Border = 1,

        /// <summary>The text drawn in the element.</summary>
        Text = 2
    }
}