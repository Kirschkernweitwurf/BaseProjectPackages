#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Base.ToolPackage.Editor.MenuManagerWindow
{
    /// <summary>Central store of all managed entries as a nested tree, saved as an asset so it ships with the package.</summary>
    public sealed class MenuRegistry : ScriptableObject
    {
        private const string AssetFileName = "MenuManagerRegistry.asset";
        private const int CurrentSchema = 2;
        private const string LegacyGroupName = "Ungrouped";
        private const string LegacySettingsPath = "ProjectSettings/MenuManagerRegistry.asset";

        private static MenuRegistry _cached;

        [SerializeField]
        private int schemaVersion;

        [SerializeField]
        private int startPriority;

        [SerializeField]
        private int separatorGap = 11;

        [SerializeField]
        private float columnFileWidth = 130f;

        [SerializeField]
        private float columnPriorityWidth = 72f;

        [SerializeField]
        private float columnStatusWidth = 52f;

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

        /// <summary>The shared registry instance, loaded from or created as an asset in the package folder.</summary>
        public static MenuRegistry Instance
        {
            get
            {
                if (_cached != null)
                    return _cached;

                _cached = LoadOrCreate();
                return _cached;
            }
        }

        /// <summary>True when the asset lives in an immutable package and must not be edited.</summary>
        public bool IsReadOnly
        {
            get
            {
                readOnly ??= ComputeReadOnly();
                return readOnly.Value;
            }
        }

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

        /// <summary>Width of the file name column.</summary>
        public float ColumnFileWidth
        {
            get => columnFileWidth;
            set => columnFileWidth = Mathf.Clamp(value, 36f, 400f);
        }

        /// <summary>Width of the priority column.</summary>
        public float ColumnPriorityWidth
        {
            get => columnPriorityWidth;
            set => columnPriorityWidth = Mathf.Clamp(value, 36f, 400f);
        }

        /// <summary>Width of the state column.</summary>
        public float ColumnStatusWidth
        {
            get => columnStatusWidth;
            set => columnStatusWidth = Mathf.Clamp(value, 36f, 400f);
        }

        [NonSerialized]
        private bool? readOnly;

        private int walkPriority;
        private bool walkFirst;
        private bool walkPendingGap;

        /// <summary>Returns the top level node list for the given kind.</summary>
        public List<MenuNode> RootFor(EMenuEntryKind kind) => kind == EMenuEntryKind.CreateAsset
            ? createAssetRoot
            : menuItemRoot;

        /// <summary>Moves legacy data into the tree and normalizes it for the current path model. Runs once.</summary>
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

            if (schemaVersion < CurrentSchema)
            {
                NormalizeForPaths();
                schemaVersion = CurrentSchema;
            }

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

                RootFor(pair.Value.Kind).Add(new MenuEntryNode(entry));
            }
        }

        /// <summary>Recomputes derived priorities for both kinds independently.</summary>
        public void RecalculatePriorities()
        {
            WalkPriorities(menuItemRoot);
            WalkPriorities(createAssetRoot);
        }

        /// <summary>Returns every entry of a kind paired with its full resolved menu path, in tree order.</summary>
        public List<(MenuEntry entry, string path)> ResolvedEntriesFor(EMenuEntryKind kind)
        {
            List<(MenuEntry, string)> result = new();
            List<string> prefix = new();
            string root = MenuPath.Prefix(kind);

            if (!string.IsNullOrEmpty(root))
                prefix.Add(root);

            CollectPaths(RootFor(kind), prefix, result);
            return result;
        }

        /// <summary>Writes the asset to disk unless it is read only.</summary>
        public void Persist()
        {
            if (IsReadOnly)
                return;

            string path = AssetDatabase.GetAssetPath(this);

            if (string.IsNullOrEmpty(path))
                return;

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }

        private static MenuRegistry LoadOrCreate()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:" + nameof(MenuRegistry)))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MenuRegistry found = AssetDatabase.LoadAssetAtPath<MenuRegistry>(path);

                if (found != null)
                    return found;
            }

            MenuRegistry instance = LoadLegacySettings();

            if (instance == null)
                instance = CreateInstance<MenuRegistry>();

            instance.hideFlags = HideFlags.None;
            instance.name = "MenuManagerRegistry";

            string folder = ResolveScriptFolder();
            string assetPath = folder + "/" + AssetFileName;

            try
            {
                AssetDatabase.CreateAsset(instance, assetPath);
                AssetDatabase.SaveAssets();
            }
            catch (Exception)
            {
                // Creation can fail in an immutable project. The in-memory instance still works for the session.
            }

            instance.Migrate();
            return instance;
        }

        private static MenuRegistry LoadLegacySettings()
        {
            if (!File.Exists(LegacySettingsPath))
                return null;

            try
            {
                Object[] objects = InternalEditorUtility.LoadSerializedFileAndForget(LegacySettingsPath);

                foreach (Object candidate in objects)
                {
                    if (candidate is MenuRegistry legacy)
                        return legacy;
                }
            }
            catch (Exception)
            {
                return null;
            }

            return null;
        }

        private static string ResolveScriptFolder()
        {
            MenuRegistry probe = CreateInstance<MenuRegistry>();
            MonoScript script = MonoScript.FromScriptableObject(probe);
            string scriptPath = AssetDatabase.GetAssetPath(script);
            DestroyImmediate(probe);

            if (string.IsNullOrEmpty(scriptPath))
                return "Assets";

            string folder = Path.GetDirectoryName(scriptPath);
            return string.IsNullOrEmpty(folder)
                ? "Assets"
                : folder.Replace("\\", "/");
        }

        private static void CollectPaths(List<MenuNode> nodes, List<string> prefix, List<(MenuEntry, string)> result)
        {
            foreach (MenuNode node in nodes)
            {
                if (node is MenuGroupNode group)
                {
                    prefix.Add(group.Name);
                    CollectPaths(group.Children, prefix, result);
                    prefix.RemoveAt(prefix.Count - 1);
                }
                else if (node is MenuEntryNode entryNode)
                {
                    prefix.Add(entryNode.Entry.Path);
                    result.Add((entryNode.Entry, MenuPath.Combine(prefix)));
                    prefix.RemoveAt(prefix.Count - 1);
                }
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

        private static void DissolveLegacyGroup(List<MenuNode> root)
        {
            List<MenuNode> flat = new();

            foreach (MenuNode node in root)
            {
                if (node is MenuGroupNode group && group.Name == LegacyGroupName)
                    flat.AddRange(group.Children);
                else
                    flat.Add(node);
            }

            root.Clear();
            root.AddRange(flat);
        }

        private static void StripAssetPrefix(List<MenuNode> nodes)
        {
            const string prefix = MenuPath.AssetRoot + "/";

            foreach (MenuNode node in nodes)
            {
                if (node is MenuGroupNode group)
                {
                    StripAssetPrefix(group.Children);
                    continue;
                }

                if (node is not MenuEntryNode entryNode)
                    continue;

                string path = entryNode.Entry.Path;

                if (path == MenuPath.AssetRoot)
                    entryNode.Entry.Path = string.Empty;
                else if (path != null && path.StartsWith(prefix, StringComparison.Ordinal))
                    entryNode.Entry.Path = path.Substring(prefix.Length);
            }
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

        private bool ComputeReadOnly()
        {
            string path = AssetDatabase.GetAssetPath(this);

            if (string.IsNullOrEmpty(path))
                return false;

            PackageInfo package = PackageInfo.FindForAssetPath(path);

            if (package != null && package.source != PackageSource.Embedded && package.source != PackageSource.Local)
                return true;

            try
            {
                string full = Path.GetFullPath(path);

                if (File.Exists(full) && (File.GetAttributes(full) & FileAttributes.ReadOnly) != 0)
                    return true;
            }
            catch (Exception)
            {
                return false;
            }

            return false;
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

        private void NormalizeForPaths()
        {
            DissolveLegacyGroup(menuItemRoot);
            DissolveLegacyGroup(createAssetRoot);
            StripAssetPrefix(createAssetRoot);
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