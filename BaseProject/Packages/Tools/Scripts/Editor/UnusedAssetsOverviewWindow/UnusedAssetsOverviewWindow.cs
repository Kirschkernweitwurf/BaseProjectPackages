using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.ToolPackage.Editor.UnusedAssetsOverviewWindow
{
    /// <summary>
    /// Editor window that lists assets that look unused and lets you ping, dismiss, or delete them.
    /// Dismissed assets are remembered per project, drop out of the count, and can be browsed and
    /// restored from a foldout at the top.
    /// </summary>
    public sealed class UnusedAssetsOverviewWindow : EditorWindow
    {
        private const float DiscardMaxHeight = 220f;
        private const string MenuPath = "Tools/Base/Unused Assets Overview";
        private const float RowHeight = 22f;
        private const float SuccessGap = 8f;
        private const float SuccessIconSize = 48f;
        private const int SuccessTitleFontSize = 15;

        private readonly List<UnusedAssetEntry> _entries = new();
        private readonly Dictionary<string, bool> _foldouts = new();
        private readonly Dictionary<string, bool> _discardFoldouts = new();

        private Vector2 _scroll;
        private Vector2 _discardScroll;
        private string _search = string.Empty;
        private bool _ignoreEditorFolders = true;
        private bool _hasScanned;
        private bool _showDiscard;
        private string _hoveredKey;
        private int _rowIndex;

        private List<UnusedAssetEntry> _discard;

        private GUIStyle _headerStyle;
        private GUIStyle _groupStyle;
        private GUIStyle _pathStyle;
        private GUIStyle _badgeStyle;
        private GUIStyle _successTitleStyle;
        private GUIStyle _successSubtitleStyle;
        private Texture2D _badgeTexture;
        private Texture _successTexture;
        private bool _stylesReady;

#region Unity Callbacks
        private void OnGUI()
        {
            EnsureStyles();
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
                DrawHint("Press Scan to search the project for unused assets.");
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

                DrawSuccess("No unused assets", subtitle);
                return;
            }

            if (filtered.Count == 0)
            {
                DrawHint("No results match the search.");
                return;
            }

            DrawResults(filtered);
        }
#endregion

        [MenuItem(MenuPath)]
        private static void Open()
        {
            UnusedAssetsOverviewWindow window = GetWindow<UnusedAssetsOverviewWindow>();
            window.titleContent = new GUIContent("Unused Assets");
            window.minSize = new Vector2(520f, 340f);
            window.Show();
        }

        private static void Navigate(UnusedAssetEntry entry)
        {
            Object asset = AssetDatabase.LoadMainAssetAtPath(entry.Path);

            if (asset == null)
                return;

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        private static string Plural(int amount, string singular, string plural) => amount == 1
            ? singular
            : plural;

        private static string FormatSize(long bytes)
        {
            if (bytes >= 1024L * 1024L)
                return (bytes / (1024f * 1024f)).ToString("0.0") + " MB";

            if (bytes >= 1024L)
                return (bytes / 1024f).ToString("0.0") + " KB";

            return bytes + " B";
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

        private static Texture2D MakeSolidTexture(Color color)
        {
            Texture2D texture = new(1, 1)
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
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
                    : $"{count} unused {Plural(count, "asset", "assets")}, {FormatSize(totalBytes)}.";

                GUILayout.Label(message, _headerStyle);
            }
        }

        private void DrawDiscardSection()
        {
            if (_discard.Count == 0)
                return;

            using (new EditorGUILayout.HorizontalScope(_groupStyle))
            {
                _showDiscard = EditorGUILayout.Foldout(_showDiscard, "Dismissed", true);
                GUILayout.FlexibleSpace();
                GUILayout.Label(_discard.Count.ToString(), _badgeStyle, GUILayout.Width(30f), GUILayout.Height(16f));
            }

            if (!_showDiscard)
                return;

            _discardScroll = EditorGUILayout.BeginScrollView(_discardScroll, GUILayout.MaxHeight(DiscardMaxHeight));

            IEnumerable<IGrouping<string, UnusedAssetEntry>> groups =
                _discard.GroupBy(entry => entry.TypeName).OrderBy(group => group.Key);

            foreach (IGrouping<string, UnusedAssetEntry> group in groups)
                DrawDiscardGroup(group);

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(4f);
        }

        private void DrawDiscardGroup(IGrouping<string, UnusedAssetEntry> group)
        {
            string key = "discard:" + group.Key;

            if (!_discardFoldouts.ContainsKey(key))
                _discardFoldouts[key] = true;

            int count = group.Count();
            Texture icon = AssetDatabase.GetCachedIcon(group.First().Path);

            using (new EditorGUILayout.HorizontalScope(_groupStyle))
            {
                GUILayout.Label(icon, GUILayout.Width(18f), GUILayout.Height(16f));
                _discardFoldouts[key] = EditorGUILayout.Foldout(_discardFoldouts[key], group.Key, true);
                GUILayout.FlexibleSpace();
                GUILayout.Label(count.ToString(), _badgeStyle, GUILayout.Width(30f), GUILayout.Height(16f));
            }

            if (!_discardFoldouts[key])
                return;

            foreach (UnusedAssetEntry entry in group)
                DrawDiscardRow(entry);
        }

        private void DrawDiscardRow(UnusedAssetEntry entry)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, RowHeight);
            string key = "discard:" + entry.Path;
            bool even = _rowIndex % 2 == 0;
            _rowIndex++;

            if (rect.Contains(Event.current.mousePosition))
                _hoveredKey = key;

            bool hovered = key == _hoveredKey;

            if (Event.current.type == EventType.Repaint)
            {
                if (hovered)
                    EditorGUI.DrawRect(rect, new Color(0.35f, 0.55f, 0.95f, 0.18f));
                else if (even)
                    EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.06f));
            }

            Rect iconRect = new(rect.x + 24f, rect.y + 3f, 16f, 16f);
            Texture icon = AssetDatabase.GetCachedIcon(entry.Path);

            if (icon != null)
                GUI.Label(iconRect, icon);

            Rect body = new(rect.x + 44f, rect.y, rect.width - 44f, rect.height);
            Rect labelRect = new(body.x, body.y, body.width - 128f, body.height);
            Rect gotoRect = new(body.xMax - 120f, body.y + 3f, 52f, body.height - 6f);
            Rect restoreRect = new(body.xMax - 64f, body.y + 3f, 64f, body.height - 6f);

            GUI.Label(labelRect,
                new GUIContent(entry.Path, entry.Path + "\n" + FormatSize(entry.SizeBytes)),
                _pathStyle);

            if (GUI.Button(gotoRect, "Go to"))
                Navigate(entry);

            if (GUI.Button(restoreRect, "Restore"))
                Restore(entry);

            if (Event.current.type == EventType.MouseDown && labelRect.Contains(Event.current.mousePosition))
            {
                Navigate(entry);
                Event.current.Use();
            }
        }

        private void DrawResults(List<UnusedAssetEntry> filtered)
        {
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

            if (!_foldouts.ContainsKey(key))
                _foldouts[key] = true;

            int count = group.Count();
            Texture icon = AssetDatabase.GetCachedIcon(group.First().Path);

            using (new EditorGUILayout.HorizontalScope(_groupStyle))
            {
                GUILayout.Label(icon, GUILayout.Width(18f), GUILayout.Height(16f));
                _foldouts[key] = EditorGUILayout.Foldout(_foldouts[key], key, true);
                GUILayout.FlexibleSpace();
                GUILayout.Label(count.ToString(), _badgeStyle, GUILayout.Width(30f), GUILayout.Height(16f));
            }

            if (!_foldouts[key])
                return;

            foreach (UnusedAssetEntry entry in group)
                DrawRow(entry);
        }

        private void DrawRow(UnusedAssetEntry entry)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, RowHeight);
            string key = entry.Path;
            bool even = _rowIndex % 2 == 0;
            _rowIndex++;

            if (rect.Contains(Event.current.mousePosition))
                _hoveredKey = key;

            bool hovered = key == _hoveredKey;

            if (Event.current.type == EventType.Repaint)
            {
                if (hovered)
                    EditorGUI.DrawRect(rect, new Color(0.35f, 0.55f, 0.95f, 0.18f));
                else if (even)
                    EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.06f));
            }

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
                new GUIContent(entry.Path, entry.Path + "\n" + FormatSize(entry.SizeBytes)),
                _pathStyle);

            if (GUI.Button(gotoRect, "Go to"))
                Navigate(entry);

            if (GUI.Button(dismissRect, "Dismiss"))
                Dismiss(entry);

            if (GUI.Button(removeRect, "Remove"))
                RemoveEntry(entry);

            if (Event.current.type == EventType.MouseDown && labelRect.Contains(Event.current.mousePosition))
            {
                Navigate(entry);
                Event.current.Use();
            }
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
                $"Delete {entries.Count} {Plural(entries.Count, "asset", "assets")}?\n\n"
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

        private List<UnusedAssetEntry> BuildDiscard()
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

        private void DrawHint(string message)
        {
            GUILayout.FlexibleSpace();

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(message, EditorStyles.centeredGreyMiniLabel);
                GUILayout.FlexibleSpace();
            }

            GUILayout.FlexibleSpace();
        }

        private void DrawSuccess(string title, string subtitle)
        {
            GUILayout.FlexibleSpace();

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                GUILayout.Label(new GUIContent(_successTexture),
                    GUILayout.Width(SuccessIconSize),
                    GUILayout.Height(SuccessIconSize));

                GUILayout.FlexibleSpace();
            }

            GUILayout.Space(SuccessGap);

            GUILayout.Label(title, _successTitleStyle);
            GUILayout.Label(subtitle, _successSubtitleStyle);

            GUILayout.FlexibleSpace();
        }

        private void EnsureStyles()
        {
            if (_stylesReady)
                return;

            _headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12
            };

            _groupStyle = new GUIStyle(EditorStyles.helpBox)
            {
                margin = new RectOffset(2, 2, 2, 0),
                padding = new RectOffset(6, 6, 3, 3)
            };

            _pathStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft
            };

            // Warning yellow, matching the Unity console warning icon, with dark text for contrast.
            _badgeTexture = MakeSolidTexture(new Color(0.96f, 0.78f, 0.12f));

            _badgeStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal =
                {
                    textColor = new Color(0.15f, 0.13f, 0.05f),
                    background = _badgeTexture
                }
            };

            _successTitleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = SuccessTitleFontSize,
                normal =
                {
                    textColor = new Color(0.36f, 0.76f, 0.46f)
                }
            };

            _successSubtitleStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                normal =
                {
                    textColor = new Color(0.5f, 0.5f, 0.5f)
                }
            };

            _successTexture = EditorGUIUtility.IconContent("TestPassed").image;

            _stylesReady = true;
        }
    }
}