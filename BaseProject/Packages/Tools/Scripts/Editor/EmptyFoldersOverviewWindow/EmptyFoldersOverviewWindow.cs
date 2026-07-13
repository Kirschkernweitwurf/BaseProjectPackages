using System;
using System.Collections.Generic;
using System.Linq;
using Base.EmptyFoldersPackage;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.ToolPackage.Editor.EmptyFoldersOverviewWindow
{
    /// <summary>
    /// Editor window that lists empty folders and lets you jump to or delete them.
    /// </summary>
    public sealed class EmptyFoldersOverviewWindow : EditorWindow
    {
        private const string MenuPath = "Tools/Base/Empty Folders Overview";
        private const float RowHeight = 22f;
        private const float SuccessGap = 8f;
        private const float SuccessIconSize = 48f;
        private const int SuccessTitleFontSize = 15;

        private readonly List<EmptyFolderEntry> _entries = new();

        private Vector2 _scroll;
        private string _search = string.Empty;
        private bool _hasScanned;
        private string _hoveredKey;
        private int _rowIndex;

        private GUIStyle _headerStyle;
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

            List<EmptyFolderEntry> filtered = _hasScanned
                ? FilterEntries()
                : new List<EmptyFolderEntry>();

            DrawActionBar(filtered);
            DrawSummary();

            if (!_hasScanned)
            {
                DrawHint("Press Scan to search the project for empty folders.");
                return;
            }

            if (_entries.Count == 0)
            {
                DrawSuccess("No empty folders", "Every folder has content. Nothing to clean up.");
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

        private static string Plural(int amount, string singular, string plural) => amount == 1
            ? singular
            : plural;

        private static GUIContent GetFolderIcon() => EditorGUIUtility.IconContent("Folder Icon");

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

        private void DrawActionBar(List<EmptyFolderEntry> filtered)
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
                    GUILayout.Label("No empty folders.", _headerStyle);
                }
                else
                {
                    string message = $"{folders} empty {Plural(folders, "folder", "folders")} found";

                    if (totalWithNested != folders)
                        message += $" ({totalWithNested} including nested)";

                    GUILayout.Label(message + ".", _headerStyle);
                }
            }
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

            Rect iconRect = new(rect.x + 4f, rect.y + 3f, 16f, 16f);
            GUI.Label(iconRect, GetFolderIcon());

            Rect body = new(rect.x + 24f, rect.y, rect.width - 24f, rect.height);
            Rect labelRect = new(body.x, body.y, body.width - 170f, body.height);
            Rect badgeRect = new(body.xMax - 160f, body.y + 3f, 34f, body.height - 6f);
            Rect gotoRect = new(body.xMax - 120f, body.y + 3f, 52f, body.height - 6f);
            Rect deleteRect = new(body.xMax - 64f, body.y + 3f, 64f, body.height - 6f);

            GUI.Label(labelRect, new GUIContent(entry.Path, entry.Path), _pathStyle);

            if (entry.NestedFolderCount > 1)
                GUI.Label(badgeRect,
                    new GUIContent(entry.NestedFolderCount.ToString(),
                        $"Removes {entry.NestedFolderCount} folders including nested empties."),
                    _badgeStyle);

            if (GUI.Button(gotoRect, "Go to"))
                Navigate(entry);

            if (GUI.Button(deleteRect, "Delete"))
                DeleteEntry(entry);

            if (Event.current.type == EventType.MouseDown && labelRect.Contains(Event.current.mousePosition))
            {
                Navigate(entry);
                Event.current.Use();
            }
        }

        private void DeleteEntry(EmptyFolderEntry entry)
        {
            if (!AssetDatabase.DeleteAsset(entry.Path))
                return;

            AssetDatabase.Refresh();
            _entries.Remove(entry);
            Repaint();
        }

        private void DeleteAll(List<EmptyFolderEntry> entries)
        {
            if (entries.Count == 0)
                return;

            bool confirmed = EditorUtility.DisplayDialog("Delete Empty Folders",
                $"Delete {entries.Count} empty {Plural(entries.Count, "folder", "folders")}?",
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
                return _entries;

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