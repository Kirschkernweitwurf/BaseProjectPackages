namespace Base.ToolsPackage.Editor.CanvasRenderOrder
{
    /// <summary>
    /// The pass a canvas is drawn in. Unity runs the passes in this order, so every overlay canvas ends up in front
    /// of every camera and world space canvas.
    /// </summary>
    internal enum ECanvasPass : byte
    {
        Camera = 0,
        Overlay = 1
    }
}