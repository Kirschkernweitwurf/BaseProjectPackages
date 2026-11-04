namespace Base.ToolsPackage.Editor.AssetZoo.Config
{
    /// <summary>
    /// How the groups of a zoo stand next to each other.
    /// </summary>
    internal enum EGroupArrangement : byte
    {
        /// <summary>
        /// Blocks like a city map, wrapping so the whole zoo stays roughly square.
        /// </summary>
        Blocks = 0,

        /// <summary>
        /// All groups side by side.
        /// </summary>
        Row = 1,

        /// <summary>
        /// All groups one behind the other.
        /// </summary>
        Column = 2
    }
}