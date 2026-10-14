using System.Runtime.CompilerServices;

// Assembly wide, so it lives at the assembly root rather than inside one file. The draw buffer and
// its commands are implementation detail behind DebugDraw, which is what keeps them internal, and the
// test assembly is the one place outside that has a reason to reach them. InternalsVisibleTo opens
// every internal in Base.CoreDebugPackage.DebugDrawing to it, not only those types, which is what it
// does and cannot be narrowed.
[assembly: InternalsVisibleTo("Base.CoreDebugPackage.Tests")]