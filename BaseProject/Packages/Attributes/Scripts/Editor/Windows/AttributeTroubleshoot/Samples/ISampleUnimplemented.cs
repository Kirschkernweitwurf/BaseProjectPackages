namespace Base.AttributePackage.Editor.Windows.AttributeTroubleshoot.Samples
{
    /// <summary>
    /// Demo interface with no implementation on purpose, so the reference picker has nothing to offer and
    /// the check can report an empty picker.
    /// </summary>
    public interface ISampleUnimplemented
    {
        /// <summary>Never called. The interface exists only to stay empty.</summary>
        void Never();
    }
}