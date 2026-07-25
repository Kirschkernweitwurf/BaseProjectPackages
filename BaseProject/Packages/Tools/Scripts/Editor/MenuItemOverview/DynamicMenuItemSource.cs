#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Base.ToolPackage.Editor.MenuManagerWindow;
using Base.ToolPackage.Editor.MenuOverview;
using UnityEditor;

namespace Base.ToolPackage.Editor.MenuItemOverview
{
    /// <summary>
    /// Source that reports every menu item registered through the menu manager, including the
    /// ones that are switched off or whose code has disappeared since the last scan.
    /// </summary>
    public sealed class DynamicMenuItemSource : IMenuItemSource
    {
        private readonly MenuScriptLookup _scripts = new();

        /// <inheritdoc/>
        public IReadOnlyList<MenuItemEntry> Collect()
        {
            _scripts.Clear();
            List<MenuItemEntry> entries = new();

            Dictionary<string, ResolvedMenu> resolved = MenuScanner.Scan();
            MenuComposite.Recalculate();

            foreach ((MenuEntry entry, string path) in MenuComposite.ResolvedEntries(EMenuEntryKind.MenuItem))
            {
                if (string.IsNullOrWhiteSpace(path))
                    continue;

                entries.Add(Build(entry, path, resolved));
            }

            return entries;
        }

        // Ids are built as "MI:{DeclaringType.FullName}.{Method}", so the method is the last segment.
        private static string MethodNameOf(string entryId)
        {
            if (string.IsNullOrEmpty(entryId))
                return string.Empty;

            int separator = entryId.LastIndexOf('.');
            return separator >= 0
                ? entryId[(separator + 1)..]
                : entryId;
        }

        private static EMenuEntryState StateOf(MenuEntry entry)
        {
            if (entry.Missing)
                return EMenuEntryState.Missing;

            return entry.Enabled
                ? EMenuEntryState.Active
                : EMenuEntryState.Disabled;
        }

        private MenuItemEntry Build(MenuEntry entry, string path, IReadOnlyDictionary<string, ResolvedMenu> resolved)
        {
            resolved.TryGetValue(entry.Id, out ResolvedMenu match);
            Type declaringType = match?.DeclaringType;

            MonoScript script = _scripts.Resolve(declaringType);
            string assetPath = MenuScriptLookup.PathOf(script);
            EMenuItemOrigin origin = MenuItemOriginResolver.Classify(assetPath);

            return MenuItemEntry.Managed(entry.Id, path, declaringType, MethodNameOf(entry.Id),
                entry.EffectivePriority, StateOf(entry), origin, script, assetPath);
        }
    }
}
#endif