using System;
using System.Collections.Generic;
using Base.UtilityPackage.Logging;
using UnityEditor;

namespace Base.ToolsPackage.Editor.MenuManagerWindows
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
        internal static void Apply(bool log)
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
                Func<bool> validate = BuildValidate(path, match);

                MenuBridge.Add(path, entry.EffectivePriority, execute, validate);
                RegisteredPaths.Add(path);
                count++;
            }

            return count;
        }

        // The check mark is addressed by path, and only this class knows the path the entry ended up at
        // after the user moved or renamed it. Declaring code therefore never names a path itself: it
        // supplies the state and the mark is applied here, from the validate call Unity makes right
        // before the menu is drawn.
        private static Func<bool> BuildValidate(string path, ResolvedMenu match)
        {
            Func<bool> isChecked = match.Checked;

            if (isChecked == null)
                return match.Validate;

            Func<bool> validate = match.Validate;

            return () =>
            {
                Menu.SetChecked(path, isChecked());

                return validate == null || validate();
            };
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