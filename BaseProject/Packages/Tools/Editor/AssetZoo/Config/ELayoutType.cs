namespace Base.ToolsPackage.Editor.AssetZoo.Config
{
    /// <summary>
    /// How the assets of one group stand on the floor.
    /// </summary>
    internal enum ELayoutType : byte
    {
        /// <summary>
        /// Rows of a set length, each asset taking only the room it needs.
        /// </summary>
        Rows = 0,

        /// <summary>
        /// All assets in a single row.
        /// </summary>
        Line = 1,

        /// <summary>
        /// A ring. The radius grows when the assets would not fit.
        /// </summary>
        Circle = 2
    }
}