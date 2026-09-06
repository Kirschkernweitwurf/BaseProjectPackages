using System;
using System.Collections.Generic;
using Base.ToolsPackage.Editor.MenuManagerModel;
using Base.ToolsPackage.Editor.Shared;
using Base.UtilityPackage.Menus;
using UnityEditor;
using UnityEngine;

namespace Base.ToolsPackage.Editor.MenuManagerWindows.CreateAssetMenuOverview
{
    /// <summary>
    /// Editor window that lists every entry under "Assets/Create", no matter whether it comes
    /// from a <see cref="CreateAssetMenuAttribute"/> or from the menu manager. Clicking a name
    /// opens the defining script, and dynamic entries link straight to the create asset manager.
    /// Only the rows currently visible in the scroll view are drawn, so the window stays
    /// responsive with a large number of results.
    /// </summary>
    internal sealed class CreateAssetMenuOverviewWindow : EditorWindow
    {
        private const string AllRootsLabel = "All";
        private const float FooterHeight = 20f;

        private static readonly string[] DefinitionLabels =
        {
            "All kinds",
            "Dynamic",
            "Static"
        };

        private static readonly GUIContent DisabledContent = new("off", "Switched off in the create asset manager");
        private static readonly GUIContent ManageContent =
            new("Manage", "Arrange this entry in the create asset manager");
        private static readonly GUIContent ManagerContent = new("Manager", "Open the create asset manager");
        private static readonly GUIContent MissingContent = new("!", "The type behind this entry no longer exists");
        private static readonly GUIContent RefreshContent = new("Refresh", "Scan the project again");

        private readonly List<CreateAssetEntry> _all = new();
        private readonly List<CreateAssetEntry> _filtered = new();

        private readonly ICreateAssetSource[] _sources =
        {
            new ReflectionCreateAssetSource(),
            new DynamicCreateAssetSource()
        };

        private string[] _roots =
        {
            AllRootsLabel
        };
        private string _root = AllRootsLabel;
        private string _search = string.Empty;
        private int _definition;
        private int _dynamicCount;
        private int _staticCount;
        private bool _includeExternal = true;
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
            MenuOverviewGui.EnsureFresh();

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
        [DynamicMenuItem("Tools/Base Packages/Code/Health/Create Asset Menu Overview")]
        private static void Open()
        {
            CreateAssetMenuOverviewWindow window = GetWindow<CreateAssetMenuOverviewWindow>("Create Assets");
            window.minSize = new Vector2(760f, 320f);
            window.Show();
        }

        private static void OpenEntry(CreateAssetEntry entry)
        {
            if (entry.Script == null)
                return;

            (int line, int column) = CreateAssetDefinitionLocator.Find(entry.Script, entry.TypeName);
            AssetDatabase.OpenAsset(entry.Script, line, column);
            EditorGUIUtility.PingObject(entry.Script);
        }

        private static GUIContent OriginBadge(EAssetOrigin origin) => origin switch
        {
            EAssetOrigin.Package => new GUIContent("pkg", "This type lives in a package"),
            EAssetOrigin.BuiltIn => new GUIContent("lib", "This type is built into Unity"),
            _ => null
        };

        private static string Tooltip(CreateAssetEntry entry) => entry.Script != null
            ? $"{MenuPath.AssetRoot}/{entry.MenuName}\n{entry.AssetPath}"
            : $"{MenuPath.AssetRoot}/{entry.MenuName}";

        private static void DrawMenuName(Rect rect, CreateAssetEntry entry)
        {
            GUIContent content = MenuOverviewGui.PathContent(entry.MenuName, Tooltip(entry));

            if (entry.Script == null)
            {
                GUI.Label(rect, content, MenuOverviewGui.PathStyle);
                return;
            }

            if (MenuOverviewGui.DrawLink(rect, content, MenuOverviewGui.PathStyle))
                OpenEntry(entry);
        }

        private static void DrawState(Rect rect, CreateAssetEntry entry)
        {
            if (entry.State == EMenuEntryState.Missing)
            {
                GUI.Label(rect, MissingContent, MenuOverviewGui.AlertStyle);
                return;
            }

            if (entry.State == EMenuEntryState.Disabled)
                GUI.Label(rect, DisabledContent, MenuOverviewGui.StateStyle);
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

            foreach (ICreateAssetSource source in _sources)
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

            foreach (CreateAssetEntry entry in _all)
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
            foreach (CreateAssetEntry entry in _all)
                distinct.Add(entry.Root);

            List<string> roots = new(distinct.Count + 1)
            {
                AllRootsLabel
            };

            roots.AddRange(distinct);
            _roots = roots.ToArray();

            if (Array.IndexOf(_roots, _root) < 0)
                _root = AllRootsLabel; // Previous focus is absent in this project; fall back.
        }

        private void RunQuery()
        {
            string root = _root == AllRootsLabel
                ? null
                : _root;

            _filtered.Clear();
            _filtered.AddRange(CreateAssetQuery.Apply(_all, _search, root, SelectedDefinition(), _includeExternal,
                _ascending));
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

                string label = _ascending
                    ? "Order \u2191"
                    : "Order \u2193";

                if (GUILayout.Button(label, EditorStyles.toolbarButton, GUILayout.Width(70f)))
                    _ascending = !_ascending;

                if (EditorGUI.EndChangeCheck())
                    RunQuery();

                GUILayout.FlexibleSpace();

                GUILayout.Label($"{_filtered.Count} of {_all.Count}", MenuOverviewGui.CountStyle,
                    GUILayout.ExpandHeight(true));

                if (GUILayout.Button(ManagerContent, EditorStyles.toolbarButton, GUILayout.Width(70f)))
                    CreateAssetMenuManagerWindow.OpenWindow();

                if (GUILayout.Button(RefreshContent, EditorStyles.toolbarButton, GUILayout.Width(60f)))
                    _needsRebuild = true;
            }
        }

        private void DrawHeader()
        {
            Rect row = GUILayoutUtility.GetRect(0f, MenuOverviewGui.RowHeight, GUILayout.ExpandWidth(true));
            MenuOverviewGui.DrawHeader(row);

            CreateAssetColumnLayout columns = new(row);
            GUIStyle style = EditorStyles.miniBoldLabel;
            GUI.Label(columns.Order, "Order", style);
            GUI.Label(columns.Kind, new GUIContent("Kind", "Dynamic entries are managed, static ones are compiled in"),
                style);

            GUI.Label(columns.MenuName, new GUIContent("Menu Name", "Path under Assets/Create"), style);
            GUI.Label(columns.Type, "Type", style);
            GUI.Label(columns.FileName, "File Name", style);
            GUI.Label(columns.State, new GUIContent("St.", "Disabled and missing markers"), style);
        }

        private void DrawList()
        {
            if (_filtered.Count == 0)
            {
                EditorGUILayout.HelpBox("No asset creation entries match the current filters.", MessageType.Info);
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
            CreateAssetEntry entry = _filtered[index];
            MenuOverviewGui.DrawRow(row, index, row.Contains(mouse), MenuOverviewGui.AccentFor(entry.Definition));

            CreateAssetColumnLayout columns = new(row);

            GUI.Label(columns.Order, entry.OrderLabel, MenuOverviewGui.NumberStyle);
            MenuOverviewGui.DrawChip(columns.Kind, MenuOverviewGui.ChipContent(entry.Definition),
                MenuOverviewGui.ChipColor(entry.Definition, entry.State));

            DrawMenuName(columns.MenuName, entry);
            GUI.Label(columns.Type, new GUIContent(entry.TypeName, entry.DeclaringType?.FullName),
                MenuOverviewGui.DetailStyle);

            GUI.Label(columns.FileName, new GUIContent(entry.FileName, entry.FileName), MenuOverviewGui.DetailStyle);
            DrawState(columns.State, entry);

            GUIContent badge = OriginBadge(entry.Origin);
            if (badge != null)
                GUI.Label(columns.Badge, badge, MenuOverviewGui.BadgeStyle);

            if (!entry.IsDynamic)
                return;

            if (GUI.Button(columns.Manage, ManageContent, EditorStyles.miniButton))
                CreateAssetMenuManagerWindow.OpenAt(entry.EntryId);
        }

        private void DrawFooter()
        {
            Rect row = GUILayoutUtility.GetRect(0f, FooterHeight, GUILayout.ExpandWidth(true));
            MenuOverviewGui.DrawFooter(row);

            Rect label = new(row.x + MenuOverviewGui.Padding, row.y, row.width - MenuOverviewGui.Padding * 2f,
                row.height);

            GUI.Label(label, "Click a name to open its script. Dynamic entries are arranged in the manager.",
                MenuOverviewGui.HintStyle);

            GUI.Label(label, $"{_dynamicCount} dynamic  |  {_staticCount} static", MenuOverviewGui.CountStyle);
        }
    }
}