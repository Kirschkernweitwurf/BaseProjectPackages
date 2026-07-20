using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Base.ToolPackage.MenuManagerWindow;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.MissingScriptsOverviewWindow
{
    /// <summary>
    /// Editor window that lists every missing script in the project and jumps to it on click.
    /// </summary>
    public sealed class MissingScriptsOverviewWindow : EditorWindow
    {
        private const string MenuPath =
            "Tools/Base Packages/Unity Editor/Project Health/Unused/Missing Scripts Overview";
        private const float RowHeight = 22f;
        private const float SuccessGap = 8f;
        private const float SuccessIconSize = 48f;
        private const int SuccessTitleFontSize = 15;

        private readonly List<MissingScriptEntry> _entries = new();
        private readonly Dictionary<string, bool> _foldouts = new();

        private Vector2 _scroll;
        private string _search = string.Empty;
        private bool _scanScenes = true;
        private bool _scanPrefabs = true;
        private bool _scanScriptableObjects = true;
        private bool _hasScanned;
        private string _hoveredKey;
        private int _rowIndex;

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

            DrawActionBar();
            DrawFilters();
            DrawSummary();

            if (!_hasScanned)
            {
                DrawHint("Press Scan to search the project for missing scripts.");
                return;
            }

            if (_entries.Count == 0)
            {
                DrawSuccess("No missing scripts", "Every script reference is intact. Nothing to fix.");
                return;
            }

            List<MissingScriptEntry> filtered = FilterEntries();

            if (filtered.Count == 0)
            {
                DrawHint("No results match the search.");
                return;
            }

            DrawResults(filtered);
        }
#endregion

        [DynamicMenuItem(MenuPath)]
        private static void Open()
        {
            MissingScriptsOverviewWindow window = GetWindow<MissingScriptsOverviewWindow>();
            window.titleContent = new GUIContent("Missing Scripts");
            window.minSize = new Vector2(460f, 320f);
            window.Show();
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

        private void DrawActionBar()
        {
            EditorGUILayout.Space(4f);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.Space(4f, false);

                if (GUILayout.Button("Scan Project", GUILayout.Height(26f), GUILayout.Width(150f)))
                    Rescan();

                GUILayout.FlexibleSpace();

                _search = EditorGUILayout.TextField(_search, EditorStyles.toolbarSearchField,
                    GUILayout.Width(200f), GUILayout.Height(20f));

                EditorGUILayout.Space(4f, false);
            }

            EditorGUILayout.Space(4f);
        }

        private void DrawFilters()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Include:", GUILayout.Width(54f));

                GUIContent scenesContent = new(" Scenes",
                    "Scans every scene in the project. Each scene is opened for scanning, so you are asked to save first.");

                _scanScenes = GUILayout.Toggle(_scanScenes, scenesContent, GUILayout.Width(74f));
                _scanPrefabs = GUILayout.Toggle(_scanPrefabs, " Prefabs", GUILayout.Width(78f));
                _scanScriptableObjects = GUILayout.Toggle(_scanScriptableObjects, " Assets", GUILayout.Width(72f));

                GUILayout.FlexibleSpace();
            }
        }

        private void DrawSummary()
        {
            if (!_hasScanned)
                return;

            int total = _entries.Sum(entry => entry.MissingCount);
            int objects = _entries.Count;

            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                string message = objects == 0
                    ? "No missing scripts."
                    : $"{total} missing {Plural(total, "script", "scripts")} "
                    + $"on {objects} {Plural(objects, "object", "objects")}.";

                GUILayout.Label(message, _headerStyle);
            }
        }

        private void DrawResults(List<MissingScriptEntry> filtered)
        {
            _hoveredKey = null;
            _rowIndex = 0;

            IEnumerable<IGrouping<string, MissingScriptEntry>> groups =
                filtered.GroupBy(entry => entry.AssetPath).OrderBy(group => group.Key);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            foreach (IGrouping<string, MissingScriptEntry> group in groups)
                DrawGroup(group);

            EditorGUILayout.EndScrollView();
        }

        private void DrawGroup(IGrouping<string, MissingScriptEntry> group)
        {
            string key = group.Key;

            if (!_foldouts.ContainsKey(key))
                _foldouts[key] = true;

            EMissingScriptSource source = group.First().Source;
            int groupTotal = group.Sum(entry => entry.MissingCount);

            using (new EditorGUILayout.HorizontalScope(_groupStyle))
            {
                GUILayout.Label(GetSourceIcon(source), GUILayout.Width(18f), GUILayout.Height(16f));
                _foldouts[key] = EditorGUILayout.Foldout(_foldouts[key], Path.GetFileName(key), true);
                GUILayout.FlexibleSpace();
                GUILayout.Label(groupTotal.ToString(), _badgeStyle, GUILayout.Width(30f), GUILayout.Height(16f));
            }

            if (!_foldouts[key])
                return;

            foreach (MissingScriptEntry entry in group)
                DrawRow(entry);
        }

        private void DrawRow(MissingScriptEntry entry)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, RowHeight);
            string key = entry.AssetPath + "|" + entry.DisplayPath;
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

            Rect body = new(rect.x + 22f, rect.y, rect.width - 22f, rect.height);
            Rect labelRect = new(body.x, body.y, body.width - 170f, body.height);
            Rect badgeRect = new(body.xMax - 160f, body.y + 3f, 34f, body.height - 6f);
            Rect gotoRect = new(body.xMax - 120f, body.y + 3f, 52f, body.height - 6f);
            Rect removeRect = new(body.xMax - 64f, body.y + 3f, 64f, body.height - 6f);

            GUI.Label(labelRect, new GUIContent(entry.DisplayPath, entry.AssetPath), _pathStyle);
            GUI.Label(badgeRect, entry.MissingCount.ToString(), _badgeStyle);

            if (GUI.Button(gotoRect, "Go to"))
                MissingScriptNavigator.Navigate(entry);

            using (new EditorGUI.DisabledScope(entry.Source == EMissingScriptSource.ScriptableObject))
            {
                if (GUI.Button(removeRect, "Remove"))
                    RemoveEntry(entry);
            }

            if (Event.current.type == EventType.MouseDown && labelRect.Contains(Event.current.mousePosition))
            {
                MissingScriptNavigator.Navigate(entry);
                Event.current.Use();
            }
        }

        private void RemoveEntry(MissingScriptEntry entry)
        {
            MissingScriptNavigator.Navigate(entry);

            GameObject target = Selection.activeGameObject;

            if (target == null)
                return;

            int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(target);

            if (removed <= 0)
                return;

            EditorUtility.SetDirty(target);
            _entries.Remove(entry);
            Repaint();
        }

        private List<MissingScriptEntry> FilterEntries()
        {
            if (string.IsNullOrWhiteSpace(_search))
                return _entries;

            string term = _search.Trim();

            return _entries.Where(entry =>
                    entry.DisplayPath.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0
                    || entry.AssetPath.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
        }

        private void Rescan()
        {
            _entries.Clear();

            // Scenes are always scanned across the whole project when enabled.
            _entries.AddRange(MissingScriptScanner.Scan(_scanScenes,
                true,
                _scanPrefabs,
                _scanScriptableObjects));

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

        private GUIContent GetSourceIcon(EMissingScriptSource source)
        {
            switch (source)
            {
                case EMissingScriptSource.Scene:
                    return EditorGUIUtility.IconContent("SceneAsset Icon");

                case EMissingScriptSource.Prefab:
                    return EditorGUIUtility.IconContent("Prefab Icon");

                default:
                    return EditorGUIUtility.IconContent("ScriptableObject Icon");
            }
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