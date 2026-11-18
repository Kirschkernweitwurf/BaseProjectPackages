namespace Base.TextPackage
{
    /// <summary>
    /// A part of the screen that zooms as a whole with the player's text size, like a phone frame or a grid of menu
    /// cards. Texts inside find it and let it carry their growth: they keep their designed size within the part and may
    /// shrink as far as the zoom grows them back on screen.
    /// </summary>
    public interface ITextZoom
    {
        /// <summary>How much the part is shown bigger than designed: 1 at the designed text size.</summary>
        float Zoom { get; }
    }
}