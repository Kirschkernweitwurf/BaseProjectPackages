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
    /// <summary>The shipped package layout, saved as an asset so it travels with the package.</summary>
    public sealed class MenuRegistry : ScriptableObject
    {
        private const string AssetFileName = "MenuManagerRegistry.asset";
        private const int CurrentSchema = 4;
        private const string LegacyGroupName = "Ungrouped";
        private const string LegacySettingsPath = "ProjectSettings/MenuManagerRegistry.asset";

        private static MenuRegistry _cached;

        [SerializeField]
        private int schemaVersion;

        [SerializeField]
        private int startPriority;

        [SerializeField]
        private int menuItemStart;

        [SerializeField]
        private int createAssetStart;

        [SerializeField]
        private float columnFileWidth = 130f;

        [SerializeField]
        private float columnPriorityWidth = 72f;

        [SerializeField]
        private float columnStatusWidth = 62f;

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
            set => columnStatusWidth = Mathf.Clamp(value, 44f, 400f);
        }

        [NonSerialized]
        private bool? readOnly;

        /// <summary>Returns the top level node list for the given kind.</summary>
        public List<MenuNode> RootFor(EMenuEntryKind kind) => kind == EMenuEntryKind.CreateAsset
            ? createAssetRoot
            : menuItemRoot;

        /// <summary>Priority given to the first entry of a kind. Each kind is its own menu and numbers independently.</summary>
        public int StartFor(EMenuEntryKind kind) => kind == EMenuEntryKind.CreateAsset
            ? createAssetStart
            : menuItemStart;

        /// <summary>Sets the priority given to the first entry of a kind.</summary>
        public void SetStart(EMenuEntryKind kind, int value)
        {
            if (kind == EMenuEntryKind.CreateAsset)
                createAssetStart = value;
            else
                menuItemStart = value;
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

                if (found == null)
                    continue;

                found.Migrate();
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
                    entryNode.Entry.Path = path[prefix.Length..];
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

        private void Migrate()
        {
            bool hasLegacy = groups.Count > 0 || menuItemGroups.Count > 0 || createAssetGroups.Count > 0;

            if (!hasLegacy && schemaVersion >= CurrentSchema)
                return;

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

            if (schemaVersion < 2)
                NormalizeForPaths();

            if (schemaVersion < 4)
            {
                menuItemStart = startPriority;
                createAssetStart = startPriority;
            }

            schemaVersion = CurrentSchema;
            Persist();
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

        private void NormalizeForPaths()
        {
            DissolveLegacyGroup(menuItemRoot);
            DissolveLegacyGroup(createAssetRoot);
            StripAssetPrefix(createAssetRoot);
        }

        private MenuGroup GetLegacyGroup(EMenuEntryKind kind, string groupName)
        {
            List<MenuGroup> list = kind == EMenuEntryKind.CreateAsset
                ? createAssetGroups
                : menuItemGroups;

            foreach (MenuGroup group in list)
            {
                if (group.Name == groupName)
                    return group;
            }

            MenuGroup created = new(groupName);
            list.Add(created);
            return created;
        }
    }
}
#endif