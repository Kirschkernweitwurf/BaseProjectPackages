namespace Base.ToolPackage.Editor.Tests.Fixtures
{
    /// <summary>
    /// A contract nothing implements. It sits on its own interface rather than alongside the implemented
    /// ones, because putting an unimplemented member on an interface a class declares is a compile error
    /// rather than a finding, and the test assembly would never build to say so.
    /// </summary>
    public interface IFixtureOrphan
    {
        /// <summary>Declared here, implemented by nobody, and never called.</summary>
        void Orphaned();
    }
}