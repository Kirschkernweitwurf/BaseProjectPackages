#if UNITY_EDITOR
using Base.ToolPackage.Editor.AssetZoo.Runtime.Builder;
using Base.ToolPackage.Editor.AssetZoo.Runtime.Config;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.AssetZoo.Editor
{
    /// <summary>
    /// Dockable window for quick zoo building. Tools &gt; Asset Zoo &gt; Open Zoo Builder.
    /// </summary>
    public class ZooEditorWindow : EditorWindow
    {
        private const float MainButtonHeight = 32f;
        private const float AuxButtonHeight = 24f;
        private const float MinWindowWidth = 340f;
        private const float MinWindowHeight = 400f;

        private ZooConfig _config;
        private Transform _parent;
        private readonly ZooBuilder _builder = new();
        private Vector2 _scroll;
        private UnityEditor.Editor _cachedConfigEditor;

        private void OnDisable() => ClearCachedEditor();

        /// <summary>
        /// Opens the zoo builder window without a config.
        /// </summary>
        [MenuItem("Tools/Asset Zoo/Open Zoo Builder")]
        public static void Open() => Open(null);

        public static void Open(ZooConfig config)
        {
            ZooEditorWindow window = GetWindow<ZooEditorWindow>("Asset Zoo");
            window.minSize = new Vector2(MinWindowWidth, MinWindowHeight);

            if (config != null)
                window._config = config;
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Asset Zoo Builder", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            EditorGUI.BeginChangeCheck();
            _config = (ZooConfig)EditorGUILayout.ObjectField(
                "Config", _config, typeof(ZooConfig), false);
            _parent = (Transform)EditorGUILayout.ObjectField(
                "Parent (optional)", _parent, typeof(Transform), true);
            if (EditorGUI.EndChangeCheck()) ClearCachedEditor();

            EditorGUILayout.Space(6);

            bool hasZoo = _builder.HasZoo;
            bool hasParent = _parent != null;

            using (new EditorGUI.DisabledScope(_config == null))
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Build Zoo", GUILayout.Height(MainButtonHeight)))
                    _builder.Build(_config, _parent);

                using (new EditorGUI.DisabledScope(!hasZoo))
                {
                    if (GUILayout.Button("Clear Zoo", GUILayout.Height(MainButtonHeight)))
                        _builder.Clear();
                }
            }

            using (new EditorGUI.DisabledScope(!hasZoo))
            {
                if (GUILayout.Button("Select Zoo Root", GUILayout.Height(AuxButtonHeight)))
                    SelectZooRoot();
            }

            using (new EditorGUI.DisabledScope(!hasParent))
            {
                if (GUILayout.Button("Select Zoo Parent", GUILayout.Height(AuxButtonHeight)))
                    SelectZooParent();
            }

            EditorGUILayout.Space(6);

            if (_config == null)
            {
                EditorGUILayout.HelpBox(
                    "1. Create a config via Assets > Create > Asset Zoo > Zoo Config.\n" +
                    "2. Drop it in the Config field above.\n" +
                    "3. Add prefabs, hit Build.",
                    MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField("Config", EditorStyles.boldLabel);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            EnsureCachedEditor();
            _cachedConfigEditor.OnInspectorGUI();

            EditorGUILayout.EndScrollView();
        }

        private void EnsureCachedEditor()
        {
            // Rebuild the cached inspector, if it was never created,
            // its underlying target object was destroyed (asset deleted),
            // it's pointing at a different config than the one currently selected
            if (_cachedConfigEditor != null
                && _cachedConfigEditor.target != null
                && _cachedConfigEditor.target == _config)
                return;

            ClearCachedEditor();
            _cachedConfigEditor = UnityEditor.Editor.CreateEditor(_config);
        }

        private void ClearCachedEditor()
        {
            if (_cachedConfigEditor != null)
                DestroyImmediate(_cachedConfigEditor);

            _cachedConfigEditor = null;
        }

        private void SelectZooRoot()
        {
            GameObject root = _builder.GetZooRoot();
            if (root == null)
                return;

            Selection.activeGameObject = root;
            EditorGUIUtility.PingObject(root);
        }

        private void SelectZooParent()
        {
            if (_parent == null)
                return;

            Selection.activeGameObject = _parent.gameObject;
            EditorGUIUtility.PingObject(_parent);
        }
    }
}
#endif