#if UNITY_EDITOR
using System.Collections.Generic;
using Base.UtilityPackage.Logging;
using UnityEditor;

namespace Base.ToolPackage.Editor.MenuManagerWindow
{
    /// <summary>Registers managed entries into the editor menus. Runs on editor load and on demand.</summary>
    [InitializeOnLoad]
    public static class MenuApplier
    {
        private static readonly List<string> RegisteredPaths = new();

        static MenuApplier() => EditorApplication.delayCall += () => Apply(false);

        /// <summary>Rescans, syncs the registry, and re-registers every enabled entry.</summary>
        public static void Apply(bool log)
        {
            Dictionary<string, ResolvedMenu> resolved = MenuScanner.Scan();
            MenuRegistry registry = MenuRegistry.instance;

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

            foreach (MenuGroup group in registry.Groups)
            {
                foreach (MenuEntry entry in group.Entries)
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

                    MenuBridge.Add(entry.Path, entry.Priority, match.Execute, match.Validate);
                    RegisteredPaths.Add(entry.Path);
                    count++;
                }
            }

            registry.Persist();

            if (log)
                CustomLogger.Log($"Menu Manager: registered {count} menu entr(y/ies).", null);
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