using System.Runtime.CompilerServices;

// Assembly wide, so it lives at the assembly root rather than inside one of the folders below it.
// The sort triangle geometry is internal and the test assembly is the one place outside with a
// reason to reach it.
[assembly: InternalsVisibleTo("Base.EditorUIPackage.Editor.Tests")]