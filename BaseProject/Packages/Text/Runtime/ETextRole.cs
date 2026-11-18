namespace Base.TextPackage
{
    /// <summary>
    /// How a kind of text makes room when the player's font, spacing or text size make it bigger than designed.
    /// </summary>
    public enum ETextRole : byte
    {
        /// <summary>The box grows with its text, like settings rows, tooltips and dialogs.</summary>
        Panel = 0,

        /// <summary>
        /// The box keeps its size, like cards, buttons, badges and table labels, so the text shrinks back into it,
        /// down to the style's readable minimum.
        /// </summary>
        Frame = 1
    }
}