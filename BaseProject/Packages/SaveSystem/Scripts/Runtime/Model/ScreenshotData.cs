namespace Base.SaveSystemPackage.Model
{
    /// <summary>
    /// A save thumbnail as already-encoded PNG bytes plus its size.
    /// </summary>
    public readonly struct ScreenshotData
    {
        public byte[] Png { get; }
        public int Width { get; }
        public int Height { get; }

        public ScreenshotData(byte[] png, int width, int height)
        {
            Png = png;
            Width = width;
            Height = height;
        }
    }
}