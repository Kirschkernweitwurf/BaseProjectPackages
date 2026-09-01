using System.Collections.Generic;

namespace Base.ToolPackage.Editor.CommandPalette
{
    /// <summary>
    /// Builds and caches the full list of palette commands. The cache lives until the next domain
    /// reload or an explicit rebuild, so opening the palette a second time is instant.
    /// </summary>
    internal static class CommandIndex
    {
        /// <summary>Every known command, built on first access.</summary>
        internal static IReadOnlyList<CommandEntry> Entries => _entries ??= Build();

        private static readonly ICommandSource[] Sources =
        {
            new MenuItemCommandSource(),
            new CreateAssetCommandSource(),
            new DynamicMenuCommandSource(),
            new SettingsPageCommandSource()
        };

        private static List<CommandEntry> _entries;

        /// <summary>Drops the cache so the next access scans the project again.</summary>
        internal static void Invalidate()
        {
            _entries = null;
            AssemblyOriginLookup.Clear();
        }

        private static List<CommandEntry> Build()
        {
            List<CommandEntry> collected = new();

            foreach (ICommandSource source in Sources)
                source.Collect(collected);

            return Deduplicate(collected);
        }

        // Several methods can claim the same menu path. Unity shows one entry for them, so the
        // palette does the same and keeps the first one it found.
        private static List<CommandEntry> Deduplicate(List<CommandEntry> collected)
        {
            HashSet<(ECommandKind, string)> seen = new();
            List<CommandEntry> unique = new(collected.Count);

            foreach (CommandEntry entry in collected)
            {
                if (seen.Add((entry.Kind, entry.Path)))
                    unique.Add(entry);
            }

            return unique;
        }
    }
}