using System.Runtime.CompilerServices;

// Assembly wide, so it lives at the assembly root rather than inside one system's folder. The version
// file is implementation detail rather than API, which is what keeps it internal, and the test
// assembly is the one place outside that has a reason to reach it. InternalsVisibleTo opens every
// internal in Base.UIPackage to it, not only that one class, which is what it does and cannot be
// narrowed.
[assembly: InternalsVisibleTo("Base.UIPackage.Tests")]