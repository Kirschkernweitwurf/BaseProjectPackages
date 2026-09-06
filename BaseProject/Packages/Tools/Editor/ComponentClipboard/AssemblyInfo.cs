using System.Runtime.CompilerServices;

// Assembly wide, so it lives at the assembly root rather than inside one tool's folder. The clipboard,
// its entries and the operations on them are all internal, and the test assembly is the one place
// outside this assembly with a reason to reach them.
[assembly: InternalsVisibleTo("Base.ToolsPackage.Editor.Tests")]