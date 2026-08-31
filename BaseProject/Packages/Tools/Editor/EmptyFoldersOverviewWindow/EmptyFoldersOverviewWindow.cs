using System;
using System.Collections.Generic;
using System.Linq;
using Base.UtilityPackage.Menus;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Overview = Base.ToolPackage.Editor.OverviewGui.OverviewGui;

namespace Base.ToolPackage.Editor.EmptyFoldersOverviewWindow
{
    /// <summary>
    /// Editor window that lists empty folders and lets you jump to or delete them.
    /// </summary>
    public sealed class EmptyFoldersOverviewWindow : EditorWindow
    {
        private const string MenuPath = "Tools/Base Packages/Unity Editor/Project Health/Unused/Empty Folders Overview";

        private readonly List<EmptyFolderEntry> _entries = new();
        private readonly List<EmptyFolderEntry> _pendingDeletes = new();

        private Vector2 _scroll;
        private string _search = string.Empty;
        private bool _hasScanned;
        private string _hoveredKey;
        private int _rowIndex;
        private bool _pendingRescan;
        private bool _pendingDeleteAll;

#region Unity Callbacks
        private void OnGUI()
        {
            Overview.EnsureStyles();
            HandleMouseMove();

            List<EmptyFolderEntry> filtered = _hasScanned
                ? FilterEntries()
                : new List<EmptyFolderEntry>();

            DrawActionBar(filtered);
            DrawSummary();
            DrawBody(filtered);
            ProcessPendingActions(filtered);
        }
#endregion

        [DynamicMenuItem(MenuPath)]
        private static void Open()
        {
            EmptyFoldersOverviewWindow window = GetWindow<EmptyFoldersOverviewWindow>();
            window.titleContent = new GUIContent("Empty Folders");
            window.minSize = new Vector2(460f, 320f);
            window.Show();
        }

        private static void Navigate(EmptyFolderEntry entry)
        {
            Object folder = AssetDatabase.LoadAssetAtPath<Object>(entry.Path);

            if (folder == null)
                return;

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = folder;
            EditorGUIUtility.PingObject(folder);
        }

        private static GUIContent GetFolderIcon() => EditorGUIUtility.IconContent("Folder Icon");

        private void DrawActionBar(List<EmptyFolderEntry> filtered)
        {
            EditorGUILayout.Space(4f);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.Space(4f, false);

                if (GUILayout.Button("Scan Project", GUILayout.Height(26f), GUILayout.Width(140f)))
                    _pendingRescan = true;

                using (new EditorGUI.DisabledScope(!_hasScanned || filtered.Count == 0))
                {
                    if (GUILayout.Button($"Delete All ({filtered.Count})", GUILayout.Height(26f),
                            GUILayout.Width(130f)))
                        _pendingDeleteAll = true;
                }

                GUILayout.FlexibleSpace();

                _search = EditorGUILayout.TextField(_search, EditorStyles.toolbarSearchField,
                    GUILayout.Width(200f), GUILayout.Height(20f));

                EditorGUILayout.Space(4f, false);
            }

            EditorGUILayout.Space(4f);
        }

        private void DrawSummary()
        {
            if (!_hasScanned)
                return;

            int folders = _entries.Count;
            int totalWithNested = _entries.Sum(entry => entry.NestedFolderCount);

            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                if (folders == 0)
                {
                    GUILayout.Label("No empty folders.", Overview.HeaderStyle);
                }
                else
                {
                    string message = $"{folders} empty {Overview.Plural(folders, "folder", "folders")} found";

                    if (totalWithNested != folders)
                        message += $" ({totalWithNested} including nested)";

                    GUILayout.Label(message + ".", Overview.HeaderStyle);
                }
            }
        }

        private void DrawBody(List<EmptyFolderEntry> filtered)
        {
            if (!_hasScanned)
            {
                Overview.DrawHint("Press Scan to search the project for empty folders.");
                return;
            }

            if (_entries.Count == 0)
            {
                Overview.DrawSuccess("No empty folders", "Every folder has content. Nothing to clean up.");
                return;
            }

            if (filtered.Count == 0)
            {
                Overview.DrawHint("No results match the search.");
                return;
            }

            DrawResults(filtered);
        }

        private void DrawResults(List<EmptyFolderEntry> filtered)
        {
            _hoveredKey = null;
            _rowIndex = 0;

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            foreach (EmptyFolderEntry entry in filtered)
                DrawRow(entry);

            EditorGUILayout.EndScrollView();
        }

        private void DrawRow(EmptyFolderEntry entry)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, Overview.RowHeight);
            string key = entry.Path;
            bool even = _rowIndex % 2 == 0;
            _rowIndex++;

            if (rect.Contains(Event.current.mousePosition))
                _hoveredKey = key;

            bool hovered = key == _hoveredKey;

            Overview.DrawRowBackground(rect, hovered, even);

            Rect iconRect = new(rect.x + 4f, rect.y + 3f, 16f, 16f);
            GUI.Label(iconRect, GetFolderIcon());

            Rect body = new(rect.x + 24f, rect.y, rect.width - 24f, rect.height);
            Rect labelRect = new(body.x, body.y, body.width - 170f, body.height);
            Rect badgeRect = new(body.xMax - 160f, body.y + 3f, 34f, body.height - 6f);
            Rect gotoRect = new(body.xMax - 120f, body.y + 3f, 52f, body.height - 6f);
            Rect deleteRect = new(body.xMax - 64f, body.y + 3f, 64f, body.height - 6f);

            GUI.Label(labelRect, new GUIContent(entry.Path, entry.Path), Overview.PathStyle);

            if (entry.NestedFolderCount > 1)
                GUI.Label(badgeRect,
                    new GUIContent(entry.NestedFolderCount.ToString(),
                        $"Removes {entry.NestedFolderCount} folders including nested empties."),
                    Overview.WarningBadgeStyle);

            if (GUI.Button(gotoRect, "Go to"))
                Navigate(entry);

            if (GUI.Button(deleteRect, "Delete"))
                _pendingDeletes.Add(entry);

            if (Event.current.type == EventType.MouseDown
                && labelRect.Contains(Event.current.mousePosition))
            {
                Navigate(entry);
                Event.current.Use();
            }
        }

        private void ProcessPendingActions(List<EmptyFolderEntry> filtered)
        {
            if (_pendingDeleteAll)
            {
                _pendingDeleteAll = false;
                _pendingDeletes.Clear();
                DeleteAll(filtered);
                return;
            }

            if (_pendingDeletes.Count > 0)
            {
                foreach (EmptyFolderEntry entry in _pendingDeletes)
                    DeleteEntry(entry);

                _pendingDeletes.Clear();
                Repaint();
                return;
            }

            if (!_pendingRescan)
                return;

            _pendingRescan = false;
            Rescan();
        }

        private void DeleteEntry(EmptyFolderEntry entry)
        {
            if (!AssetDatabase.DeleteAsset(entry.Path))
                return;

            AssetDatabase.Refresh();
            _entries.Remove(entry);
        }

        private void DeleteAll(List<EmptyFolderEntry> entries)
        {
            if (entries.Count == 0)
                return;

            bool confirmed = EditorUtility.DisplayDialog("Delete Empty Folders",
                $"Delete {entries.Count} empty {Overview.Plural(entries.Count, "folder", "folders")}?",
                "Delete",
                "Cancel");

            if (!confirmed)
                return;

            foreach (EmptyFolderEntry entry in entries)
                AssetDatabase.DeleteAsset(entry.Path);

            AssetDatabase.Refresh();

            // Deleting can make parent folders empty, so scan again to catch them.
            Rescan();
        }

        private List<EmptyFolderEntry> FilterEntries()
        {
            if (string.IsNullOrWhiteSpace(_search))
                return new List<EmptyFolderEntry>(_entries);

            string term = _search.Trim();

            return _entries
                .Where(entry => entry.Path.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
        }

        private void Rescan()
        {
            _entries.Clear();
            _entries.AddRange(EmptyFolderScanner.Scan());
            _hasScanned = true;
            Repaint();
        }

        private void HandleMouseMove()
        {
            wantsMouseMove = true;

            if (Event.current.type == EventType.MouseMove)
                Repaint();
        }
    }
}