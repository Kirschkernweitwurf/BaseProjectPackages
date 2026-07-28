namespace Base.SaveSystemPackage.Model
{
    /// <summary>
    /// A save thumbnail as already-encoded PNG bytes plus its size.
    /// </summary>
    public readonly struct ScreenshotData
    {
        /// <summary>The thumbnail, already encoded to PNG by the caller.</summary>
        public byte[] Png { get; }

        /// <summary>Width of the thumbnail in pixels.</summary>
        public int Width { get; }

        /// <summary>Height of the thumbnail in pixels.</summary>
        public int Height { get; }

        /// <summary>Whether this actually carries image data worth writing to disk.</summary>
        public bool IsValid => Png != null && Png.Length > 0;

        /// <param name="png">The thumbnail, already encoded to PNG.</param>
        /// <param name="width">Width in pixels.</param>
        /// <param name="height">Height in pixels.</param>
        public ScreenshotData(byte[] png, int width, int height)
        {
            Png = png;
            Width = width;
            Height = height;
        }
    }
}