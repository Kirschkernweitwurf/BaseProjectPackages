using System.Runtime.CompilerServices;

// Assembly wide, so it lives at the assembly root rather than inside one of the folders below it. It
// opens every internal in this assembly to the test assembly, not only the types a test names, which
// is what InternalsVisibleTo does and cannot be narrowed.
[assembly: InternalsVisibleTo("Base.ToolsPackage.Editor.Tests")]