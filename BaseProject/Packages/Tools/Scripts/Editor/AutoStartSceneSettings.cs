using UnityEditor;
using UnityEditor.SceneManagement;
using Utility.Logging;

namespace Editor
{
    /// <summary>
    /// Automatically sets a specified scene to load when entering Play mode in the Unity Editor.
    /// The previous scene is restored when exiting Play mode.
    /// Settings are stored using EditorPrefs.
    /// </summary>
    [InitializeOnLoad]
    public static class AutoStartSceneSettings
    {
        private const string SceneKey = "AutoStartScenePath";
        private const string EnabledKey = "AutoStartSceneEnabled";

        static AutoStartSceneSettings() => EditorApplication.playModeStateChanged += OnPlayModeChanged;

        /// <summary>
        /// Sets the scene to be loaded when entering Play mode.
        /// If null is passed, the auto start scene feature is disabled.
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
        /// Gets the currently set start scene.
        /// Returns null if no scene is set.
        /// </summary>
        public static SceneAsset GetStartScene()
        {
            string path = EditorPrefs.GetString(SceneKey, string.Empty);
            return string.IsNullOrEmpty(path)
                ? null
                : AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
        }

        /// <summary>
        /// Enables or disables the auto start scene feature.
        /// </summary>
        /// <param name="enabled"><c>true</c> to enable, <c>false</c> to disable.</param>
        public static void SetEnabled(bool enabled) => EditorPrefs.SetBool(EnabledKey, enabled);

        /// <summary>
        /// Checks if the auto start scene feature is enabled.
        /// </summary>
        public static bool IsEnabled() => EditorPrefs.GetBool(EnabledKey, true);

        /// <summary>
        /// Called when Unity’s play mode state changes.
        /// If enabled, it assigns the chosen start scene to load automatically when Play starts.
        /// </summary>
        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode || !IsEnabled())
                return;

            string scenePath = EditorPrefs.GetString(SceneKey, string.Empty);
            if (string.IsNullOrEmpty(scenePath))
                return;

            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            if (sceneAsset != null)
                EditorSceneManager.playModeStartScene = sceneAsset;
            else
                CustomLogger.LogWarning($"Auto Start Scene not found at path: {scenePath}", null);
        }
    }
}