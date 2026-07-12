using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.RequiredReferenceWindow
{
    /// <summary>
    /// Editor window that lists missing required references in the open scenes and refreshes live.
    /// </summary>
    public sealed class RequiredReferenceOverviewWindow : EditorWindow
    {
        private const float ListSpacing = 4f;
        private const string MenuPath = "Tools/Base Packages/Required References";
        private const double MinScanInterval = 0.3;
        private const double SafetyPollInterval = 1.0;
        private const float SearchWidth = 180f;
        private const string WindowTitle = "Required References";

        [SerializeField] private Vector2 scrollPosition;
        [SerializeField] private string search = string.Empty;

        private readonly RequiredReferenceStyles _styles = new();

        private List<RequiredReferenceGroup> _groups = new();

        private int _total;
        private bool _dirty;
        private double _lastScan;

#region Unity Callbacks
        private void OnEnable()
        {
            titleContent = new GUIContent(WindowTitle);

            EditorApplication.hierarchyChanged += MarkDirty;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            ObjectChangeEvents.changesPublished += OnObjectChanged;

            Rescan();
        }

        private void OnGUI()
        {
            _styles.EnsureBuilt();

            DrawToolbar();

            if (_total == 0)
            {
                RequiredReferenceView.DrawSuccess(_styles);
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Space(ListSpacing);

            GameObject clicked =
                RequiredReferenceView.DrawGroups(_groups, search, _styles, out bool anyShown);

            if (!anyShown)
                EditorGUILayout.LabelField($"No matches for \"{search}\".",
                    EditorStyles.centeredGreyMiniLabel);

            GUILayout.Space(ListSpacing);

            EditorGUILayout.EndScrollView();

            if (clicked != null)
                Focus(clicked);
        }

        private void OnDisable()
        {
            EditorApplication.hierarchyChanged -= MarkDirty;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            ObjectChangeEvents.changesPublished -= OnObjectChanged;
        }

        private void OnFocus() => MarkDirty();

        private void OnInspectorUpdate()
        {
            double elapsed = EditorApplication.timeSinceStartup - _lastScan;

            if (elapsed < MinScanInterval)
                return;

            if (_dirty || elapsed >= SafetyPollInterval)
                Rescan();
        }
#endregion

        [MenuItem(MenuPath)]
        private static void Open()
        {
            RequiredReferenceOverviewWindow window =
                GetWindow<RequiredReferenceOverviewWindow>();

            window.minSize = new Vector2(320f, 200f);
            window.Show();
        }

        private static void Focus(GameObject owner)
        {
            if (owner == null)
                return;

            Selection.activeGameObject = owner;
            EditorGUIUtility.PingObject(owner);
        }

        private void MarkDirty() => _dirty = true;

        private void OnPlayModeChanged(PlayModeStateChange _) => _dirty = true;

        private void OnObjectChanged(ref ObjectChangeEventStream _) => _dirty = true;

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            DrawStatus();

            GUILayout.FlexibleSpace();

            search = GUILayout.TextField(search,
                EditorStyles.toolbarSearchField,
                GUILayout.Width(SearchWidth));

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
            {
                Rescan();
                GUIUtility.ExitGUI();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawStatus()
        {
            bool ok = _total == 0;

            Texture icon = ok
                ? _styles.SuccessTexture
                : _styles.ErrorTexture;

            GUILayout.Label(new GUIContent(ok
                        ? "All references assigned"
                        : $"{_total} missing references",
                    icon),
                EditorStyles.label,
                GUILayout.Height(18));
        }

        private void Rescan()
        {
            _dirty = false;
            _lastScan = EditorApplication.timeSinceStartup;

            _groups = RequiredReferenceCollector.Collect(out _total);

            Repaint();
        }
    }
}