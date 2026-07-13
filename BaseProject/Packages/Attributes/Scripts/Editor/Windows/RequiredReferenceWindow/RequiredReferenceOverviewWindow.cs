using System.Collections.Generic;
using Base.UtilityPackage.Generated;
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
        private const float ListSpacing = 4f;
        private const string MenuPath = "Tools/Base Packages/Unity Editor/Required References";
        private const double MinScanInterval = 0.3;
        private const double SafetyPollInterval = 1.0;
        private const float SearchWidth = 180f;
        private const string WindowTitle = "Required References";

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
            titleContent = new GUIContent(WindowTitle);

            _assetGroups ??= new List<RequiredReferenceGroup>();
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

            DrawToolbar();

            if (_total == 0)
            {
                RequiredReferenceView.DrawSuccess(_styles);
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Space(ListSpacing);

            Object clicked =
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

        [MenuItem(MenuPath, priority = MenuOrders.UnityEditor)]
        private static void Open()
        {
            RequiredReferenceOverviewWindow window = GetWindow<RequiredReferenceOverviewWindow>();

            window.minSize = new Vector2(320f, 200f);
            window.Show();
        }

        private static void Focus(Object owner)
        {
            if (owner == null)
                return;

            Selection.activeObject = owner;
            EditorGUIUtility.PingObject(owner);
        }

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
                _assetsDirty = true;
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

            if (_assetsDirty)
            {
                _assetGroups = RequiredReferenceCollector.CollectAssets(out _assetTotal);
                _assetsDirty = false;
            }

            List<RequiredReferenceGroup> scene = RequiredReferenceCollector.CollectScene(out int sceneTotal);

            _groups = new List<RequiredReferenceGroup>(scene);
            _groups.AddRange(_assetGroups ?? new List<RequiredReferenceGroup>());
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