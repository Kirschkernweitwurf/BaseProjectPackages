using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Base.ToolPackage.Editor.AutoStartScene
{
    /// <summary>
    /// Automatically sets a specified scene to load when entering Play mode in the Unity Editor.
    /// Defaults to the first enabled scene in Build Settings when no scene is explicitly set.
    /// The previous scene is restored when exiting Play mode.
    /// Settings are stored using EditorPrefs.
    /// </summary>
    [InitializeOnLoad]
    public static class AutoStartSceneSettings
    {
        private const string EnabledKey = "AutoStartSceneEnabled";
        private const string SceneKey = "AutoStartScenePath";

        static AutoStartSceneSettings() => EditorApplication.playModeStateChanged += OnPlayModeChanged;

        /// <summary>
        /// Sets the scene to be loaded when entering Play mode.
        /// If null is passed, the setting is cleared and the first build scene is used as fallback.
        /// </summary>
        public static void SetStartScene(SceneAsset scene)
        {
            if (scene == null)
            {
                EditorPrefs.DeleteKey(SceneKey);
                return;
            }

            string path = AssetDatabase.GetAssetPath(scene);
            EditorPrefs.SetString(SceneKey, path);
        }

        /// <summary>
        /// Gets the currently set start scene, falling back to the first enabled build scene.
        /// Returns null if neither is available.
        /// </summary>
        public static SceneAsset GetStartScene()
        {
            string path = ResolveScenePath();
            return string.IsNullOrEmpty(path)
                ? null
                : AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
        }

        /// <summary>
        /// Checks whether an explicit start scene has been set by the user.
        /// </summary>
        public static bool HasExplicitStartScene()
            => !string.IsNullOrEmpty(EditorPrefs.GetString(SceneKey, string.Empty));

        /// <summary>
        /// Enables or disables the auto start scene feature.
        /// </summary>
        /// <param name="enabled"><c>true</c> to enable, <c>false</c> to disable.</param>
        public static void SetEnabled(bool enabled)
        {
            EditorPrefs.SetBool(EnabledKey, enabled);

            if (!enabled)
                EditorSceneManager.playModeStartScene = null;
        }

        /// <summary>
        /// Checks if the auto start scene feature is enabled. Enabled by default.
        /// </summary>
        public static bool IsEnabled() => EditorPrefs.GetBool(EnabledKey, true);

        /// <summary>
        /// Resolves the effective start scene path: the explicitly set scene if present,
        /// otherwise the first enabled scene in Build Settings.
        /// </summary>
        private static string ResolveScenePath()
        {
            string path = EditorPrefs.GetString(SceneKey, string.Empty);
            if (!string.IsNullOrEmpty(path))
                return path;

            foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
            {
                if (buildScene.enabled)
                    return buildScene.path;
            }

            return string.Empty;
        }

        /// <summary>
        /// Called when Unity's play mode state changes.
        /// If enabled, it assigns the resolved start scene to load automatically when Play starts.
        /// If disabled, it clears any previously assigned start scene.
        /// </summary>
        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode)
                return;

            if (!IsEnabled())
            {
                EditorSceneManager.playModeStartScene = null;
                return;
            }

            string scenePath = ResolveScenePath();
            if (string.IsNullOrEmpty(scenePath))
            {
                EditorSceneManager.playModeStartScene = null;
                return;
            }

            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            if (sceneAsset != null)
                EditorSceneManager.playModeStartScene = sceneAsset;
            else
                CustomLogger.LogWarning($"Auto Start Scene not found at path: {scenePath}", null);
        }
    }
}