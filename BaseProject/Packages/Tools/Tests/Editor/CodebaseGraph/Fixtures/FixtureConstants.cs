namespace Base.ToolsPackage.Editor.Tests.Fixtures
{
    /// <summary>
    /// Constants read from another file. The compiler inlines their values at every call site, so no
    /// instruction ever points back here and only the source text can say they are used.
    /// <br/><br/>
    /// Internal rather than public on purpose. This assembly ships inside a package, so a public const
    /// counts as published API and an unused one is reported as unused API rather than as dead, which
    /// is a different finding from the one the test is about.
    /// </summary>
    internal static class FixtureConstants
    {
        /// <summary>Read from the fixture behaviour, in a different file.</summary>
        internal const string SharedLabel = "FixtureSharedLabel";

        /// <summary>Read nowhere at all.</summary>
        internal const string UnreadLabel = "FixtureUnreadLabel";
    }
}