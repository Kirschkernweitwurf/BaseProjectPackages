using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Base.ToolPackage.MenuManagerWindow;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.OverviewGui.UnusedAssetsOverviewWindow
{
    /// <summary>
    /// Editor window that lists assets that look unused and lets you ping, dismiss, or delete them.
    /// Dismissed assets are remembered per project, drop out of the count, and can be browsed and
    /// restored from a foldout at the top.
    /// </summary>
    public sealed class UnusedAssetsOverviewWindow : EditorWindow
    {
        private const float DiscardDefaultHeight = 220f;
        private const string DiscardHeightKey = "Base.UnusedAssetsOverview.DiscardHeight";
        private const float DiscardMinHeight = 60f;
        private const string MenuPath = "Tools/Base Packages/Project Health/Unused Assets Overview";

        private readonly List<UnusedAssetEntry> _entries = new();
        private readonly Dictionary<string, bool> _foldouts = new();
        private readonly Dictionary<string, bool> _discardFoldouts = new();

        private Vector2 _scroll;
        private Vector2 _discardScroll;
        private float _discardHeight;
        private float _discardContentHeight;
        private string _search = string.Empty;
        private bool _ignoreEditorFolders = true;
        private bool _hasScanned;
        private bool _showDiscard;
        private bool _showFound = true;
        private string _hoveredKey;
        private int _rowIndex;

        private List<UnusedAssetEntry> _discard;

#region Unity Callbacks
        private void OnEnable() => _discardHeight = EditorPrefs.GetFloat(DiscardHeightKey, DiscardDefaultHeight);

        private void OnGUI()
        {
            OverviewGui.EnsureStyles();
            HandleMouseMove();

            List<UnusedAssetEntry> active = _hasScanned
                ? ActiveEntries()
                : new List<UnusedAssetEntry>();

            List<UnusedAssetEntry> filtered = _hasScanned
                ? ApplySearch(active)
                : new List<UnusedAssetEntry>();

            DrawActionBar(filtered);
            DrawFilters(filtered);
            DrawSummary(active);

            if (!_hasScanned)
            {
                OverviewGui.DrawHint("Press Scan to search the project for unused assets.");
                return;
            }

            _hoveredKey = null;
            _rowIndex = 0;
            _discard ??= BuildDiscard();

            DrawDiscardSection();

            if (active.Count == 0)
            {
                string subtitle = UnusedAssetsDismissStore.Count > 0
                    ? "Everything else is dismissed. Nothing left to review."
                    : "Nothing under Assets looks unreferenced.";

                OverviewGui.DrawSuccess("No unused assets", subtitle);
                return;
            }

            if (filtered.Count == 0)
            {
                OverviewGui.DrawHint("No results match the search.");
                return;
            }

            DrawResults(filtered);
        }
#endregion

        [DynamicMenuItem(MenuPath)]
        private static void Open()
        {
            UnusedAssetsOverviewWindow window = GetWindow<UnusedAssetsOverviewWindow>();
            window.titleContent = new GUIContent("Unused Assets");
            window.minSize = new Vector2(520f, 340f);
            window.Show();
        }

        private static long GetFileSize(string assetPath)
        {
            try
            {
                string projectPath = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);
                return new FileInfo(projectPath + assetPath).Length;
            }
            catch
            {
                return 0L;
            }
        }

        private static List<UnusedAssetEntry> BuildDiscard()
        {
            List<UnusedAssetEntry> list = new();

            foreach (string guid in UnusedAssetsDismissStore.GetAll())
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (string.IsNullOrEmpty(path))
                    continue;

                Type type = AssetDatabase.GetMainAssetTypeAtPath(path);
                list.Add(new UnusedAssetEntry(path, guid, type != null
                    ? type.Name
                    : "Unknown", GetFileSize(path)));
            }

            list.Sort((first, second) =>
            {
                int byType = string.Compare(first.TypeName, second.TypeName, StringComparison.Ordinal);
                return byType != 0
                    ? byType
                    : string.Compare(first.Path, second.Path, StringComparison.Ordinal);
            });

            return list;
        }

        private void DrawActionBar(List<UnusedAssetEntry> filtered)
        {
            EditorGUILayout.Space(4f);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.Space(4f, false);

                if (GUILayout.Button("Scan Project", GUILayout.Height(26f), GUILayout.Width(140f)))
                    Rescan();

                using (new EditorGUI.DisabledScope(!_hasScanned || filtered.Count == 0))
                {
                    if (GUILayout.Button($"Delete All ({filtered.Count})", GUILayout.Height(26f),
                            GUILayout.Width(130f)))
                        DeleteAll(filtered);
                }

                GUILayout.FlexibleSpace();

                _search = EditorGUILayout.TextField(_search, EditorStyles.toolbarSearchField,
                    GUILayout.Width(200f), GUILayout.Height(20f));

                EditorGUILayout.Space(4f, false);
            }

            EditorGUILayout.Space(4f);
        }

        private void DrawFilters(List<UnusedAssetEntry> filtered)
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                GUIContent editorContent = new(" Ignore Editor folders",
                    "Skips assets under any Editor folder, which are usually referenced by editor code.");

                _ignoreEditorFolders = GUILayout.Toggle(_ignoreEditorFolders, editorContent, GUILayout.Width(150f));

                using (new EditorGUI.DisabledScope(!_hasScanned || filtered.Count == 0))
                {
                    if (GUILayout.Button($"Dismiss All ({filtered.Count})", GUILayout.Width(120f)))
                        DismissAll(filtered);
                }

                GUILayout.FlexibleSpace();

                GUILayout.Label($"Dismissed: {UnusedAssetsDismissStore.Count}", EditorStyles.miniLabel);

                using (new EditorGUI.DisabledScope(UnusedAssetsDismissStore.Count == 0))
                {
                    if (GUILayout.Button("Clear", GUILayout.Width(50f)))
                    {
                        UnusedAssetsDismissStore.Clear();
                        _discard = null;
                        Repaint();
                    }
                }
            }
        }

        private void DrawSummary(List<UnusedAssetEntry> active)
        {
            if (!_hasScanned)
                return;

            int count = active.Count;
            long totalBytes = active.Sum(entry => entry.SizeBytes);

            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                string message = count == 0
                    ? "No unused assets."
                    : $"{count} unused {OverviewGui.Plural(count, "asset", "assets")}, "
                    + $"{OverviewGui.FormatSize(totalBytes)}.";

                GUILayout.Label(message, OverviewGui.HeaderStyle);
            }
        }

        private void DrawDiscardSection()
        {
            if (_discard.Count == 0)
                return;

            _showDiscard = OverviewGui.DrawSectionHeader(_showDiscard, "Dismissed", _discard.Count,
                EOverviewAccent.Neutral);

            if (!_showDiscard)
                return;

            float maxHeight = _discardContentHeight > 0f
                ? _discardContentHeight
                : _discardHeight;

            float height = Mathf.Clamp(_discardHeight, DiscardMinHeight, Mathf.Max(DiscardMinHeight, maxHeight));

            _discardScroll = EditorGUILayout.BeginScrollView(_discardScroll, GUILayout.Height(height));

            using (new EditorGUILayout.VerticalScope())
            {
                IEnumerable<IGrouping<string, UnusedAssetEntry>> groups =
                    _discard.GroupBy(entry => entry.TypeName).OrderBy(group => group.Key);

                foreach (IGrouping<string, UnusedAssetEntry> group in groups)
                    DrawDiscardGroup(group);
            }

            if (Event.current.type == EventType.Repaint)
                _discardContentHeight = GUILayoutUtility.GetLastRect().height;

            EditorGUILayout.EndScrollView();

            _discardHeight = OverviewGui.DrawResizeHandle(height, DiscardMinHeight, maxHeight, DiscardHeightKey);
            EditorGUILayout.Space(4f);
        }

        private void DrawDiscardGroup(IGrouping<string, UnusedAssetEntry> group)
        {
            string key = "discard:" + group.Key;

            _discardFoldouts.TryAdd(key, true);

            int count = group.Count();
            Texture icon = AssetDatabase.GetCachedIcon(group.First().Path);

            using (new EditorGUILayout.HorizontalScope(OverviewGui.GroupStyle))
            {
                GUILayout.Label(icon, GUILayout.Width(18f), GUILayout.Height(16f));
                _discardFoldouts[key] = EditorGUILayout.Foldout(_discardFoldouts[key], group.Key, true);
                GUILayout.FlexibleSpace();
                GUILayout.Label(count.ToString(), OverviewGui.NeutralBadgeStyle, GUILayout.Width(30f),
                    GUILayout.Height(16f));
            }

            if (!_discardFoldouts[key])
                return;

            foreach (UnusedAssetEntry entry in group)
                DrawDiscardRow(entry);
        }

        private void DrawDiscardRow(UnusedAssetEntry entry)
        {
            Rect rect = BeginRow("discard:" + entry.Path);

            Rect iconRect = new(rect.x + 24f, rect.y + 3f, 16f, 16f);
            Texture icon = AssetDatabase.GetCachedIcon(entry.Path);

            if (icon != null)
                GUI.Label(iconRect, icon);

            Rect body = new(rect.x + 44f, rect.y, rect.width - 44f, rect.height);
            Rect labelRect = new(body.x, body.y, body.width - 128f, body.height);
            Rect gotoRect = new(body.xMax - 120f, body.y + 3f, 52f, body.height - 6f);
            Rect restoreRect = new(body.xMax - 64f, body.y + 3f, 64f, body.height - 6f);

            GUI.Label(labelRect,
                new GUIContent(entry.Path, entry.Path + "\n" + OverviewGui.FormatSize(entry.SizeBytes)),
                OverviewGui.PathStyle);

            if (GUI.Button(gotoRect, "Go to"))
                OverviewGui.Navigate(entry.Path);

            if (GUI.Button(restoreRect, "Restore"))
                Restore(entry);

            HandleRowClick(labelRect, entry);
        }

        private void DrawResults(List<UnusedAssetEntry> filtered)
        {
            _showFound = OverviewGui.DrawSectionHeader(_showFound, "Found", filtered.Count, EOverviewAccent.Warning);

            if (!_showFound)
                return;

            IEnumerable<IGrouping<string, UnusedAssetEntry>> groups =
                filtered.GroupBy(entry => entry.TypeName).OrderBy(group => group.Key);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            foreach (IGrouping<string, UnusedAssetEntry> group in groups)
                DrawGroup(group);

            EditorGUILayout.EndScrollView();
        }

        private void DrawGroup(IGrouping<string, UnusedAssetEntry> group)
        {
            string key = group.Key;

            _foldouts.TryAdd(key, true);

            int count = group.Count();
            Texture icon = AssetDatabase.GetCachedIcon(group.First().Path);

            using (new EditorGUILayout.HorizontalScope(OverviewGui.GroupStyle))
            {
                GUILayout.Label(icon, GUILayout.Width(18f), GUILayout.Height(16f));
                _foldouts[key] = EditorGUILayout.Foldout(_foldouts[key], key, true);
                GUILayout.FlexibleSpace();
                GUILayout.Label(count.ToString(), OverviewGui.WarningBadgeStyle, GUILayout.Width(30f),
                    GUILayout.Height(16f));
            }

            if (!_foldouts[key])
                return;

            foreach (UnusedAssetEntry entry in group)
                DrawRow(entry);
        }

        private void DrawRow(UnusedAssetEntry entry)
        {
            Rect rect = BeginRow(entry.Path);

            Rect iconRect = new(rect.x + 24f, rect.y + 3f, 16f, 16f);
            Texture icon = AssetDatabase.GetCachedIcon(entry.Path);

            if (icon != null)
                GUI.Label(iconRect, icon);

            Rect body = new(rect.x + 44f, rect.y, rect.width - 44f, rect.height);
            Rect labelRect = new(body.x, body.y, body.width - 196f, body.height);
            Rect gotoRect = new(body.xMax - 192f, body.y + 3f, 52f, body.height - 6f);
            Rect dismissRect = new(body.xMax - 136f, body.y + 3f, 68f, body.height - 6f);
            Rect removeRect = new(body.xMax - 64f, body.y + 3f, 64f, body.height - 6f);

            GUI.Label(labelRect,
                new GUIContent(entry.Path, entry.Path + "\n" + OverviewGui.FormatSize(entry.SizeBytes)),
                OverviewGui.PathStyle);

            if (GUI.Button(gotoRect, "Go to"))
                OverviewGui.Navigate(entry.Path);

            if (GUI.Button(dismissRect, "Dismiss"))
                Dismiss(entry);

            if (GUI.Button(removeRect, "Remove"))
                RemoveEntry(entry);

            HandleRowClick(labelRect, entry);
        }

        private Rect BeginRow(string key)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, OverviewGui.RowHeight);
            bool even = _rowIndex % 2 == 0;
            _rowIndex++;

            if (rect.Contains(Event.current.mousePosition))
                _hoveredKey = key;

            OverviewGui.DrawRowBackground(rect, key == _hoveredKey, even);

            return rect;
        }

        private void HandleRowClick(Rect labelRect, UnusedAssetEntry entry)
        {
            if (Event.current.type != EventType.MouseDown
                || !labelRect.Contains(Event.current.mousePosition))
                return;

            OverviewGui.Navigate(entry.Path);
            Event.current.Use();
        }

        private void Dismiss(UnusedAssetEntry entry)
        {
            UnusedAssetsDismissStore.Dismiss(entry.Guid);
            _discard = null;
            Repaint();
        }

        private void DismissAll(List<UnusedAssetEntry> entries)
        {
            if (entries.Count == 0)
                return;

            UnusedAssetsDismissStore.DismissRange(entries.Select(entry => entry.Guid));
            _discard = null;
            Repaint();
        }

        private void Restore(UnusedAssetEntry entry)
        {
            UnusedAssetsDismissStore.Restore(entry.Guid);
            _discard = null;
            Repaint();
        }

        private void RemoveEntry(UnusedAssetEntry entry)
        {
            if (!AssetDatabase.DeleteAsset(entry.Path))
                return;

            AssetDatabase.Refresh();
            _entries.Remove(entry);
            Repaint();
        }

        private void DeleteAll(List<UnusedAssetEntry> entries)
        {
            if (entries.Count == 0)
                return;

            bool confirmed = EditorUtility.DisplayDialog("Delete Unused Assets",
                $"Delete {entries.Count} {OverviewGui.Plural(entries.Count, "asset", "assets")}?\n\n"
                + "This only checks build scenes, Resources, the render pipeline, preloaded assets, "
                + "ProjectSettings, and Addressables. Assets loaded by code or by path are not detected, "
                + "so review the list first.",
                "Delete",
                "Cancel");

            if (!confirmed)
                return;

            foreach (UnusedAssetEntry entry in entries)
                AssetDatabase.DeleteAsset(entry.Path);

            AssetDatabase.Refresh();
            Rescan();
        }

        private List<UnusedAssetEntry> ActiveEntries()
            => _entries.Where(entry => !UnusedAssetsDismissStore.IsDismissed(entry.Guid)).ToList();

        private List<UnusedAssetEntry> ApplySearch(List<UnusedAssetEntry> source)
        {
            if (string.IsNullOrWhiteSpace(_search))
                return source;

            string term = _search.Trim();

            return source.Where(entry =>
                    entry.Path.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0
                    || entry.TypeName.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
        }

        private void Rescan()
        {
            _entries.Clear();
            _entries.AddRange(UnusedAssetScanner.Scan(_ignoreEditorFolders));
            _hasScanned = true;
            _discard = null;
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