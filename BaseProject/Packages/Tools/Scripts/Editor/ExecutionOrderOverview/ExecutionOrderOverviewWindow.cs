using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.ExecutionOrderOverview
{
    /// <summary>
    /// Editor window that lists every script with a custom execution order, sorted by the
    /// order that wins at runtime. Only the rows currently visible in the scroll view are
    /// drawn, so the window stays responsive even with a large number of results.
    /// </summary>
    public sealed class ExecutionOrderOverviewWindow : EditorWindow
    {
        private const float RowHeight = 22f;

        private readonly List<ExecutionOrderEntry> _all = new();
        private readonly List<ExecutionOrderEntry> _filtered = new();

        private IExecutionOrderSource _source;
        private GUIStyle _orderStyle;
        private GUIStyle _countStyle;
        private GUIStyle _badgeStyle;
        private string _search = string.Empty;
        private bool _includeExternal = true;
        private bool _ascending = true;
        private bool _needsRebuild = true;
        private Vector2 _scroll;

        /// <summary>Opens or focuses the window from the Tools menu.</summary>
        [MenuItem("Tools/Base Packages/Code Health/Execution Order Overview", priority = 2)]
        private static void Open()
        {
            ExecutionOrderOverviewWindow window = GetWindow<ExecutionOrderOverviewWindow>("Execution Order");
            window.minSize = new Vector2(520f, 320f);
            window.Show();
        }

        private void OnEnable()
        {
            _source = new MonoScriptExecutionOrderSource();
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

        private static void OpenEntry(ExecutionOrderEntry entry)
        {
            (int line, int column) = ScriptDefinitionLocator.Find(entry.Script, entry.Type);
            AssetDatabase.OpenAsset(entry.Script, line, column);
            EditorGUIUtility.PingObject(entry.Script);
        }

        private static GUIContent OriginBadge(ScriptOrigin origin)
        {
            return origin switch
            {
                ScriptOrigin.Package => new GUIContent("pkg", "This script lives in a package"),
                ScriptOrigin.BuiltIn => new GUIContent("lib", "This script is built into Unity"),
                _ => null
            };
        }

        private void EnsureStyles()
        {
            if (_orderStyle != null)
                return;

            _orderStyle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleRight };
            _countStyle = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleRight, fixedHeight = 0f };
            _badgeStyle = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleRight };
        }

        private void Rebuild()
        {
            _all.Clear();
            _all.AddRange(_source.Collect());
            RunQuery();
            _needsRebuild = false;
        }

        private void RunQuery()
        {
            _filtered.Clear();
            _filtered.AddRange(ExecutionOrderQuery.Apply(_all, _search, _includeExternal, _ascending));
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUI.BeginChangeCheck();

                _search = GUILayout.TextField(_search, EditorStyles.toolbarSearchField, GUILayout.MinWidth(160f));
                _includeExternal = GUILayout.Toggle(_includeExternal, "Include external", EditorStyles.toolbarButton);

                if (GUILayout.Button(_ascending ? "Order \u2191" : "Order \u2193", EditorStyles.toolbarButton, GUILayout.Width(72f)))
                    _ascending = !_ascending;

                if (EditorGUI.EndChangeCheck())
                    RunQuery();

                GUILayout.FlexibleSpace();
                GUILayout.Label($"{_filtered.Count} scripts", _countStyle, GUILayout.ExpandHeight(true));

                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(64f)))
                    _needsRebuild = true;
            }
        }

        private void DrawHeader()
        {
            Rect row = GUILayoutUtility.GetRect(0f, RowHeight, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(row, new Color(0f, 0f, 0f, 0.12f));

            ExecutionOrderColumnLayout columns = new(row);
            GUIStyle style = EditorStyles.miniBoldLabel;
            GUI.Label(columns.Order, "Order", style);
            GUI.Label(columns.Name, "Script", style);
            GUI.Label(columns.Namespace, "Namespace", style);
        }

        private void DrawList()
        {
            if (_filtered.Count == 0)
            {
                EditorGUILayout.HelpBox("No scripts with a custom execution order found.", MessageType.Info);
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
            ExecutionOrderEntry entry = _filtered[index];

            if (index % 2 == 0)
                EditorGUI.DrawRect(row, new Color(0f, 0f, 0f, 0.06f));

            ExecutionOrderColumnLayout columns = new(row);

            GUI.Label(columns.Order, entry.EffectiveOrder.ToString(), _orderStyle);

            EditorGUIUtility.AddCursorRect(columns.Name, MouseCursor.Link);
            if (GUI.Button(columns.Name, new GUIContent(entry.Name, entry.AssetPath), EditorStyles.label))
                OpenEntry(entry);

            GUI.Label(columns.Namespace, entry.Namespace, EditorStyles.miniLabel);

            GUIContent badge = OriginBadge(entry.Origin);
            if (badge != null)
                GUI.Label(columns.Badge, badge, _badgeStyle);
        }
    }
}