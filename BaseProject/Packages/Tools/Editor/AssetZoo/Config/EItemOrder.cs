namespace Base.ToolsPackage.Editor.AssetZoo.Config
{
    /// <summary>
    /// In which order the assets of a group stand.
    /// </summary>
    internal enum EItemOrder : byte
    {
        /// <summary>
        /// Small ones in the front row and big ones at the back, like a class photo, so nothing hides.
        /// </summary>
        SmallInFront = 0,

        /// <summary>
        /// The order of the list in the window.
        /// </summary>
        AsListed = 1
    }
}