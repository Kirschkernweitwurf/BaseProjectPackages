namespace Base.ToolPackage.Editor.AssetZoo.Config
{
    /// <summary>
    /// Layout type for the zoo. Grid is a standard grid layout.
    /// Line lays out all items in a single horizontal line, wrapping to multiple lines if needed.
    /// Circle lays out items in a circle around the center of the zoo.
    /// </summary>
    public enum ELayoutType : byte
    {
        /// <summary>
        /// Standard grid layout, with configurable columns and spacing.
        /// </summary>
        Grid = 0,

        /// <summary>
        /// All items are laid out in a single horizontal line, with configurable spacing.
        /// If there are too many items to fit in one line, it wraps to multiple lines.
        /// </summary>
        Line = 1,

        /// <summary>
        /// Items are laid out in a circle around the center of the zoo.
        /// The radius is configurable, and auto-grows if items don't fit.
        /// </summary>
        Circle = 2
    }
}