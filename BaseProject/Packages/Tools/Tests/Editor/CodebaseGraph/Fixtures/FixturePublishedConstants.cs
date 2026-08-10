namespace Base.ToolPackage.Editor.Tests.Fixtures
{
    /// <summary>
    /// A const nothing reads, published rather than internal. It exists to pin down a rule that is
    /// otherwise invisible: this assembly ships inside a package, so anything public counts as surface
    /// that consumers may be calling, and an unread one is reported as unused API rather than as dead.
    /// The same const declared internal is dead, and the pair of tests says so.
    /// </summary>
    public static class FixturePublishedConstants
    {
        /// <summary>Read by nothing, here or anywhere a consumer could be.</summary>
        public const string PublishedLabel = "FixturePublishedLabel";
    }
}