using System.Collections.Generic;

namespace Base.ToolsPackage.Editor.CommandPalette
{
    /// <summary>
    /// Supplies one group of palette commands. Every source appends into the same list so a full
    /// index pass allocates one collection instead of one per source.
    /// </summary>
    internal interface ICommandSource
    {
        /// <summary>Appends every command this source knows about.</summary>
        /// <param name="entries">The list the commands are added to.</param>
        void Collect(List<CommandEntry> entries);
    }
}