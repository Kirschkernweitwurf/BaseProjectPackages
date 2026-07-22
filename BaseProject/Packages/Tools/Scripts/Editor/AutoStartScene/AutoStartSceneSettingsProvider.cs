using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.AutoStartScene
{
    /// <summary>
    /// Provides a settings provider in Unity's Project Settings to configure the auto start scene.
    /// Allows users to select a scene that will automatically load when entering Play mode.
    /// </summary>
    public class AutoStartSceneSettingsProvider : SettingsProvider
    {
        private SceneAsset _startScene;

        private AutoStartSceneSettingsProvider(string path, SettingsScope scope = SettingsScope.Project)
            : base(path, scope) { }

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
            => new AutoStartSceneSettingsProvider("Project/Custom Tools/Auto Start Scene");

        public override void OnGUI(string searchContext)
        {
            GUILayout.Label("Auto Start Scene", EditorStyles.boldLabel);
            GUILayout.Label("Select a scene to automatically load when entering Play mode.",
                EditorStyles.wordWrappedLabel);

            GUILayout.Space(10);

            // Toggle
            EditorGUI.BeginChangeCheck();
            bool enabled = EditorGUILayout.Toggle("Enable Auto Start", AutoStartSceneSettings.IsEnabled());
            if (EditorGUI.EndChangeCheck())
                AutoStartSceneSettings.SetEnabled(enabled);

            EditorGUI.BeginDisabledGroup(!enabled);

            EditorGUI.BeginChangeCheck();
            _startScene = (SceneAsset)EditorGUILayout.ObjectField("Start Scene",
                AutoStartSceneSettings.GetStartScene(), typeof(SceneAsset), false);

            if (EditorGUI.EndChangeCheck())
                AutoStartSceneSettings.SetStartScene(_startScene);

            GUILayout.Space(4);

            if (_startScene == null)
                EditorGUILayout.HelpBox("No start scene available."
                    + " Add a scene to Build Settings or set one manually.", MessageType.Warning);
            else if (AutoStartSceneSettings.HasExplicitStartScene())
                EditorGUILayout.HelpBox($"Current Start Scene: {AssetDatabase.GetAssetPath(_startScene)}",
                    MessageType.Info);
            else
                EditorGUILayout.HelpBox(
                    $"Using first build scene as default: {AssetDatabase.GetAssetPath(_startScene)}",
                    MessageType.Info);

            EditorGUI.EndDisabledGroup();
        }
    }
}