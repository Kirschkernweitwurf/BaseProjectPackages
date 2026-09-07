using System.Runtime.CompilerServices;

// Assembly wide, so it lives at the assembly root rather than inside one of the folders below it. The
// layout and alignment strategies are internal and the test assembly is the one place outside with a
// reason to reach them.
[assembly: InternalsVisibleTo("Base.ToolsPackage.Editor.Tests")]