using System.Runtime.CompilerServices;

// The menu manager is one unit that was split across assemblies so a change to a window stops
// recompiling the tools that only read the model. Its consumers are named here rather than the
// model being made public, because three assemblies inside one package reading a store is not a
// published API, and the list is bounded by what the menu manager is.
[assembly: InternalsVisibleTo("Base.ToolsPackage.Editor.CommandPalette")]
[assembly: InternalsVisibleTo("Base.ToolsPackage.Editor.MenuManagerWindows")]
[assembly: InternalsVisibleTo("Base.ToolsPackage.Editor.NamingConventions")]