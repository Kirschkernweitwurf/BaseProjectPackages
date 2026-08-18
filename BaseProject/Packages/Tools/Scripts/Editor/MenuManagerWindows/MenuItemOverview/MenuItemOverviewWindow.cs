using System;
using System.Collections.Generic;
using Base.ToolPackage.Editor.Shared;
using Base.UtilityPackage.Menus;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.MenuManagerWindows.MenuItemOverview
{
    /// <summary>
    /// Editor window that lists every menu item known to the editor, no matter whether it comes
    /// from a <see cref="MenuItem"/> attribute or from the menu manager. Clicking a path opens
    /// the defining script, and dynamic entries link straight to the menu item manager. Only the
    /// rows currently visible in the scroll view are drawn, so the window stays responsive with
    /// a large number of results.
    /// </summary>
    public sealed class MenuItemOverviewWindow : EditorWindow
    {
        private const string AllRootsLabel = "All";
        private const string DefaultRoot = "Tools";
        private const float FooterHeight = 20f;

        private static readonly string[] DefinitionLabels =
        {
            "All kinds",
            "Dynamic",
            "Static"
        };

        private static readonly GUIContent DisabledContent = new("off", "Switched off in the menu item manager");
        private static readonly GUIContent ManageContent = new("Manage", "Arrange this entry in the menu item manager");
        private static readonly GUIContent ManagerContent = new("Manager", "Open the menu item manager");
        private static readonly GUIContent MissingContent = new("!", "The code behind this entry no longer exists");
        private static readonly GUIContent RefreshContent = new("Refresh", "Scan the project again");
        private static readonly GUIContent ValidationContent = new("v", "Validation function");

        private readonly List<MenuItemEntry> _all = new();
        private readonly List<MenuItemEntry> _filtered = new();

        private readonly IMenuItemSource[] _sources =
        {
            new ReflectionMenuItemSource(),
            new DynamicMenuItemSource()
        };

        private string[] _roots =
        {
            AllRootsLabel
        };
        private string _root = DefaultRoot;
        private string _search = string.Empty;
        private int _definition;
        private int _dynamicCount;
        private int _staticCount;
        private bool _includeExternal = true;
        private bool _hideValidation;
        private bool _ascending = true;
        private bool _needsRebuild = true;
        private Vector2 _scroll;

#region Unity Callbacks
        private void OnEnable()
        {
            wantsMouseMove = true;
            _needsRebuild = true;
        }

        private void OnGUI()
        {
            if (_needsRebuild)
                Rebuild();

            if (Event.current.type == EventType.MouseMove)
                Repaint();

            DrawToolbar();
            DrawHeader();
            DrawList();
            DrawFooter();
        }
#endregion

        /// <summary>Opens or focuses the window from the Tools menu.</summary>
        [DynamicMenuItem("Tools/Base Packages/Code/Health/Menu Item Overview")]
        private static void Open()
        {
            MenuItemOverviewWindow window = GetWindow<MenuItemOverviewWindow>("Menu Items");
            window.minSize = new Vector2(760f, 320f);
            window.Show();
        }

        private static void OpenEntry(MenuItemEntry entry)
        {
            if (entry.Script == null)
                return;

            (int line, int column) = MenuItemDefinitionLocator.Find(entry.Script, entry.MenuPath, entry.MethodName);
            AssetDatabase.OpenAsset(entry.Script, line, column);
            EditorGUIUtility.PingObject(entry.Script);
        }

        private static GUIContent OriginBadge(EAssetOrigin origin) => origin switch
        {
            EAssetOrigin.Package => new GUIContent("pkg", "This item lives in a package"),
            EAssetOrigin.BuiltIn => new GUIContent("lib", "This item is built into Unity"),
            _ => null
        };

        private static string Tooltip(MenuItemEntry entry) => entry.Script != null
            ? $"{entry.MenuPath}\n{entry.AssetPath}"
            : entry.MenuPath;

        private static void DrawPath(Rect rect, MenuItemEntry entry)
        {
            GUIContent content = MenuOverviewGui.PathContent(entry.MenuPath, Tooltip(entry));

            if (entry.Script == null)
            {
                GUI.Label(rect, content, MenuOverviewGui.PathStyle);
                return;
            }

            if (MenuOverviewGui.DrawLink(rect, content, MenuOverviewGui.PathStyle))
                OpenEntry(entry);
        }

        private static void DrawState(Rect rect, MenuItemEntry entry)
        {
            if (entry.State == EMenuEntryState.Missing)
            {
                GUI.Label(rect, MissingContent, MenuOverviewGui.AlertStyle);
                return;
            }

            if (entry.State == EMenuEntryState.Disabled)
            {
                GUI.Label(rect, DisabledContent, MenuOverviewGui.StateStyle);
                return;
            }

            if (entry.IsValidation)
                GUI.Label(rect, ValidationContent, MenuOverviewGui.StateStyle);
        }

        private EMenuDefinition? SelectedDefinition() => _definition switch
        {
            1 => EMenuDefinition.Dynamic,
            2 => EMenuDefinition.Static,
            _ => null
        };

        private void Rebuild()
        {
            _all.Clear();

            foreach (IMenuItemSource source in _sources)
                _all.AddRange(source.Collect());

            CountDefinitions();
            BuildRoots();
            RunQuery();
            _needsRebuild = false;
        }

        private void CountDefinitions()
        {
            _dynamicCount = 0;
            _staticCount = 0;

            foreach (MenuItemEntry entry in _all)
            {
                if (entry.IsDynamic)
                    _dynamicCount++;
                else
                    _staticCount++;
            }
        }

        private void BuildRoots()
        {
            SortedSet<string> distinct = new(StringComparer.Ordinal);
            foreach (MenuItemEntry entry in _all)
                distinct.Add(entry.Root);

            List<string> roots = new(distinct.Count + 1)
            {
                AllRootsLabel
            };

            roots.AddRange(distinct);
            _roots = roots.ToArray();

            if (Array.IndexOf(_roots, _root) < 0)
                _root = AllRootsLabel; // Default focus is absent in this project; fall back.
        }

        private void RunQuery()
        {
            string root = _root == AllRootsLabel
                ? null
                : _root;

            _filtered.Clear();
            _filtered.AddRange(MenuItemQuery.Apply(_all, _search, root, SelectedDefinition(), _includeExternal,
                _hideValidation, _ascending));
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUI.BeginChangeCheck();

                _search = GUILayout.TextField(_search, EditorStyles.toolbarSearchField, GUILayout.MinWidth(140f));

                int current = Mathf.Max(0, Array.IndexOf(_roots, _root));
                int selected = EditorGUILayout.Popup(current, _roots, EditorStyles.toolbarPopup, GUILayout.Width(130f));
                _root = _roots[selected];

                _definition = EditorGUILayout.Popup(_definition, DefinitionLabels, EditorStyles.toolbarPopup,
                    GUILayout.Width(90f));

                _includeExternal = GUILayout.Toggle(_includeExternal, "External", EditorStyles.toolbarButton,
                    GUILayout.Width(64f));

                _hideValidation = GUILayout.Toggle(_hideValidation, "Hide validation", EditorStyles.toolbarButton,
                    GUILayout.Width(96f));

                string label = _ascending
                    ? "Priority \u2191"
                    : "Priority \u2193";

                if (GUILayout.Button(label, EditorStyles.toolbarButton, GUILayout.Width(78f)))
                    _ascending = !_ascending;

                if (EditorGUI.EndChangeCheck())
                    RunQuery();

                GUILayout.FlexibleSpace();

                GUILayout.Label($"{_filtered.Count} of {_all.Count}", MenuOverviewGui.CountStyle,
                    GUILayout.ExpandHeight(true));

                if (GUILayout.Button(ManagerContent, EditorStyles.toolbarButton, GUILayout.Width(70f)))
                    MenuItemManagerWindow.OpenWindow();

                if (GUILayout.Button(RefreshContent, EditorStyles.toolbarButton, GUILayout.Width(60f)))
                    _needsRebuild = true;
            }
        }

        private void DrawHeader()
        {
            Rect row = GUILayoutUtility.GetRect(0f, MenuOverviewGui.RowHeight, GUILayout.ExpandWidth(true));
            MenuOverviewGui.DrawHeader(row);

            MenuItemColumnLayout columns = new(row);
            GUIStyle style = EditorStyles.miniBoldLabel;
            GUI.Label(columns.Priority, "Priority", style);
            GUI.Label(columns.Kind, new GUIContent("Kind", "Dynamic entries are managed, static ones are compiled in"),
                style);

            GUI.Label(columns.Path, "Menu Path", style);
            GUI.Label(columns.Member, "Member", style);
            GUI.Label(columns.State, new GUIContent("St.", "Validation, disabled and missing markers"), style);
        }

        private void DrawList()
        {
            if (_filtered.Count == 0)
            {
                EditorGUILayout.HelpBox("No menu items match the current filters.", MessageType.Info);
                GUILayout.FlexibleSpace();
                return;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            float totalHeight = _filtered.Count * MenuOverviewGui.RowHeight;
            Rect content = GUILayoutUtility.GetRect(0f, totalHeight, GUILayout.ExpandWidth(true));

            int firstVisible = Mathf.Max(0, Mathf.FloorToInt(_scroll.y / MenuOverviewGui.RowHeight) - 1);
            int visibleCount = Mathf.CeilToInt(position.height / MenuOverviewGui.RowHeight) + 2;
            int lastVisible = Mathf.Min(_filtered.Count, firstVisible + visibleCount);
            Vector2 mouse = Event.current.mousePosition;

            for (int i = firstVisible; i < lastVisible; i++)
            {
                Rect row = new(content.x, content.y + i * MenuOverviewGui.RowHeight, content.width,
                    MenuOverviewGui.RowHeight);

                DrawRow(row, i, mouse);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawRow(Rect row, int index, Vector2 mouse)
        {
            MenuItemEntry entry = _filtered[index];
            MenuOverviewGui.DrawRow(row, index, row.Contains(mouse), MenuOverviewGui.AccentFor(entry.Definition));

            MenuItemColumnLayout columns = new(row);

            GUI.Label(columns.Priority, entry.PriorityLabel, MenuOverviewGui.NumberStyle);
            MenuOverviewGui.DrawChip(columns.Kind, MenuOverviewGui.ChipContent(entry.Definition),
                MenuOverviewGui.ChipColor(entry.Definition, entry.State));

            DrawPath(columns.Path, entry);
            GUI.Label(columns.Member, new GUIContent(entry.Member, entry.DeclaringType?.FullName),
                MenuOverviewGui.DetailStyle);

            DrawState(columns.State, entry);

            GUIContent badge = OriginBadge(entry.Origin);
            if (badge != null)
                GUI.Label(columns.Badge, badge, MenuOverviewGui.BadgeStyle);

            if (!entry.IsDynamic)
                return;

            if (GUI.Button(columns.Manage, ManageContent, EditorStyles.miniButton))
                MenuItemManagerWindow.OpenAt(entry.EntryId);
        }

        private void DrawFooter()
        {
            Rect row = GUILayoutUtility.GetRect(0f, FooterHeight, GUILayout.ExpandWidth(true));
            MenuOverviewGui.DrawFooter(row);

            Rect label = new(row.x + MenuOverviewGui.Padding, row.y, row.width - MenuOverviewGui.Padding * 2f,
                row.height);

            GUI.Label(label, "Click a path to open its script. Dynamic entries are arranged in the manager.",
                MenuOverviewGui.HintStyle);

            GUI.Label(label, $"{_dynamicCount} dynamic  |  {_staticCount} static", MenuOverviewGui.CountStyle);
        }
    }
}