#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Base.ToolPackage.MenuManagerWindow;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.CreateAssetMenuOverview
{
    /// <summary>
    /// Editor window that lists every <see cref="CreateAssetMenuAttribute"/> in the project,
    /// its packages and Unity itself, sorted by menu order. Clicking a project or package
    /// type opens its script at the order argument. Only the rows currently visible in the
    /// scroll view are drawn, so the window stays responsive with a large number of results.
    /// </summary>
    public sealed class CreateAssetMenuOverviewWindow : EditorWindow
    {
        private const string AllRootsLabel = "All";
        private const float RowHeight = 22f;

        private readonly List<CreateAssetEntry> _all = new();
        private readonly List<CreateAssetEntry> _filtered = new();

        private ICreateAssetSource _source;
        private GUIStyle _orderStyle;
        private GUIStyle _countStyle;
        private GUIStyle _badgeStyle;
        private string[] _roots =
        {
            AllRootsLabel
        };
        private string _root = AllRootsLabel;
        private string _search = string.Empty;
        private bool _includeExternal = true;
        private bool _ascending = true;
        private bool _needsRebuild = true;
        private Vector2 _scroll;

#region Unity Callbacks
        private void OnEnable()
        {
            _source = new ReflectionCreateAssetSource();
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
        [DynamicMenuItem("Tools/Base Packages/Code/Health/Create Asset Menu Overview")]
        private static void Open()
        {
            CreateAssetMenuOverviewWindow window = GetWindow<CreateAssetMenuOverviewWindow>("Create Assets");
            window.minSize = new Vector2(640f, 320f);
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

        private static GUIContent OriginBadge(ECreateAssetOrigin origin) => origin switch
        {
            ECreateAssetOrigin.Package => new GUIContent("pkg", "This type lives in a package"),
            ECreateAssetOrigin.BuiltIn => new GUIContent("lib", "This type is built into Unity"),
            _ => null
        };

        private static void DrawMenuName(Rect rect, CreateAssetEntry entry)
        {
            if (entry.Script == null)
            {
                GUI.Label(rect, new GUIContent(entry.MenuName, entry.MenuName), EditorStyles.label);
                return;
            }

            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
            if (GUI.Button(rect, new GUIContent(entry.MenuName, entry.AssetPath), EditorStyles.label))
                OpenEntry(entry);
        }

        private void EnsureStyles()
        {
            if (_orderStyle != null)
                return;

            _orderStyle = new GUIStyle(EditorStyles.boldLabel)
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
            _filtered.AddRange(CreateAssetQuery.Apply(_all, _search, root, _includeExternal, _ascending));
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

                string label = _ascending
                    ? "Order \u2191"
                    : "Order \u2193";

                if (GUILayout.Button(label, EditorStyles.toolbarButton, GUILayout.Width(72f)))
                    _ascending = !_ascending;

                if (EditorGUI.EndChangeCheck())
                    RunQuery();

                GUILayout.FlexibleSpace();
                GUILayout.Label($"{_filtered.Count} types", _countStyle, GUILayout.ExpandHeight(true));

                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(64f)))
                    _needsRebuild = true;
            }
        }

        private void DrawHeader()
        {
            Rect row = GUILayoutUtility.GetRect(0f, RowHeight, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(row, new Color(0f, 0f, 0f, 0.12f));

            CreateAssetColumnLayout columns = new(row);
            GUIStyle style = EditorStyles.miniBoldLabel;
            GUI.Label(columns.Order, "Order", style);
            GUI.Label(columns.MenuName, "Menu Name", style);
            GUI.Label(columns.Type, "Type", style);
            GUI.Label(columns.FileName, "File Name", style);
        }

        private void DrawList()
        {
            if (_filtered.Count == 0)
            {
                EditorGUILayout.HelpBox("No CreateAssetMenu types found.", MessageType.Info);
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
            CreateAssetEntry entry = _filtered[index];

            if (index % 2 == 0)
                EditorGUI.DrawRect(row, new Color(0f, 0f, 0f, 0.06f));

            CreateAssetColumnLayout columns = new(row);

            GUI.Label(columns.Order, entry.Order.ToString(), _orderStyle);
            DrawMenuName(columns.MenuName, entry);
            GUIContent type = new(entry.TypeName, entry.DeclaringType.FullName);
            GUI.Label(columns.Type, type, EditorStyles.miniLabel);
            GUI.Label(columns.FileName, new GUIContent(entry.FileName, entry.FileName), EditorStyles.miniLabel);

            GUIContent badge = OriginBadge(entry.Origin);
            if (badge != null)
                GUI.Label(columns.Badge, badge, _badgeStyle);
        }
    }
}
#endif