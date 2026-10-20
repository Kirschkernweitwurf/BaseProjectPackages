using System.Runtime.CompilerServices;

// The editor syncs profiles and appliers from the open scene, which needs their internal members.
[assembly: InternalsVisibleTo("Base.ToolsPackage.Editor")]
[assembly: InternalsVisibleTo("Base.ToolsPackage.Editor.Tests")]