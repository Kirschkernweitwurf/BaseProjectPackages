#if UNITY_EDITOR
using System.Collections.Generic;

namespace Base.ToolPackage.Editor.MenuManagerWindow
{
    /// <summary>Coordinates the read only package registry and the writable project overlay as one view.</summary>
    public static class MenuComposite
    {
        private static readonly EMenuEntryKind[] Kinds = { EMenuEntryKind.MenuItem, EMenuEntryKind.CreateAsset };

        private static MenuRegistry Registry => MenuRegistry.Instance;
        private static MenuOverlay Overlay => MenuOverlay.instance;

        /// <summary>Package root first, then overlay root, for the given kind.</summary>
        public static List<List<MenuNode>> RootsFor(EMenuEntryKind kind) =>
            new() { Registry.RootFor(kind), Overlay.RootFor(kind) };

        /// <summary>Discovers new entries, flags missing ones, and routes new entries to the writable store.</summary>
        public static void Sync(IReadOnlyDictionary<string, ResolvedMenu> resolved)
        {
            Overlay.Migrate();

            HashSet<string> known = new();
            HashSet<string> packageIds = new();
            bool changed = false;

            foreach (EMenuEntryKind kind in Kinds)
            {
                changed |= MenuTree.Mark(Registry.RootFor(kind), resolved, known);
                changed |= MenuTree.Mark(Overlay.RootFor(kind), resolved, known);
                MenuTree.CollectIds(Registry.RootFor(kind), packageIds);
            }

            foreach (EMenuEntryKind kind in Kinds)
                changed |= MenuTree.RemoveEntries(Overlay.RootFor(kind), id => packageIds.Contains(id));

            bool locked = Registry.IsReadOnly;

            foreach (KeyValuePair<string, ResolvedMenu> pair in resolved)
            {
                if (known.Contains(pair.Key))
                    continue;

                MenuEntry entry = new(pair.Key, pair.Value.DefaultPath, pair.Value.Kind);

                if (pair.Value.Kind == EMenuEntryKind.CreateAsset)
                    entry.CreateFileName = pair.Value.DefaultFileName;

                List<MenuNode> target = locked ? Overlay.RootFor(pair.Value.Kind) : Registry.RootFor(pair.Value.Kind);
                target.Add(new MenuEntryNode(entry));
                changed = true;
            }

            Recalculate();

            if (changed)
            {
                Registry.Persist();
                Overlay.Persist();
            }
        }

        /// <summary>Recomputes derived priorities across both stores for both kinds.</summary>
        public static void Recalculate()
        {
            foreach (EMenuEntryKind kind in Kinds)
                MenuTree.Priorities(RootsFor(kind), Registry.StartPriority);
        }

        /// <summary>Returns every entry of a kind paired with its full resolved path, package first then overlay.</summary>
        public static List<(MenuEntry entry, string path)> ResolvedEntries(EMenuEntryKind kind)
        {
            List<(MenuEntry, string)> result = new();
            MenuTree.Collect(RootsFor(kind), kind, result);
            return result;
        }
    }
}
#endif
