#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.MenuManagerWindow
{
    /// <summary>Registers managed entries into the editor menus. Runs on editor load and on demand.</summary>
    [InitializeOnLoad]
    public static class MenuApplier
    {
        private static readonly List<string> RegisteredPaths = new();

        static MenuApplier() => EditorApplication.delayCall += () => Apply(false);

        /// <summary>Rescans, syncs the registry, and re-registers every enabled entry of both kinds.</summary>
        public static void Apply(bool log)
        {
            Dictionary<string, ResolvedMenu> resolved = MenuScanner.Scan();
            MenuRegistry registry = MenuRegistry.instance;

            registry.Migrate();
            registry.Sync(resolved);
            registry.RecalculatePriorities();

            if (!MenuBridge.IsAvailable)
            {
                registry.Persist();
                return;
            }

            RemoveAll();

            HashSet<string> usedPaths = new();
            int count = 0;

            count += Register(registry.EntriesFor(EMenuEntryKind.MenuItem), resolved, usedPaths, log);
            count += Register(registry.EntriesFor(EMenuEntryKind.CreateAsset), resolved, usedPaths, log);

            registry.Persist();

            if (log)
                CustomLogger.Log($"Menu Manager: registered {count} menu entr(y/ies).", null);
        }

        private static int Register(IEnumerable<MenuEntry> entries, IReadOnlyDictionary<string, ResolvedMenu> resolved,
            HashSet<string> usedPaths, bool log)
        {
            int count = 0;

            foreach (MenuEntry entry in entries)
            {
                if (!entry.Enabled || entry.Missing || string.IsNullOrWhiteSpace(entry.Path))
                    continue;

                if (!resolved.TryGetValue(entry.Id, out ResolvedMenu match))
                    continue;

                if (!usedPaths.Add(entry.Path))
                {
                    if (log)
                        CustomLogger.LogWarning($"Menu Manager: duplicate path '{entry.Path}' skipped.", null);

                    continue;
                }

                Action execute = BuildExecute(entry, match);
                MenuBridge.Add(entry.Path, entry.Priority, execute, match.Validate);
                RegisteredPaths.Add(entry.Path);
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

            return () =>
            {
                ScriptableObject instance = ScriptableObject.CreateInstance(type);
                ProjectWindowUtil.CreateAsset(instance, fileName + ".asset");
            };
        }

        private static void RemoveAll()
        {
            foreach (string path in RegisteredPaths)
                MenuBridge.Remove(path);

            RegisteredPaths.Clear();
        }
    }
}
#endif