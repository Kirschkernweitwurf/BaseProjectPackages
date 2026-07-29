using System.Collections.Generic;
using Base.ToolPackage.MenuManagerWindow;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor.Windows.RequiredReferenceWindow
{
    /// <summary>
    /// Editor window that lists validation issues in the open scenes and on ScriptableObject assets, and
    /// refreshes live. Scene issues rescan often; asset issues are cached and refreshed on project change.
    /// </summary>
    public sealed class RequiredReferenceOverviewWindow : EditorWindow
    {
        private const float BarSpacing = 4f;
        private const float ButtonHeight = 26f;
        private const float ButtonWidth = 140f;
        private const float ListSpacing = 4f;
        private const float MinimumHeight = 200f;
        private const float MinimumWidth = 320f;
        private const double MinScanInterval = 0.3;
        private const double SafetyPollInterval = 1.0;
        private const float SearchHeight = 20f;
        private const float SearchWidth = 200f;

        [SerializeField] private Vector2 scrollPosition;
        [SerializeField] private string search = string.Empty;

        private readonly RequiredReferenceStyles _styles = new();

        private List<RequiredReferenceGroup> _groups = new();
        private List<RequiredReferenceGroup> _assetGroups = new();

        private int _total;
        private int _assetTotal;
        private bool _dirty;
        private bool _assetsDirty = true;
        private double _lastScan;

#region Unity Callbacks
        private void OnEnable()
        {
            titleContent = new GUIContent(ReferenceWindowInfo.WindowTitle);

            _assetsDirty = true;

            EditorApplication.hierarchyChanged += MarkDirty;
            EditorApplication.projectChanged += MarkAssetsDirty;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            ObjectChangeEvents.changesPublished += OnObjectChanged;
            EditorApplication.delayCall += DeferredAssetScan;

            Rescan();
        }

        private void OnGUI()
        {
            _styles.EnsureBuilt();

            DrawActionBar();
            DrawSummary();

            if (_total == 0)
            {
                RequiredReferenceView.DrawSuccess(_styles);
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Space(ListSpacing);

            Object clicked = RequiredReferenceView.DrawGroups(_groups, search, _styles, out bool anyShown);

            if (!anyShown)
                EditorGUILayout.LabelField($"No matches for \"{search}\".", EditorStyles.centeredGreyMiniLabel);

            GUILayout.Space(ListSpacing);

            EditorGUILayout.EndScrollView();

            if (clicked != null)
                Focus(clicked);
        }

        private void OnDisable()
        {
            EditorApplication.hierarchyChanged -= MarkDirty;
            EditorApplication.projectChanged -= MarkAssetsDirty;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            ObjectChangeEvents.changesPublished -= OnObjectChanged;
            EditorApplication.delayCall -= DeferredAssetScan;
        }

        private void OnFocus() => MarkDirty();

        private void OnInspectorUpdate()
        {
            double elapsed = EditorApplication.timeSinceStartup - _lastScan;

            if (elapsed < MinScanInterval)
                return;

            if (_dirty || _assetsDirty || elapsed >= SafetyPollInterval)
                Rescan();
        }
#endregion

        [DynamicMenuItem(ReferenceWindowInfo.MenuPath)]
        private static void Open()
        {
            RequiredReferenceOverviewWindow window = GetWindow<RequiredReferenceOverviewWindow>();

            window.minSize = new Vector2(MinimumWidth, MinimumHeight);
            window.Show();
        }

        private static void Focus(Object owner)
        {
            if (owner == null)
                return;

            Selection.activeObject = owner;
            EditorGUIUtility.PingObject(owner);
        }

        private void DrawActionBar()
        {
            EditorGUILayout.Space(BarSpacing);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.Space(BarSpacing, false);

            if (GUILayout.Button("Refresh", GUILayout.Height(ButtonHeight), GUILayout.Width(ButtonWidth)))
            {
                _assetsDirty = true;
                Rescan();
                GUIUtility.ExitGUI();
            }

            GUILayout.FlexibleSpace();

            search = EditorGUILayout.TextField(search,
                EditorStyles.toolbarSearchField,
                GUILayout.Width(SearchWidth),
                GUILayout.Height(SearchHeight));

            EditorGUILayout.Space(BarSpacing, false);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(BarSpacing);
        }

        private void DrawSummary()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            string message = _total == 0
                ? "No missing references."
                : $"{_total} missing {(_total == 1 ? "reference" : "references")}.";

            GUILayout.Label(message, _styles.Summary);

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
        }

        private void Rescan()
        {
            _dirty = false;
            _lastScan = EditorApplication.timeSinceStartup;

            if (_assetsDirty)
            {
                _assetGroups = RequiredReferenceCollector.CollectAssets(out _assetTotal);
                _assetsDirty = false;
            }

            List<RequiredReferenceGroup> scene = RequiredReferenceCollector.CollectScene(out int sceneTotal);

            _groups = new List<RequiredReferenceGroup>(scene);
            _groups.AddRange(_assetGroups);
            _total = sceneTotal + _assetTotal;

            Repaint();
        }

        private void MarkDirty() => _dirty = true;

        private void MarkAssetsDirty() => _assetsDirty = true;

        private void DeferredAssetScan()
        {
            if (this == null)
                return;

            _assetsDirty = true;
            Rescan();
        }

        private void OnPlayModeChanged(PlayModeStateChange change) => _dirty = true;

        private void OnObjectChanged(ref ObjectChangeEventStream stream) => _dirty = true;
    }
}