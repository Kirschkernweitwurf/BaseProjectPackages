using System.Runtime.CompilerServices;

// The layout solver behind the monitor window is internal and covered by tests, which is the only
// reason anything outside this assembly reads it. InternalsVisibleTo opens every internal here to
// the test assembly, not only the solver, which is what it does and cannot be narrowed.
[assembly: InternalsVisibleTo("Base.CorePackage.Tests")]