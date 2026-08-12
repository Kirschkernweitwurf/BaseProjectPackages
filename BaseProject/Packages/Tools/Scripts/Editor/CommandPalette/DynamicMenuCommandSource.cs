using System;
using System.Collections.Generic;
using Base.ToolPackage.Editor.MenuManagerWindows;

namespace Base.ToolPackage.Editor.CommandPalette
{
    /// <summary>
    /// Collects the entries arranged in the menu manager. They are registered at runtime and are
    /// therefore invisible to an attribute scan, so they need their own source. Entries that are
    /// switched off or whose code is gone are skipped, because they cannot be run.
    /// </summary>
    internal sealed class DynamicMenuCommandSource : ICommandSource
    {
        /// <inheritdoc/>
        public void Collect(List<CommandEntry> entries)
        {
            Dictionary<string, ResolvedMenu> resolved = MenuScanner.Scan();
            MenuComposite.Recalculate();

            foreach ((MenuEntry entry, string path) in MenuComposite.ResolvedEntries(EMenuEntryKind.MenuItem))
                TryAddMenuItem(entries, entry, path, resolved);

            foreach ((MenuEntry entry, string path) in MenuComposite.ResolvedEntries(EMenuEntryKind.CreateAsset))
                TryAddCreateAsset(entries, entry, path, resolved);
        }

        private static bool IsRunnable(MenuEntry entry, string path) => entry.Enabled
            && !entry.Missing
            && !string.IsNullOrWhiteSpace(path);

        private static void TryAddMenuItem(List<CommandEntry> entries, MenuEntry entry, string path,
            IReadOnlyDictionary<string, ResolvedMenu> resolved)
        {
            if (!IsRunnable(entry, path))
                return;

            if (!resolved.TryGetValue(entry.Id, out ResolvedMenu match) || match.Execute == null)
                return;

            entries.Add(new CommandEntry(entry.Id, path, match.DeclaringType, ECommandKind.MenuItem,
                EMenuDefinition.Dynamic, AssemblyOriginLookup.Classify(match.DeclaringType), match.Execute));
        }

        private static void TryAddCreateAsset(List<CommandEntry> entries, MenuEntry entry, string path,
            IReadOnlyDictionary<string, ResolvedMenu> resolved)
        {
            if (!IsRunnable(entry, path))
                return;

            if (!resolved.TryGetValue(entry.Id, out ResolvedMenu match) || match.AssetType == null)
                return;

            Type type = match.AssetType;
            string fileName = string.IsNullOrWhiteSpace(entry.CreateFileName)
                ? match.DefaultFileName
                : entry.CreateFileName;

            entries.Add(new CommandEntry(entry.Id, path, type, ECommandKind.CreateAsset, EMenuDefinition.Dynamic,
                AssemblyOriginLookup.Classify(type), () => MenuAssetCreator.Create(type, fileName)));
        }
    }
}