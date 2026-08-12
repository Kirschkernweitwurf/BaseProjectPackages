using System;
using System.Collections.Generic;
using Base.UtilityPackage.Logging;
using UnityEditor;

namespace Base.ToolPackage.Editor.MenuManagerWindows
{
    /// <summary>Registers managed entries into the editor menus. Runs on editor load and on demand.</summary>
    [InitializeOnLoad]
    internal static class MenuApplier
    {
        private const int MaxWaitTicks = 200;

        private static readonly List<string> RegisteredPaths = new();

        private static int _waitTicks;

        static MenuApplier() => Schedule();

        /// <summary>Rescans, syncs both stores, and re-registers every enabled entry of both kinds.</summary>
        public static void Apply(bool log)
        {
            Dictionary<string, ResolvedMenu> resolved = MenuScanner.Scan();
            MenuComposite.Sync(resolved);

            if (!MenuBridge.IsAvailable)
                return;

            RemoveAll();

            HashSet<string> usedPaths = new();
            HashSet<string> usedIds = new();
            int count = 0;

            count += Register(MenuComposite.ResolvedEntries(EMenuEntryKind.MenuItem), resolved, usedPaths, usedIds,
                log);

            count += Register(MenuComposite.ResolvedEntries(EMenuEntryKind.CreateAsset), resolved, usedPaths, usedIds,
                log);

            if (!log)
                return;

            string entryPlural = count == 1
                ? "entry"
                : "entries";

            CustomLogger.Log($"Menu Manager: registered {count} menu {entryPlural}.", null);
        }

        /// <summary>Queues a registration pass for once the editor has finished loading.</summary>
        private static void Schedule()
        {
            _waitTicks = 0;
            EditorApplication.delayCall += ApplyWhenReady;
        }

        private static void ApplyWhenReady()
        {
            bool busy = EditorApplication.isCompiling || EditorApplication.isUpdating;

            if (busy && _waitTicks < MaxWaitTicks)
            {
                _waitTicks++;
                EditorApplication.delayCall += ApplyWhenReady;
                return;
            }

            Apply(false);
        }

        private static int Register(List<(MenuEntry entry, string path)> entries,
            IReadOnlyDictionary<string, ResolvedMenu> resolved,
            HashSet<string> usedPaths, HashSet<string> usedIds, bool log)
        {
            int count = 0;

            foreach ((MenuEntry entry, string path) in entries)
            {
                if (!entry.Enabled
                    || entry.Missing
                    || string.IsNullOrWhiteSpace(path))
                    continue;

                if (!resolved.TryGetValue(entry.Id, out ResolvedMenu match))
                    continue;

                if (!usedIds.Add(entry.Id))
                    continue;

                if (!usedPaths.Add(path))
                {
                    if (log)
                        CustomLogger.LogWarning($"Menu Manager: duplicate path '{path}' skipped.", null);

                    continue;
                }

                Action execute = BuildExecute(entry, match);
                MenuBridge.Add(path, entry.EffectivePriority, execute, match.Validate);
                RegisteredPaths.Add(path);
                count++;
            }

            return count;
        }

        private static Action BuildExecute(MenuEntry entry, ResolvedMenu match)
        {
            if (match.Kind != EMenuEntryKind.CreateAsset)
                return match.Execute;

            Type type = match.AssetType;
            string fileName = string.IsNullOrWhiteSpace(entry.CreateFileName)
                ? match.DefaultFileName
                : entry.CreateFileName;

            return () => MenuAssetCreator.Create(type, fileName);
        }

        private static void RemoveAll()
        {
            foreach (string path in RegisteredPaths)
                MenuBridge.Remove(path);

            RegisteredPaths.Clear();
        }
    }
}