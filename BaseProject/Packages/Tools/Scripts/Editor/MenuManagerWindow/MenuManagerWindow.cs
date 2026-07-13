#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Base.ToolPackage.Editor.MenuManagerWindow
{
    /// <summary>Window to arrange menu items and asset creation entries into groups without generated code.</summary>
    public sealed class MenuManagerWindow : EditorWindow
    {
        private const float GroupWidth = 130f;
        private const float KindWidth = 78f;
        private const float Pad = 4f;
        private const float PriorityWidth = 44f;
        private const float RowSpacing = 2f;
        private const float ToggleWidth = 16f;
        private const string WindowTitle = "Menu Manager";
        private readonly List<ReorderableList> entryLists = new();

        private MenuRegistry registry;
        private Dictionary<string, ResolvedMenu> resolved = new();
        private bool listsDirty = true;
        private Action pendingOperation;
        private Vector2 scroll;

#region Unity Callbacks
        private void OnEnable()
        {
            registry = MenuRegistry.instance;
            RefreshScan();
        }

        private void OnGUI()
        {
            if (registry == null)
                return;

            DrawToolbar();
            registry.RecalculatePriorities();

            if (listsDirty)
                BuildLists();

            scroll = EditorGUILayout.BeginScrollView(scroll);

            for (int i = 0; i < registry.Groups.Count; i++)
                DrawGroup(i);

            EditorGUILayout.EndScrollView();
            DrawFooter();

            if (pendingOperation == null)
                return;

            pendingOperation.Invoke();
            pendingOperation = null;
            registry.Persist();
            listsDirty = true;
            Repaint();
        }
#endregion

        [MenuItem("Tools/Base Packages/Menu Management/Menu Manager")]
        private static void Open()
        {
            MenuManagerWindow window = GetWindow<MenuManagerWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(520f, 360f);
            window.Show();
        }

        private static string KindLabel(MenuEntry entry)
        {
            if (entry.Missing)
                return "missing";

            return entry.Kind == EMenuEntryKind.CreateAsset
                ? "asset"
                : "menu";
        }

        private static string PriorityLabel(MenuEntry entry) => entry.Priority == int.MinValue
            ? "-"
            : entry.Priority.ToString();

        private void RefreshScan()
        {
            resolved = MenuScanner.Scan();
            registry.Sync(resolved);
            registry.RecalculatePriorities();
            registry.Persist();
            listsDirty = true;
        }

        private void BuildLists()
        {
            entryLists.Clear();

            foreach (MenuGroup group in registry.Groups)
                entryLists.Add(CreateList(group));

            listsDirty = false;
        }

        private ReorderableList CreateList(MenuGroup group)
        {
            ReorderableList list = new(group.Entries, typeof(MenuEntry), true, false, false, false)
            {
                elementHeight = EditorGUIUtility.singleLineHeight + RowSpacing * 2f
            };

            list.drawElementCallback = (rect, index, active, focused) =>
                DrawEntryRow(rect, group.Entries[index]);

            list.onReorderCallback = _ => registry.Persist();
            return list;
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(70f)))
                RefreshScan();

            if (GUILayout.Button("Apply", EditorStyles.toolbarButton, GUILayout.Width(70f)))
                MenuApplier.Apply(true);

            GUILayout.FlexibleSpace();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("Start", GUILayout.Width(34f));
            registry.StartPriority = EditorGUILayout.IntField(registry.StartPriority, EditorStyles.toolbarTextField,
                GUILayout.Width(50f));

            EditorGUILayout.LabelField("Gap", GUILayout.Width(28f));
            registry.SeparatorGap = EditorGUILayout.IntField(registry.SeparatorGap, EditorStyles.toolbarTextField,
                GUILayout.Width(50f));

            if (EditorGUI.EndChangeCheck())
                registry.Persist();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawGroup(int groupIndex)
        {
            MenuGroup group = registry.Groups[groupIndex];

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawGroupHeader(group, groupIndex);

            if (group.Entries.Count == 0)
                EditorGUILayout.LabelField("No entries. Drag entries here from other groups.", EditorStyles.miniLabel);
            else
                entryLists[groupIndex].DoLayoutList();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2f);
        }

        private void DrawGroupHeader(MenuGroup group, int groupIndex)
        {
            EditorGUILayout.BeginHorizontal();

            string newName = EditorGUILayout.DelayedTextField(group.Name, EditorStyles.boldLabel);

            if (newName != group.Name)
            {
                group.Name = newName;
                registry.Persist();
            }

            GUILayout.FlexibleSpace();

            using (new EditorGUI.DisabledScope(groupIndex == 0))
            {
                if (GUILayout.Button("Up", GUILayout.Width(40f)))
                    pendingOperation = () => MoveGroup(groupIndex, -1);
            }

            using (new EditorGUI.DisabledScope(groupIndex == registry.Groups.Count - 1))
            {
                if (GUILayout.Button("Down", GUILayout.Width(50f)))
                    pendingOperation = () => MoveGroup(groupIndex, 1);
            }

            using (new EditorGUI.DisabledScope(group.Entries.Count != 0))
            {
                if (GUILayout.Button("Delete", GUILayout.Width(60f)))
                    pendingOperation = () => registry.Groups.RemoveAt(groupIndex);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawEntryRow(Rect rect, MenuEntry entry)
        {
            float y = rect.y + RowSpacing;
            float height = EditorGUIUtility.singleLineHeight;
            float x = rect.x;

            Rect toggleRect = new(x, y, ToggleWidth, height);
            x += ToggleWidth + Pad;

            float fixedRight = KindWidth + PriorityWidth + GroupWidth + Pad * 3f;
            float pathWidth = Mathf.Max(80f, rect.xMax - x - fixedRight);
            Rect pathRect = new(x, y, pathWidth, height);
            x += pathWidth + Pad;

            Rect kindRect = new(x, y, KindWidth, height);
            x += KindWidth + Pad;

            Rect priorityRect = new(x, y, PriorityWidth, height);
            x += PriorityWidth + Pad;

            Rect groupRect = new(x, y, GroupWidth, height);

            bool enabled = EditorGUI.Toggle(toggleRect, entry.Enabled);

            if (enabled != entry.Enabled)
            {
                entry.Enabled = enabled;
                registry.Persist();
            }

            using (new EditorGUI.DisabledScope(entry.Missing))
            {
                string path = EditorGUI.DelayedTextField(pathRect, entry.Path);

                if (path != entry.Path)
                {
                    entry.Path = path;
                    registry.Persist();
                }
            }

            EditorGUI.LabelField(kindRect, KindLabel(entry), EditorStyles.miniLabel);
            EditorGUI.LabelField(priorityRect, PriorityLabel(entry), EditorStyles.miniLabel);
            DrawGroupPopup(groupRect, entry);
        }

        private void DrawGroupPopup(Rect rect, MenuEntry entry)
        {
            List<MenuGroup> groups = registry.Groups;
            int current = IndexOfOwningGroup(entry);
            string[] names = new string[groups.Count];

            for (int i = 0; i < groups.Count; i++)
                names[i] = groups[i].Name;

            int selected = EditorGUI.Popup(rect, current, names);

            if (selected == current || selected < 0)
                return;

            int from = current;
            int to = selected;
            pendingOperation = () => MoveEntry(entry, from, to);
        }

        private void DrawFooter()
        {
            EditorGUILayout.Space(4f);

            if (GUILayout.Button("Add Group"))
                pendingOperation = () => registry.Groups.Add(new MenuGroup("New Group"));
        }

        private void MoveGroup(int index, int direction)
        {
            int target = index + direction;

            if (target < 0 || target >= registry.Groups.Count)
                return;

            (registry.Groups[index], registry.Groups[target]) = (registry.Groups[target], registry.Groups[index]);
        }

        private void MoveEntry(MenuEntry entry, int fromGroup, int toGroup)
        {
            if (fromGroup < 0 || toGroup < 0 || fromGroup >= registry.Groups.Count || toGroup >= registry.Groups.Count)
                return;

            registry.Groups[fromGroup].Entries.Remove(entry);
            registry.Groups[toGroup].Entries.Add(entry);
        }

        private int IndexOfOwningGroup(MenuEntry entry)
        {
            for (int i = 0; i < registry.Groups.Count; i++)
            {
                if (registry.Groups[i].Entries.Contains(entry))
                    return i;
            }

            return 0;
        }
    }
}
#endif