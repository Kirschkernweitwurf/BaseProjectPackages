using System;
using System.Collections.Generic;
using System.Linq;
using Base.UnusedScriptsPackage;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.ToolPackage.Editor.UnusedScriptsOverviewWindow
{
    /// <summary>
    /// Editor window that lists scripts that look dead and lets you ping, dismiss, or delete them.
    /// Dismissed scripts are remembered per project, drop out of the count, and can be browsed and
    /// restored from a foldout at the top.
    /// </summary>
    public sealed class UnusedScriptsOverviewWindow : EditorWindow
    {
        private const float DiscardMaxHeight = 220f;
        private const string MenuPath = "Tools/Base/Unused Scripts Overview";
        private const float RowHeight = 22f;
        private const float SuccessGap = 8f;
        private const float SuccessIconSize = 48f;
        private const int SuccessTitleFontSize = 15;

        private readonly List<UnusedScriptEntry> _entries = new();
        private readonly Dictionary<string, bool> _foldouts = new();
        private readonly Dictionary<string, bool> _discardFoldouts = new();

        private Vector2 _scroll;
        private Vector2 _discardScroll;
        private string _search = string.Empty;
        private bool _ignoreEditorScripts = true;
        private bool _hasScanned;
        private bool _showDiscard;
        private string _hoveredKey;
        private int _rowIndex;

        private List<UnusedScriptEntry> _discard;

        private GUIStyle _headerStyle;
        private GUIStyle _groupStyle;
        private GUIStyle _pathStyle;
        private GUIStyle _badgeStyle;
        private GUIStyle _neutralBadgeStyle;
        private GUIStyle _successTitleStyle;
        private GUIStyle _successSubtitleStyle;
        private Texture2D _badgeTexture;
        private Texture2D _neutralBadgeTexture;
        private Texture _successTexture;
        private bool _stylesReady;

#region Unity Callbacks
        private void OnGUI()
        {
            EnsureStyles();
            HandleMouseMove();

            List<UnusedScriptEntry> active = _hasScanned
                ? ActiveEntries()
                : new List<UnusedScriptEntry>();

            List<UnusedScriptEntry> filtered = _hasScanned
                ? ApplySearch(active)
                : new List<UnusedScriptEntry>();

            DrawActionBar(filtered);
            DrawFilters(filtered);
            DrawSummary(active);

            if (!_hasScanned)
            {
                DrawHint("Press Scan to search the project for dead scripts.");
                return;
            }

            _hoveredKey = null;
            _rowIndex = 0;
            _discard ??= BuildDiscard();

            DrawDiscardSection();

            if (active.Count == 0)
            {
                string subtitle = UnusedScriptsDismissStore.Count > 0
                    ? "Everything else is dismissed. Nothing left to review."
                    : "Every script is referenced somewhere.";

                DrawSuccess("No dead scripts", subtitle);
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
            UnusedScriptsOverviewWindow window = GetWindow<UnusedScriptsOverviewWindow>();
            window.titleContent = new GUIContent("Unused Scripts");
            window.minSize = new Vector2(520f, 340f);
            window.Show();
        }

        private static void Navigate(UnusedScriptEntry entry)
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

        private void DrawActionBar(List<UnusedScriptEntry> filtered)
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

        private void DrawFilters(List<UnusedScriptEntry> filtered)
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                GUIContent editorContent = new(" Ignore Editor scripts",
                    "Skips scripts in editor assemblies, which Unity often runs through attributes.");

                _ignoreEditorScripts = GUILayout.Toggle(_ignoreEditorScripts, editorContent, GUILayout.Width(160f));

                using (new EditorGUI.DisabledScope(!_hasScanned || filtered.Count == 0))
                {
                    if (GUILayout.Button($"Dismiss All ({filtered.Count})", GUILayout.Width(120f)))
                        DismissAll(filtered);
                }

                GUILayout.FlexibleSpace();

                GUILayout.Label($"Dismissed: {UnusedScriptsDismissStore.Count}", EditorStyles.miniLabel);

                using (new EditorGUI.DisabledScope(UnusedScriptsDismissStore.Count == 0))
                {
                    if (GUILayout.Button("Clear", GUILayout.Width(50f)))
                    {
                        UnusedScriptsDismissStore.Clear();
                        _discard = null;
                        Repaint();
                    }
                }
            }
        }

        private void DrawSummary(List<UnusedScriptEntry> active)
        {
            if (!_hasScanned)
                return;

            int count = active.Count;

            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                string message = count == 0
                    ? "No dead scripts."
                    : $"{count} dead {Plural(count, "script", "scripts")}.";

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
                GUILayout.Label(_discard.Count.ToString(), _neutralBadgeStyle, GUILayout.Width(30f),
                    GUILayout.Height(16f));
            }

            if (!_showDiscard)
                return;

            _discardScroll = EditorGUILayout.BeginScrollView(_discardScroll, GUILayout.MaxHeight(DiscardMaxHeight));

            IEnumerable<IGrouping<string, UnusedScriptEntry>> groups =
                _discard.GroupBy(entry => entry.Folder).OrderBy(group => group.Key);

            foreach (IGrouping<string, UnusedScriptEntry> group in groups)
                DrawDiscardGroup(group);

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(4f);
        }

        private void DrawDiscardGroup(IGrouping<string, UnusedScriptEntry> group)
        {
            string key = "discard:" + group.Key;

            if (!_discardFoldouts.ContainsKey(key))
                _discardFoldouts[key] = true;

            int count = group.Count();

            using (new EditorGUILayout.HorizontalScope(_groupStyle))
            {
                GUILayout.Label(EditorGUIUtility.IconContent("Folder Icon"), GUILayout.Width(18f),
                    GUILayout.Height(16f));

                _discardFoldouts[key] = EditorGUILayout.Foldout(_discardFoldouts[key], group.Key, true);
                GUILayout.FlexibleSpace();
                GUILayout.Label(count.ToString(), _neutralBadgeStyle, GUILayout.Width(30f), GUILayout.Height(16f));
            }

            if (!_discardFoldouts[key])
                return;

            foreach (UnusedScriptEntry entry in group)
                DrawDiscardRow(entry);
        }

        private void DrawDiscardRow(UnusedScriptEntry entry)
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

            GUI.Label(labelRect, new GUIContent(entry.Name, entry.Path), _pathStyle);

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

        private void DrawResults(List<UnusedScriptEntry> filtered)
        {
            IEnumerable<IGrouping<string, UnusedScriptEntry>> groups =
                filtered.GroupBy(entry => entry.Folder).OrderBy(group => group.Key);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            foreach (IGrouping<string, UnusedScriptEntry> group in groups)
                DrawGroup(group);

            EditorGUILayout.EndScrollView();
        }

        private void DrawGroup(IGrouping<string, UnusedScriptEntry> group)
        {
            string key = group.Key;

            if (!_foldouts.ContainsKey(key))
                _foldouts[key] = true;

            int count = group.Count();

            using (new EditorGUILayout.HorizontalScope(_groupStyle))
            {
                GUILayout.Label(EditorGUIUtility.IconContent("Folder Icon"), GUILayout.Width(18f),
                    GUILayout.Height(16f));

                _foldouts[key] = EditorGUILayout.Foldout(_foldouts[key], key, true);
                GUILayout.FlexibleSpace();
                GUILayout.Label(count.ToString(), _badgeStyle, GUILayout.Width(30f), GUILayout.Height(16f));
            }

            if (!_foldouts[key])
                return;

            foreach (UnusedScriptEntry entry in group)
                DrawRow(entry);
        }

        private void DrawRow(UnusedScriptEntry entry)
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

            GUI.Label(labelRect, new GUIContent(entry.Name, entry.Path), _pathStyle);

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

        private void Dismiss(UnusedScriptEntry entry)
        {
            UnusedScriptsDismissStore.Dismiss(entry.Guid);
            _discard = null;
            Repaint();
        }

        private void DismissAll(List<UnusedScriptEntry> entries)
        {
            if (entries.Count == 0)
                return;

            UnusedScriptsDismissStore.DismissRange(entries.Select(entry => entry.Guid));
            _discard = null;
            Repaint();
        }

        private void Restore(UnusedScriptEntry entry)
        {
            UnusedScriptsDismissStore.Restore(entry.Guid);
            _discard = null;
            Repaint();
        }

        private void RemoveEntry(UnusedScriptEntry entry)
        {
            if (!AssetDatabase.DeleteAsset(entry.Path))
                return;

            AssetDatabase.Refresh();
            _entries.Remove(entry);
            Repaint();
        }

        private void DeleteAll(List<UnusedScriptEntry> entries)
        {
            if (entries.Count == 0)
                return;

            bool confirmed = EditorUtility.DisplayDialog("Delete Dead Scripts",
                $"Delete {entries.Count} {Plural(entries.Count, "script", "scripts")}?\n\n"
                + "A script used only through reflection, code generation, or a build entry point "
                + "cannot be detected, so deleting it may break compilation. Review the list first.",
                "Delete",
                "Cancel");

            if (!confirmed)
                return;

            foreach (UnusedScriptEntry entry in entries)
                AssetDatabase.DeleteAsset(entry.Path);

            AssetDatabase.Refresh();
            Rescan();
        }

        private List<UnusedScriptEntry> ActiveEntries()
            => _entries.Where(entry => !UnusedScriptsDismissStore.IsDismissed(entry.Guid)).ToList();

        private List<UnusedScriptEntry> ApplySearch(List<UnusedScriptEntry> source)
        {
            if (string.IsNullOrWhiteSpace(_search))
                return source;

            string term = _search.Trim();

            return source
                .Where(entry => entry.Path.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
        }

        private List<UnusedScriptEntry> BuildDiscard()
        {
            List<UnusedScriptEntry> list = new();

            foreach (string guid in UnusedScriptsDismissStore.GetAll())
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (string.IsNullOrEmpty(path))
                    continue;

                list.Add(new UnusedScriptEntry(path, guid));
            }

            list.Sort((first, second) => string.Compare(first.Path, second.Path, StringComparison.Ordinal));
            return list;
        }

        private void Rescan()
        {
            _entries.Clear();
            _entries.AddRange(UnusedScriptScanner.Scan(_ignoreEditorScripts));
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

            // Calm blue for the dismissed list, so it reads as stored, not as a problem.
            _neutralBadgeTexture = MakeSolidTexture(new Color(0.33f, 0.52f, 0.74f));

            _neutralBadgeStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal =
                {
                    textColor = Color.white,
                    background = _neutralBadgeTexture
                }
            };

            Color successTitleColor = new(0.36f, 0.76f, 0.46f);
            Color successSubtitleColor = new(0.5f, 0.5f, 0.5f);

            _successTitleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = SuccessTitleFontSize,
                normal =
                {
                    textColor = successTitleColor
                },
                hover =
                {
                    textColor = successTitleColor
                }
            };

            _successSubtitleStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                normal =
                {
                    textColor = successSubtitleColor
                },
                hover =
                {
                    textColor = successSubtitleColor
                }
            };

            _successTexture = EditorGUIUtility.IconContent("TestPassed").image;

            _stylesReady = true;
        }
    }
}