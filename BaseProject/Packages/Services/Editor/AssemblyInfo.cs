using System.Runtime.CompilerServices;

// Assembly wide, so it lives at the assembly root rather than beside one window. The runtime half of
// this package opens itself to this assembly; this is the same step one level further out, so the
// column layout the whole service window is built on can be named by a test.
[assembly: InternalsVisibleTo("Base.ServicesPackage.Tests")]