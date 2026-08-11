namespace Base.AttributePackage.Editor.Windows.AttributeGallery
{
    /// <summary>
    /// Demo interface used by the gallery to show the managed reference picker and the interface
    /// reference field. Exists only so those two attributes have something to point at.
    /// </summary>
    public interface IGalleryEffect
    {
        /// <summary>Human-readable name of the effect.</summary>
        string Description { get; }
    }
}