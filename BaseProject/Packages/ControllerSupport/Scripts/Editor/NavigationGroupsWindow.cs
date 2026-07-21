#if UNITY_EDITOR
using System.Collections.Generic;
using Base.ControllerSupport.Controller.Navigation;
using Base.ToolPackage.MenuManagerWindow;
using UnityEditor;
using UnityEngine;
using Menu = Base.CorePackage.MenuManaging.Menu;

namespace Base.ControllerSupport.Editor
{
    /// <summary>
    /// Overview of every <see cref="NavigableGroup"/> in the loaded scenes. Lists each group with
    /// menu, scene, priority and element count badges, offers per group navigation and rebuild, and
    /// hosts the scene wide and project wide rebuild actions. Groups sitting on a <see cref="Menu"/>
    /// are checked against the menu rules: Auto Activate must be off, since the menu is the one
    /// activating, and the priority should match the menu's. Violations tint the row, explain
    /// themselves in tooltips and are resolved with the per row Fix button, never silently. Each badge
    /// column uses one shared width, the widest text in that column, so rows align into clean
    /// scannable columns and nothing ever clips.
    /// </summary>
    public sealed class NavigationGroupsWindow : EditorWindow
    {
        private const float BadgeGap = 4f;
        private const float BadgeHeight = 16f;
        private const float BadgePadding = 14f;
        private const float ButtonWidth = 56f;
        private const float FixButtonWidth = 40f;
        private const float HeaderHeight = 20f;
        private const string MenuPath = "Tools/Base Packages/Unity Editor/Controller Navigation Groups";
        private const float MinBadgeWidth = 64f;
        private const string NoMenuText = "None";
        private const float RowHeight = 26f;
        private const float RowPadding = 6f;
        private const string WindowTitle = "Navigation Groups";
        private static readonly Color ElementsBadgeColor = new(0.7f, 0.45f, 0.95f, 0.32f);

        private static readonly Color EmptyBadgeColor = new(0.95f, 0.55f, 0.2f, 0.4f);
        private static readonly Color HeaderColor = new(0f, 0f, 0f, 0.18f);
        private static readonly Color HoverColor = new(1f, 1f, 1f, 0.05f);
        private static readonly Color IssueRowColor = new(0.95f, 0.45f, 0.2f, 0.06f);
        private static readonly Color MenuBadgeColor = new(0.3f, 0.7f, 0.4f, 0.32f);
        private static readonly Color NoMenuBadgeColor = new(0.5f, 0.5f, 0.5f, 0.12f);
        private static readonly Color PriorityBadgeColor = new(0.35f, 0.55f, 0.95f, 0.32f);
        private static readonly Color SceneBadgeColor = new(0.5f, 0.5f, 0.5f, 0.28f);
        private static readonly Color SeparatorColor = new(0f, 0f, 0f, 0.25f);
        private static readonly Color StripeColor = new(1f, 1f, 1f, 0.02f);
        private static readonly Color WarningBadgeColor = new(0.95f, 0.55f, 0.2f, 0.45f);

        private GUIStyle BadgeStyle => _badgeStyle ??= new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 10
        };

        private GUIStyle HeaderStyle => _headerStyle ??= new GUIStyle(EditorStyles.miniBoldLabel)
        {
            alignment = TextAnchor.MiddleCenter
        };

        private GUIStyle NameStyle => _nameStyle ??= new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fontStyle = FontStyle.Bold
        };

        private readonly List<int> _elementCounts = new();
        private readonly List<NavigableGroup> _groups = new();
        private readonly List<Menu> _menus = new();

        private GUIStyle _badgeStyle;
        private float _elementsColumnWidth;
        private GUIStyle _headerStyle;
        private float _menuColumnWidth;
        private GUIStyle _nameStyle;
        private float _priorityColumnWidth;
        private float _sceneColumnWidth;
        private Vector2 _scroll;

#region Unity Callbacks
        private void OnEnable()
        {
            minSize = new Vector2(680f, 200f);
            EditorApplication.hierarchyChanged += Refresh;
            Refresh();
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (_groups.Count == 0)
            {
                EditorGUILayout.HelpBox("No navigable groups in the loaded scenes.", MessageType.Info);
                return;
            }

            ComputeColumnWidths();
            DrawHeader();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            for (int i = 0; i < _groups.Count; i++)
            {
                if (_groups[i] != null)
                    DrawRow(i);
            }

            EditorGUILayout.EndScrollView();
        }

        private void OnDisable() => EditorApplication.hierarchyChanged -= Refresh;
#endregion

        [DynamicMenuItem(MenuPath)]
        public static void Open()
        {
            NavigationGroupsWindow window = GetWindow<NavigationGroupsWindow>(WindowTitle);
            window.Refresh();
            window.Show();
        }

        private static void GoTo(NavigableGroup group)
        {
            Selection.activeGameObject = group.gameObject;
            EditorGUIUtility.PingObject(group.gameObject);
        }

        private static bool HasAutoActivateConflict(NavigableGroup group, Menu menu)
            => menu != null && group.AutoActivate;

        private static bool HasPriorityMismatch(NavigableGroup group, Menu menu)
            => menu != null && group.Priority != menu.Priority;

        private static string ElementsText(int elementCount) => elementCount == 1
            ? "1 Element"
            : $"{elementCount} Elements";

        private static string BuildIssueTooltip(bool autoConflict, bool priorityMismatch, Menu menu)
        {
            string tooltip = string.Empty;

            if (autoConflict)
                tooltip += "Auto Activate is enabled, but the menu is the one activating this group.";

            if (!priorityMismatch)
                return tooltip;

            if (autoConflict)
                tooltip += "\n";

            tooltip += $"Priority differs from the menu ({menu.Priority}).";
            return tooltip;
        }

        // Fixes are only ever applied through this explicit click, with undo support, so the window
        // reports problems instead of rewriting components silently.
        private static void FixIssues(NavigableGroup group, Menu menu)
        {
            SerializedObject serializedGroup = new(group);

            if (HasAutoActivateConflict(group, menu))
                serializedGroup.FindProperty(NavigableGroup.AutoActivateFieldName).boolValue = false;

            if (HasPriorityMismatch(group, menu))
                serializedGroup.FindProperty(NavigableGroup.PriorityFieldName).intValue = (int)menu.Priority;

            serializedGroup.ApplyModifiedProperties();
        }

        private static bool ConfirmProjectRebuild() => EditorUtility.DisplayDialog("Rebuild Project Navigation",
            "This opens every scene in the project, rebuilds all navigable groups and saves the "
            + "scenes. Prefabs containing groups are rebuilt and saved too.\n\nContinue?",
            "Rebuild", "Cancel");

        private static string MenuText(Menu menu) => menu != null
            ? menu.GetType().Name
            : NoMenuText;

        private static Color MenuBadgeColorFor(Menu menu, bool autoConflict)
        {
            if (menu == null)
                return NoMenuBadgeColor;

            return autoConflict
                ? WarningBadgeColor
                : MenuBadgeColor;
        }

        private static string MenuTooltip(Menu menu, bool autoConflict, bool priorityMismatch)
        {
            if (menu == null)
                return "This group sits on no menu and manages its own activation.";

            return autoConflict || priorityMismatch
                ? BuildIssueTooltip(autoConflict, priorityMismatch, menu)
                : "This group sits on a menu, which drives its activation.";
        }

        private static float DrawButton(float right, Rect row, float width, string label, bool enabled,
            out bool clicked)
        {
            Rect rect = new(right - width, row.y + (row.height - BadgeHeight - 2f) * 0.5f, width, BadgeHeight + 2f);

            using (new EditorGUI.DisabledScope(!enabled))
                clicked = GUI.Button(rect, label, EditorStyles.miniButton);

            return rect.x - BadgeGap;
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60f)))
                Refresh();

            GUILayout.FlexibleSpace();
            GUILayout.Label($"{_groups.Count} group(s)", EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Rebuild Scene", EditorStyles.toolbarButton))
            {
                NavigationRebuildService.RebuildLoadedScenes();
                Refresh();
            }

            if (GUILayout.Button("Rebuild Project", EditorStyles.toolbarButton) && ConfirmProjectRebuild())
            {
                NavigationRebuildService.RebuildProject();
                Refresh();
            }

            EditorGUILayout.EndHorizontal();
        }

        // The header mirrors the row layout exactly, same widths and gaps, so every label sits
        // precisely above its column. It lives outside the scroll view and stays visible.
        private void DrawHeader()
        {
            Rect header = EditorGUILayout.GetControlRect(false, HeaderHeight, GUILayout.ExpandWidth(true));
            header.x = 0f;
            header.width = position.width;

            EditorGUI.DrawRect(header, HeaderColor);
            EditorGUI.DrawRect(new Rect(header.x, header.yMax - 1f, header.width, 1f), SeparatorColor);

            float x = header.xMax - RowPadding;
            x = DrawHeaderLabel(x, header, ButtonWidth + BadgeGap + ButtonWidth + BadgeGap + FixButtonWidth,
                "Actions");

            x -= BadgeGap;
            x = DrawHeaderLabel(x, header, _elementsColumnWidth, "Elements");
            x = DrawHeaderLabel(x, header, _priorityColumnWidth, "Priority");
            x = DrawHeaderLabel(x, header, _sceneColumnWidth, "Scene");
            x = DrawHeaderLabel(x, header, _menuColumnWidth, "Menu");

            Rect nameRect = new(header.x + RowPadding, header.y, x - header.x - RowPadding * 2f, header.height);
            EditorGUI.LabelField(nameRect, "Group", EditorStyles.miniBoldLabel);
        }

        private float DrawHeaderLabel(float right, Rect header, float width, string text)
        {
            Rect rect = new(right - width, header.y, width, header.height);
            GUI.Label(rect, text, HeaderStyle);
            return rect.x - BadgeGap;
        }

        private void DrawRow(int index)
        {
            NavigableGroup group = _groups[index];
            Menu menu = _menus[index];
            int elementCount = _elementCounts[index];

            bool autoConflict = HasAutoActivateConflict(group, menu);
            bool priorityMismatch = HasPriorityMismatch(group, menu);
            bool hasIssues = autoConflict || priorityMismatch;

            Rect row = EditorGUILayout.GetControlRect(false, RowHeight, GUILayout.ExpandWidth(true));
            row.x = 0f;
            row.width = position.width;

            if (index % 2 == 1)
                EditorGUI.DrawRect(row, StripeColor);

            if (hasIssues)
                EditorGUI.DrawRect(row, IssueRowColor);

            if (row.Contains(Event.current.mousePosition))
            {
                EditorGUI.DrawRect(row, HoverColor);
                Repaint();
            }

            EditorGUI.DrawRect(new Rect(row.x, row.yMax - 1f, row.width, 1f), SeparatorColor);

            // Fixed action buttons on the right, then column aligned badges right to left, name gets
            // whatever remains. Column widths come from the widest text, so nothing ever clips.
            float x = row.xMax - RowPadding;
            x = DrawButton(x, row, ButtonWidth, "Rebuild", true, out bool rebuildClicked);
            x = DrawButton(x, row, ButtonWidth, "Go to", true, out bool goToClicked);
            x = DrawButton(x, row, FixButtonWidth, "Fix", hasIssues, out bool fixClicked);
            x -= BadgeGap;

            bool isEmpty = elementCount == 0;

            x = DrawBadge(x, row, ElementsText(elementCount), _elementsColumnWidth, isEmpty
                ? EmptyBadgeColor
                : ElementsBadgeColor, isEmpty
                ? "This group has no navigable elements."
                : string.Empty);

            x = DrawBadge(x, row, group.Priority.ToString(), _priorityColumnWidth, priorityMismatch
                ? WarningBadgeColor
                : PriorityBadgeColor, priorityMismatch
                ? $"Priority differs from the menu ({menu.Priority})."
                : "Focus priority");

            x = DrawBadge(x, row, group.gameObject.scene.name, _sceneColumnWidth, SceneBadgeColor, "Scene");

            x = DrawBadge(x, row, MenuText(menu), _menuColumnWidth, MenuBadgeColorFor(menu, autoConflict),
                MenuTooltip(menu, autoConflict, priorityMismatch));

            Rect nameRect = new(row.x + RowPadding, row.y, x - row.x - RowPadding * 2f, row.height);
            EditorGUI.LabelField(nameRect, new GUIContent(group.name, group.name), NameStyle);

            if (goToClicked)
                GoTo(group);

            if (fixClicked)
                FixIssues(group, menu);

            if (!rebuildClicked)
                return;

            group.Rebuild();
            Refresh();
        }

        private float DrawBadge(float right, Rect row, string text, float width, Color color, string tooltip)
        {
            Rect rect = new(right - width, row.y + (row.height - BadgeHeight) * 0.5f, width, BadgeHeight);

            EditorGUI.DrawRect(rect, color);
            GUI.Label(rect, new GUIContent(text, tooltip), BadgeStyle);

            return rect.x - BadgeGap;
        }

        private float MeasureBadge(string text)
            => Mathf.Max(MinBadgeWidth, BadgeStyle.CalcSize(new GUIContent(text)).x + BadgePadding);

        // One shared width per column, taken from its widest text, keeps the badge edges aligned
        // across rows so the list reads like a table instead of jagged per row sizing.
        private void ComputeColumnWidths()
        {
            _menuColumnWidth = MeasureBadge("Menu");
            _sceneColumnWidth = MeasureBadge("Scene");
            _priorityColumnWidth = MeasureBadge("Priority");
            _elementsColumnWidth = MeasureBadge("Elements");

            for (int i = 0; i < _groups.Count; i++)
            {
                NavigableGroup group = _groups[i];
                if (group == null)
                    continue;

                _menuColumnWidth = Mathf.Max(_menuColumnWidth, MeasureBadge(MenuText(_menus[i])));
                _sceneColumnWidth = Mathf.Max(_sceneColumnWidth, MeasureBadge(group.gameObject.scene.name));
                _priorityColumnWidth = Mathf.Max(_priorityColumnWidth, MeasureBadge(group.Priority.ToString()));
                _elementsColumnWidth = Mathf.Max(_elementsColumnWidth, MeasureBadge(ElementsText(_elementCounts[i])));
            }
        }

        private void Refresh()
        {
            _groups.Clear();
            _menus.Clear();
            _elementCounts.Clear();

            NavigableGroup[] found = FindObjectsByType<NavigableGroup>(FindObjectsInactive.Include,
                FindObjectsSortMode.InstanceID);

            foreach (NavigableGroup group in found)
            {
                _groups.Add(group);
                _menus.Add(group.GetComponent<Menu>());
                _elementCounts.Add(group.GetComponentsInChildren<NavigableElement>(true).Length);
            }

            Repaint();
        }
    }
}
#endif