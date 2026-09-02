namespace Base.ToolsPackage.Editor.Tests.Fixtures
{
    /// <summary>
    /// A type whose only job is holding nested types. Every use is written against the nested type, and
    /// a const inside it leaves no trace in the compiled code at all, so the outer type is referenced by
    /// nothing and declares nothing. It is alive for exactly as long as what it contains.
    /// </summary>
    internal static class FixtureNestingHost
    {
        /// <summary>Read from the fixture behaviour, in another file.</summary>
        internal static class Metrics
        {
            /// <summary>Read from the fixture behaviour, in another file.</summary>
            internal const int Padding = 4;
        }

        /// <summary>Read by nothing, so this one really is dead.</summary>
        internal static class Unused
        {
            /// <summary>Read by nothing.</summary>
            internal const int Ignored = 8;
        }
    }
}