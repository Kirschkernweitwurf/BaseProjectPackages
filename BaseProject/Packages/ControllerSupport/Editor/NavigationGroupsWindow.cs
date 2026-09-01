using System.Collections.Generic;
using Base.ControllerSupportPackage.Controller.Navigation;
using Base.EditorUiPackage;
using Base.UtilityPackage.Menus;
using UnityEditor;
using UnityEngine;

namespace Base.ControllerSupportPackage.Editor
{
    /// <summary>
    /// Overview of every <see cref="NavigableGroup"/> in the loaded scenes. Lists each group with menu,
    /// scene, priority and element count badges, offers per group navigation and rebuild, and hosts the
    /// scene wide and project wide rebuild actions. Rule violations, described by
    /// <see cref="NavigationGroupEntry"/>, tint the row, explain themselves in tooltips and are resolved
    /// with the per row Fix button, never silently. Each badge column uses one shared width, the widest
    /// text in that column, so rows align into clean scannable columns and nothing ever clips.
    /// </summary>
    public sealed class NavigationGroupsWindow : EditorWindow
    {
        private const string ActionsHeader = "Actions";
        private const string CancelLabel = "Cancel";
        private const string ConfirmMessage = "This opens every scene in the project, rebuilds all navigable "
            + "groups and saves the scenes. Prefabs containing groups are rebuilt and saved too.\n\nContinue?";
        private const string ConfirmTitle = "Rebuild Project Navigation";
        private const string ElementsHeader = "Elements";
        private const string EmptyElementsTooltip = "This group has no navigable elements.";
        private const string EmptyMessage = "No navigable groups in the loaded scenes.";
        private const string FixLabel = "Fix";
        private const string GoToLabel = "Go to";
        private const string GroupHeader = "Group";
        private const string MenuHeader = "Menu";
        private const string MenuPath = "Tools/Base Packages/Unity Editor/Controller Navigation Groups";
        private const string PriorityHeader = "Priority";
        private const string RebuildLabel = "Rebuild";
        private const string RebuildProjectLabel = "Rebuild Project";
        private const string RebuildSceneLabel = "Rebuild Scene";
        private const string RefreshLabel = "Refresh";
        private const string SceneHeader = "Scene";
        private const string SceneTooltip = "Scene";
        private const string WindowTitle = "Navigation Groups";

        // Reused rather than built per badge: four badges a row, on every repaint, and only the text
        // and tooltip differ between them.
        private static readonly GUIContent BadgeContent = new();

        private readonly List<NavigationGroupEntry> _entries = new();

        private float _elementsColumnWidth;
        private float _menuColumnWidth;
        private float _priorityColumnWidth;
        private float _sceneColumnWidth;
        private Vector2 _scroll;

#region Unity Callbacks
        private void OnEnable()
        {
            minSize = new Vector2(NavigationGroupsStyles.MinWindowWidth, NavigationGroupsStyles.MinWindowHeight);
            EditorApplication.hierarchyChanged += Refresh;
            Refresh();
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (_entries.Count == 0)
            {
                EditorGUILayout.HelpBox(EmptyMessage, MessageType.Info);
                return;
            }

            ComputeColumnWidths();
            DrawHeader();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].IsAlive)
                    DrawRow(i);
            }

            EditorGUILayout.EndScrollView();
        }

        private void OnDisable() => EditorApplication.hierarchyChanged -= Refresh;
#endregion

        /// <summary>Opens the window and rescans the loaded scenes.</summary>
        [DynamicMenuItem(MenuPath)]
        public static void Open()
        {
            NavigationGroupsWindow window = GetWindow<NavigationGroupsWindow>(WindowTitle);
            window.Refresh();
            window.Show();
        }

        private static bool ConfirmProjectRebuild()
            => EditorUtility.DisplayDialog(ConfirmTitle, ConfirmMessage, RebuildLabel, CancelLabel);

        // Only the cursor walk is local. The badge itself is centered and filled by EditorRows, which
        // is what every other window in the stack draws with.
        private static float DrawBadge(float right, Rect row, string text, float width, Color color, string tooltip)
        {
            Rect cell = new(right - width, row.y, width, row.height);

            BadgeContent.text = text;
            BadgeContent.tooltip = tooltip;

            EditorRows.DrawBadge(cell, BadgeContent, color, NavigationGroupsStyles.Badge);

            return cell.x - NavigationGroupsStyles.BadgeGap;
        }

        private static float DrawButton(float right, Rect row, float width, string label, bool enabled,
            out bool clicked)
        {
            float y = row.y + (row.height - NavigationGroupsStyles.ButtonHeight) * 0.5f;
            Rect rect = new(right - width, y, width, NavigationGroupsStyles.ButtonHeight);

            using (new EditorGUI.DisabledScope(!enabled))
                clicked = GUI.Button(rect, label, EditorStyles.miniButton);

            return rect.x - NavigationGroupsStyles.BadgeGap;
        }

        private static float DrawLabel(float right, Rect strip, float width, string text)
        {
            Rect rect = new(right - width, strip.y, width, strip.height);
            GUI.Label(rect, text, NavigationGroupsStyles.Header);

            return rect.x - NavigationGroupsStyles.BadgeGap;
        }

        private static void DrawSeparator(Rect strip)
        {
            Rect rect = new(strip.x, strip.yMax - NavigationGroupsStyles.SeparatorThickness, strip.width,
                NavigationGroupsStyles.SeparatorThickness);

            EditorGUI.DrawRect(rect, NavigationGroupsStyles.SeparatorColor);
        }

        private static Color ResolveMenuBadgeColor(NavigationGroupEntry entry)
        {
            if (entry.Menu == null)
                return NavigationGroupsStyles.NoMenuBadgeColor;

            return entry.HasAutoActivateConflict
                ? NavigationGroupsStyles.WarningBadgeColor
                : NavigationGroupsStyles.MenuBadgeColor;
        }

        private static Rect ResolveNameRect(Rect strip, float right) => new(strip.x + NavigationGroupsStyles.RowPadding,
            strip.y,
            right - strip.x - NavigationGroupsStyles.RowPadding * 2f, strip.height);

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button(RefreshLabel, EditorStyles.toolbarButton,
                    GUILayout.Width(NavigationGroupsStyles.ToolbarButtonWidth)))
                Refresh();

            GUILayout.FlexibleSpace();
            GUILayout.Label($"{_entries.Count} group(s)", EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(RebuildSceneLabel, EditorStyles.toolbarButton))
            {
                NavigationRebuildService.RebuildLoadedScenes();
                Refresh();
            }

            if (GUILayout.Button(RebuildProjectLabel, EditorStyles.toolbarButton) && ConfirmProjectRebuild())
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
            Rect header = EditorGUILayout.GetControlRect(false, NavigationGroupsStyles.HeaderHeight,
                GUILayout.ExpandWidth(true));

            header.x = 0f;
            header.width = position.width;

            EditorGUI.DrawRect(header, NavigationGroupsStyles.HeaderColor);
            DrawSeparator(header);

            float actionsWidth = NavigationGroupsStyles.ButtonWidth * 2f
                + NavigationGroupsStyles.FixButtonWidth
                + NavigationGroupsStyles.BadgeGap * 2f;

            float x = header.xMax - NavigationGroupsStyles.RowPadding;
            x = DrawLabel(x, header, actionsWidth, ActionsHeader);
            x -= NavigationGroupsStyles.BadgeGap;
            x = DrawLabel(x, header, _elementsColumnWidth, ElementsHeader);
            x = DrawLabel(x, header, _priorityColumnWidth, PriorityHeader);
            x = DrawLabel(x, header, _sceneColumnWidth, SceneHeader);
            x = DrawLabel(x, header, _menuColumnWidth, MenuHeader);

            EditorGUI.LabelField(ResolveNameRect(header, x), GroupHeader, EditorStyles.miniBoldLabel);
        }

        private void DrawRow(int index)
        {
            NavigationGroupEntry entry = _entries[index];

            Rect row = EditorGUILayout.GetControlRect(false, NavigationGroupsStyles.RowHeight,
                GUILayout.ExpandWidth(true));

            row.x = 0f;
            row.width = position.width;

            DrawRowBackground(row, index, entry.HasIssues);

            // Fixed action buttons on the right, then column aligned badges right to left, name gets
            // whatever remains. Column widths come from the widest text, so nothing ever clips.
            float x = row.xMax - NavigationGroupsStyles.RowPadding;
            x = DrawButton(x, row, NavigationGroupsStyles.ButtonWidth, RebuildLabel, true, out bool rebuildClicked);
            x = DrawButton(x, row, NavigationGroupsStyles.ButtonWidth, GoToLabel, true, out bool goToClicked);
            x = DrawButton(x, row, NavigationGroupsStyles.FixButtonWidth, FixLabel, entry.HasIssues,
                out bool fixClicked);

            x -= NavigationGroupsStyles.BadgeGap;
            x = DrawBadges(x, row, entry);

            EditorGUI.LabelField(ResolveNameRect(row, x), new GUIContent(entry.Group.name, entry.Group.name),
                NavigationGroupsStyles.Name);

            if (goToClicked)
                entry.GoTo();

            if (fixClicked)
                entry.Fix();

            if (!rebuildClicked)
                return;

            NavigationRebuildService.RebuildGroup(entry.Group);
            Refresh();
        }

        private float DrawBadges(float right, Rect row, NavigationGroupEntry entry)
        {
            float x = DrawBadge(right, row, entry.ElementsText, _elementsColumnWidth, entry.IsEmpty
                ? NavigationGroupsStyles.EmptyBadgeColor
                : NavigationGroupsStyles.ElementsBadgeColor, entry.IsEmpty
                ? EmptyElementsTooltip
                : string.Empty);

            x = DrawBadge(x, row, entry.PriorityText, _priorityColumnWidth, entry.HasPriorityMismatch
                ? NavigationGroupsStyles.WarningBadgeColor
                : NavigationGroupsStyles.PriorityBadgeColor, entry.PriorityTooltip);

            x = DrawBadge(x, row, entry.SceneText, _sceneColumnWidth, NavigationGroupsStyles.SceneBadgeColor,
                SceneTooltip);

            return DrawBadge(x, row, entry.MenuText, _menuColumnWidth, ResolveMenuBadgeColor(entry),
                entry.BuildMenuTooltip());
        }

        private void DrawRowBackground(Rect row, int index, bool hasIssues)
        {
            if (index % 2 == 1)
                EditorGUI.DrawRect(row, NavigationGroupsStyles.StripeColor);

            if (hasIssues)
                EditorGUI.DrawRect(row, NavigationGroupsStyles.IssueRowColor);

            if (row.Contains(Event.current.mousePosition))
            {
                EditorGUI.DrawRect(row, NavigationGroupsStyles.HoverColor);
                Repaint();
            }

            DrawSeparator(row);
        }

        // One shared width per column, taken from its widest text, keeps the badge edges aligned
        // across rows so the list reads like a table instead of jagged per row sizing.
        private void ComputeColumnWidths()
        {
            _menuColumnWidth = NavigationGroupsStyles.MeasureBadge(MenuHeader);
            _sceneColumnWidth = NavigationGroupsStyles.MeasureBadge(SceneHeader);
            _priorityColumnWidth = NavigationGroupsStyles.MeasureBadge(PriorityHeader);
            _elementsColumnWidth = NavigationGroupsStyles.MeasureBadge(ElementsHeader);

            foreach (NavigationGroupEntry entry in _entries)
            {
                if (!entry.IsAlive)
                    continue;

                _menuColumnWidth = Mathf.Max(_menuColumnWidth, NavigationGroupsStyles.MeasureBadge(entry.MenuText));
                _sceneColumnWidth = Mathf.Max(_sceneColumnWidth, NavigationGroupsStyles.MeasureBadge(entry.SceneText));

                _priorityColumnWidth = Mathf.Max(_priorityColumnWidth,
                    NavigationGroupsStyles.MeasureBadge(entry.PriorityText));

                _elementsColumnWidth = Mathf.Max(_elementsColumnWidth,
                    NavigationGroupsStyles.MeasureBadge(entry.ElementsText));
            }
        }

        private void Refresh()
        {
            _entries.Clear();

            NavigableGroup[] found = FindObjectsByType<NavigableGroup>(FindObjectsInactive.Include,
                FindObjectsSortMode.InstanceID);

            foreach (NavigableGroup group in found)
                _entries.Add(new NavigationGroupEntry(group));

            Repaint();
        }
    }
}