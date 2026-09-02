namespace Base.ToolsPackage.Editor.Tests.Fixtures
{
    /// <summary>
    /// The other half of the fixture. A liveness tool that reports nothing passes every test about what
    /// is alive, so the suite also has to assert that these are reported.
    /// <br/><br/>
    /// The unused field warnings are switched off rather than fixed, because they are the point: these
    /// fields exist to be unused, and leaving the warnings on would emit them into every project that
    /// installs the package, on every compile, forever.
    /// </summary>
    public sealed class FixtureDeadCode
    {
        /// <summary>Called by the fixture behaviour, so the type itself is reachable.</summary>
        public void Touch() => _writeOnly = 1;

        /// <summary>Called by nothing at all.</summary>
        private void NeverCalled() { }
#pragma warning disable 169, 414
        /// <summary>Assigned and never read, which is a write only field.</summary>
        private int _writeOnly;

        /// <summary>Neither read nor written by anything.</summary>
        private int _untouched;
#pragma warning restore 169, 414
    }
}