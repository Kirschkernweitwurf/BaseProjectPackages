using System.Runtime.CompilerServices;

// Assembly wide, so it lives at the assembly root rather than inside one tool's folder. It opens every
// internal in Base.ToolsPackage.Editor to the test assembly, not only the codebase graph, which is what
// InternalsVisibleTo does and cannot be narrowed.
[assembly: InternalsVisibleTo("Base.ToolsPackage.Editor.Tests")]