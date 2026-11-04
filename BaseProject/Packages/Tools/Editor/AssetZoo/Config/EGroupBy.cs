namespace Base.ToolsPackage.Editor.AssetZoo.Config
{
    /// <summary>
    /// What decides which group an asset lands in.
    /// </summary>
    internal enum EGroupBy : byte
    {
        /// <summary>
        /// The folder the asset is in, which is how art is usually sorted already.
        /// </summary>
        Folder = 0,

        /// <summary>
        /// The first word of the asset's name behind its prefix.
        /// </summary>
        Name = 1
    }
}