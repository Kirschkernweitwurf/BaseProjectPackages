namespace Base.ToolPackage.Editor.AssetZoo.Config
{
    /// <summary>
    /// The direction in which to expand a category when adding new items.
    /// </summary>
    public enum ECategoryDirection : byte
    {
        /// <summary>
        /// Expand forward, adding new items in front of existing ones.
        /// </summary>
        Forward = 0,

        /// <summary>
        /// Expand backward, adding new items behind existing ones.
        /// </summary>
        Backward = 1,

        /// <summary>
        /// Expand to the left, adding new items to the left of existing ones.
        /// </summary>
        Left = 2,

        /// <summary>
        /// Expand to the right, adding new items to the right of existing ones.
        /// </summary>
        Right = 3
    }
}