namespace Base.ToolsPackage.Editor.CanvasRenderOrder
{
    /// <summary>
    /// Where a listed canvas lives. This decides how a change to its sorting is written back.
    /// </summary>
    internal enum ECanvasSource : byte
    {
        Scene = 0,
        Prefab = 1
    }
}