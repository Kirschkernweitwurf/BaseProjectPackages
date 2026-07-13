#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Base.UtilityPackage.Generated;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.MenuItemOverview
{
    /// <summary>
    /// Editor window that lists every <see cref="MenuItem"/> in the project, its packages
    /// and Unity itself, sorted by menu priority. Clicking a project or package item opens
    /// its script at the priority argument. Only the rows currently visible in the scroll
    /// view are drawn, so the window stays responsive with a large number of results.
    /// </summary>
    public sealed class MenuItemOverviewWindow : EditorWindow
    {
        private const string AllRootsLabel = "All";
        private const string DefaultRoot = "Tools";
        private const float RowHeight = 22f;

        private readonly List<MenuItemEntry> _all = new();
        private readonly List<MenuItemEntry> _filtered = new();

        private IMenuItemSource _source;
        private GUIStyle _priorityStyle;
        private GUIStyle _countStyle;
        private GUIStyle _badgeStyle;
        private GUIStyle _validationStyle;
        private string[] _roots =
        {
            AllRootsLabel
        };
        private string _root = DefaultRoot;
        private string _search = string.Empty;
        private bool _includeExternal = true;
        private bool _hideValidation;
        private bool _ascending = true;
        private bool _needsRebuild = true;
        private Vector2 _scroll;

#region Unity Callbacks
        private void OnEnable()
        {
            _source = new ReflectionMenuItemSource();
            _needsRebuild = true;
        }

        private void OnGUI()
        {
            EnsureStyles();

            if (_needsRebuild)
                Rebuild();

            DrawToolbar();
            DrawHeader();
            DrawList();
        }
#endregion

        /// <summary>Opens or focuses the window from the Tools menu.</summary>
        [MenuItem("Tools/Base Packages/Code Health/Menu Item Overview", priority = MenuOrders.Code)]
        private static void Open()
        {
            MenuItemOverviewWindow window = GetWindow<MenuItemOverviewWindow>("Menu Items");
            window.minSize = new Vector2(640f, 320f);
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

        private static GUIContent OriginBadge(EMenuItemOrigin origin) => origin switch
        {
            EMenuItemOrigin.Package => new GUIContent("pkg", "This item lives in a package"),
            EMenuItemOrigin.BuiltIn => new GUIContent("lib", "This item is built into Unity"),
            _ => null
        };

        private static void DrawPath(Rect rect, MenuItemEntry entry)
        {
            if (entry.Script == null)
            {
                GUI.Label(rect, new GUIContent(entry.MenuPath, entry.MenuPath), EditorStyles.label);
                return;
            }

            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
            if (GUI.Button(rect, new GUIContent(entry.MenuPath, entry.AssetPath), EditorStyles.label))
                OpenEntry(entry);
        }

        private void EnsureStyles()
        {
            if (_priorityStyle != null)
                return;

            _priorityStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleRight
            };

            _countStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleRight,
                fixedHeight = 0f
            };

            _badgeStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleRight
            };

            _validationStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter
            };
        }

        private void Rebuild()
        {
            _all.Clear();
            _all.AddRange(_source.Collect());
            BuildRoots();
            RunQuery();
            _needsRebuild = false;
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
            _filtered.AddRange(MenuItemQuery.Apply(_all, _search, root, _includeExternal, _hideValidation, _ascending));
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUI.BeginChangeCheck();

                _search = GUILayout.TextField(_search, EditorStyles.toolbarSearchField, GUILayout.MinWidth(160f));

                int current = Mathf.Max(0, Array.IndexOf(_roots, _root));
                int selected = EditorGUILayout.Popup(current, _roots, EditorStyles.toolbarPopup, GUILayout.Width(140f));
                _root = _roots[selected];

                _includeExternal = GUILayout.Toggle(_includeExternal, "Include external", EditorStyles.toolbarButton);
                _hideValidation = GUILayout.Toggle(_hideValidation, "Hide validation", EditorStyles.toolbarButton);

                string label = _ascending
                    ? "Priority \u2191"
                    : "Priority \u2193";

                if (GUILayout.Button(label, EditorStyles.toolbarButton, GUILayout.Width(82f)))
                    _ascending = !_ascending;

                if (EditorGUI.EndChangeCheck())
                    RunQuery();

                GUILayout.FlexibleSpace();
                GUILayout.Label($"{_filtered.Count} items", _countStyle, GUILayout.ExpandHeight(true));

                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(64f)))
                    _needsRebuild = true;
            }
        }

        private void DrawHeader()
        {
            Rect row = GUILayoutUtility.GetRect(0f, RowHeight, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(row, new Color(0f, 0f, 0f, 0.12f));

            MenuItemColumnLayout columns = new(row);
            GUIStyle style = EditorStyles.miniBoldLabel;
            GUI.Label(columns.Priority, "Priority", style);
            GUI.Label(columns.Path, "Menu Path", style);
            GUI.Label(columns.Member, "Member", style);
            GUI.Label(columns.Validation, new GUIContent("Validation", "Validation function"), style);
        }

        private void DrawList()
        {
            if (_filtered.Count == 0)
            {
                EditorGUILayout.HelpBox("No menu items found.", MessageType.Info);
                return;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            float totalHeight = _filtered.Count * RowHeight;
            Rect content = GUILayoutUtility.GetRect(0f, totalHeight, GUILayout.ExpandWidth(true));

            int firstVisible = Mathf.Max(0, Mathf.FloorToInt(_scroll.y / RowHeight) - 1);
            int visibleCount = Mathf.CeilToInt(position.height / RowHeight) + 2;
            int lastVisible = Mathf.Min(_filtered.Count, firstVisible + visibleCount);

            for (int i = firstVisible; i < lastVisible; i++)
            {
                Rect row = new(content.x, content.y + i * RowHeight, content.width, RowHeight);
                DrawRow(row, i);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawRow(Rect row, int index)
        {
            MenuItemEntry entry = _filtered[index];

            if (index % 2 == 0)
                EditorGUI.DrawRect(row, new Color(0f, 0f, 0f, 0.06f));

            MenuItemColumnLayout columns = new(row);

            GUI.Label(columns.Priority, entry.Priority.ToString(), _priorityStyle);
            DrawPath(columns.Path, entry);
            GUI.Label(columns.Member, entry.Member, EditorStyles.miniLabel);

            if (entry.IsValidation)
                GUI.Label(columns.Validation, new GUIContent("v", "Validation function"), _validationStyle);

            GUIContent badge = OriginBadge(entry.Origin);
            if (badge != null)
                GUI.Label(columns.Badge, badge, _badgeStyle);
        }
    }
}
#endif