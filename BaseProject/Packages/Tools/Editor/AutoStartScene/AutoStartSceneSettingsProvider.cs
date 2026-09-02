using Base.EditorUIPackage.Editor;
using Base.ToolsPackage.Editor.BaseToolsOverview;
using UnityEditor;

namespace Base.ToolsPackage.Editor.AutoStartScene
{
    /// <summary>
    /// Provides a settings provider in Unity's Project Settings to configure the auto start scene.
    /// Allows users to select a scene that will automatically load when entering Play mode.
    /// </summary>
    internal class AutoStartSceneSettingsProvider : SettingsProvider
    {
        private const string DefaultSceneFormat = "Using the first build scene as the default: {0}";
        private const string EnableLabel = "Enable Auto Start";
        private const string ExplicitSceneFormat = "Current start scene: {0}";
        private const string Intro = "Select a scene to automatically load when entering play mode, whichever "
            + "scene happens to be open.";
        private const string MissingSceneMessage = "No start scene available. Add a scene to Build Settings or "
            + "set one manually.";
        private const string SceneLabel = "Start Scene";
        private const string SettingsPath = "Project/Base Tools/Auto Start Scene";
        private const string Summary = "The scene that loads when entering play mode, whichever scene is open.";

        private readonly EditorWindowStyles _styles = new();

        private SceneAsset _startScene;

        private AutoStartSceneSettingsProvider(string path, SettingsScope scope = SettingsScope.Project)
            : base(path, scope) { }

        /// <summary>Registers the page with the project settings window. Called by Unity.</summary>
        /// <returns>The provider Unity draws the page from.</returns>
        [SettingsProvider]
        [BaseToolsPage(Summary)]
        public static SettingsProvider CreateSettingsProvider() => new AutoStartSceneSettingsProvider(SettingsPath);

        /// <inheritdoc/>
        public override void OnGUI(string searchContext)
        {
            _styles.EnsureBuilt();

            EditorWindowChrome.DrawIntro(_styles, Intro);

            bool isEnabled = DrawEnabledToggle();

            EditorGUI.BeginDisabledGroup(!isEnabled);

            DrawScenePicker();

            EditorGUILayout.Space(EditorMetrics.TightGap);

            DrawSceneState();

            EditorGUI.EndDisabledGroup();
        }

        /// <inheritdoc/>
        public override void OnDeactivate() => _styles.Dispose();

        private static bool DrawEnabledToggle()
        {
            EditorGUI.BeginChangeCheck();

            bool isEnabled = EditorGUILayout.Toggle(EnableLabel, AutoStartSceneSettings.IsEnabled());

            if (EditorGUI.EndChangeCheck())
                AutoStartSceneSettings.SetEnabled(isEnabled);

            return isEnabled;
        }

        private void DrawScenePicker()
        {
            EditorGUI.BeginChangeCheck();

            _startScene = EditorGUILayout.ObjectField(SceneLabel, AutoStartSceneSettings.GetStartScene(),
                typeof(SceneAsset), false) as SceneAsset;

            if (EditorGUI.EndChangeCheck())
                AutoStartSceneSettings.SetStartScene(_startScene);
        }

        private void DrawSceneState()
        {
            if (_startScene == null)
            {
                EditorGUILayout.HelpBox(MissingSceneMessage, MessageType.Warning);
                return;
            }

            string path = AssetDatabase.GetAssetPath(_startScene);
            string format = AutoStartSceneSettings.HasExplicitStartScene()
                ? ExplicitSceneFormat
                : DefaultSceneFormat;

            EditorGUILayout.HelpBox(string.Format(format, path), MessageType.Info);
        }
    }
}