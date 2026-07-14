#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.MenuManagerWindow
{
    /// <summary>Central store of all managed entries as a nested tree, kept separate per kind.</summary>
    [FilePath(FilePathValue, FilePathAttribute.Location.ProjectFolder)]
    public sealed class MenuRegistry : ScriptableSingleton<MenuRegistry>
    {
        private const string DefaultGroupName = "Ungrouped";
        private const string FilePathValue = "ProjectSettings/MenuManagerRegistry.asset";

        [SerializeField]
        private int startPriority;

        [SerializeField]
        private int separatorGap = 11;

        [SerializeReference]
        private List<MenuNode> menuItemRoot = new();

        [SerializeReference]
        private List<MenuNode> createAssetRoot = new();

        [SerializeField]
        private List<MenuGroup> menuItemGroups = new();

        [SerializeField]
        private List<MenuGroup> createAssetGroups = new();

        [SerializeField]
        private List<MenuGroup> groups = new();

        /// <summary>Priority assigned to the first registered entry of each kind.</summary>
        public int StartPriority
        {
            get => startPriority;
            set => startPriority = value;
        }

        /// <summary>Priority gap inserted at group boundaries. A gap of 11 or more draws a separator line.</summary>
        public int SeparatorGap
        {
            get => separatorGap;
            set => separatorGap = Mathf.Max(1, value);
        }

        private int walkPriority;
        private bool walkFirst;
        private bool walkPendingGap;

        /// <summary>Returns the top level node list for the given kind.</summary>
        public List<MenuNode> RootFor(EMenuEntryKind kind) => kind == EMenuEntryKind.CreateAsset
            ? createAssetRoot
            : menuItemRoot;

        /// <summary>Moves legacy flat data into the nested tree. Runs once.</summary>
        public void Migrate()
        {
            if (groups.Count > 0)
            {
                foreach (MenuGroup legacy in groups)
                {
                    foreach (MenuEntry entry in legacy.Entries)
                        GetLegacyGroup(entry.Kind, legacy.Name).Entries.Add(entry);
                }

                groups.Clear();
            }

            ConvertLegacy(menuItemGroups, menuItemRoot);
            ConvertLegacy(createAssetGroups, createAssetRoot);
            Persist();
        }

        /// <summary>Adds newly discovered entries, flags missing ones, and refreshes kinds.</summary>
        public void Sync(IReadOnlyDictionary<string, ResolvedMenu> resolved)
        {
            HashSet<string> known = new();

            MarkTree(menuItemRoot, resolved, known);
            MarkTree(createAssetRoot, resolved, known);

            foreach (KeyValuePair<string, ResolvedMenu> pair in resolved)
            {
                if (known.Contains(pair.Key))
                    continue;

                MenuEntry entry = new(pair.Key, pair.Value.DefaultPath, pair.Value.Kind);

                if (pair.Value.Kind == EMenuEntryKind.CreateAsset)
                    entry.CreateFileName = pair.Value.DefaultFileName;

                GetDefaultGroup(RootFor(pair.Value.Kind)).Children.Add(new MenuEntryNode(entry));
            }
        }

        /// <summary>Recomputes derived priorities for both kinds independently.</summary>
        public void RecalculatePriorities()
        {
            WalkPriorities(menuItemRoot);
            WalkPriorities(createAssetRoot);
        }

        /// <summary>Enumerates every entry of a kind in tree order.</summary>
        public IEnumerable<MenuEntry> EntriesFor(EMenuEntryKind kind) => Flatten(RootFor(kind));

        /// <summary>Writes the in-memory state to disk.</summary>
        public void Persist() => Save(true);

        private static IEnumerable<MenuEntry> Flatten(List<MenuNode> nodes)
        {
            foreach (MenuNode node in nodes)
            {
                if (node is MenuEntryNode entryNode)
                    yield return entryNode.Entry;
                else if (node is MenuGroupNode group)
                    foreach (MenuEntry entry in Flatten(group.Children))
                        yield return entry;
            }
        }

        private static void MarkTree(List<MenuNode> nodes, IReadOnlyDictionary<string, ResolvedMenu> resolved,
            HashSet<string> known)
        {
            foreach (MenuNode node in nodes)
            {
                if (node is MenuGroupNode group)
                {
                    MarkTree(group.Children, resolved, known);
                    continue;
                }

                if (node is not MenuEntryNode entryNode)
                    continue;

                MenuEntry entry = entryNode.Entry;
                known.Add(entry.Id);
                bool present = resolved.TryGetValue(entry.Id, out ResolvedMenu match);
                entry.Missing = !present;

                if (!present)
                    continue;

                entry.Kind = match.Kind;

                if (match.Kind == EMenuEntryKind.CreateAsset && string.IsNullOrWhiteSpace(entry.CreateFileName))
                    entry.CreateFileName = match.DefaultFileName;
            }
        }

        private static MenuGroupNode GetDefaultGroup(List<MenuNode> root)
        {
            foreach (MenuNode node in root)
            {
                if (node is MenuGroupNode group && group.Name == DefaultGroupName)
                    return group;
            }

            MenuGroupNode created = new(DefaultGroupName);
            root.Add(created);
            return created;
        }

        private static void ConvertLegacy(List<MenuGroup> legacy, List<MenuNode> root)
        {
            if (legacy.Count == 0)
                return;

            foreach (MenuGroup group in legacy)
            {
                MenuGroupNode node = new(group.Name);

                foreach (MenuEntry entry in group.Entries)
                    node.Children.Add(new MenuEntryNode(entry));

                root.Add(node);
            }

            legacy.Clear();
        }

        private void WalkPriorities(List<MenuNode> nodes)
        {
            walkPriority = startPriority;
            walkFirst = true;
            walkPendingGap = false;
            WalkNodes(nodes);
        }

        private void WalkNodes(List<MenuNode> nodes)
        {
            foreach (MenuNode node in nodes)
            {
                if (node is MenuGroupNode group)
                {
                    walkPendingGap = true;
                    WalkNodes(group.Children);
                    walkPendingGap = true;
                }
                else if (node is MenuEntryNode entryNode)
                {
                    EmitPriority(entryNode.Entry);
                }
            }
        }

        private void EmitPriority(MenuEntry entry)
        {
            if (!entry.Enabled || entry.Missing || string.IsNullOrWhiteSpace(entry.Path))
            {
                entry.Priority = int.MinValue;
                return;
            }

            if (walkFirst)
                walkFirst = false;
            else if (walkPendingGap)
                walkPriority += separatorGap;

            walkPendingGap = false;
            entry.Priority = walkPriority;
            walkPriority++;
        }

        private MenuGroup GetLegacyGroup(EMenuEntryKind kind, string name)
        {
            List<MenuGroup> list = kind == EMenuEntryKind.CreateAsset
                ? createAssetGroups
                : menuItemGroups;

            foreach (MenuGroup group in list)
            {
                if (group.Name == name)
                    return group;
            }

            MenuGroup created = new(name);
            list.Add(created);
            return created;
        }
    }
}
#endif