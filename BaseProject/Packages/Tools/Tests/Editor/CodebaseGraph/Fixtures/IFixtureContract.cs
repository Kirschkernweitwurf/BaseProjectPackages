namespace Base.ToolPackage.Editor.Tests.Fixtures
{
    /// <summary>
    /// Contract shapes the liveness rules have to get right. Every member here is implemented by the
    /// fixture behaviour, one implicitly, one explicitly, and one carrying its own body so that nothing
    /// has to implement it at all.
    /// </summary>
    public interface IFixtureContract
    {
        /// <summary>Implemented implicitly by the fixture behaviour and called through the interface.</summary>
        void Implicit();

        /// <summary>Implemented explicitly, so its metadata name is the fully qualified one.</summary>
        void Explicit();

        /// <summary>
        /// Carries its own body, so nothing has to implement it. It is never called, which should read as
        /// an unused contract member rather than as one waiting to be written.
        /// </summary>
        void Describe() { }
    }
}