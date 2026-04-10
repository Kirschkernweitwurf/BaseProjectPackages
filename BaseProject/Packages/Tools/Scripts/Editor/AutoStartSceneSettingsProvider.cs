using UnityEditor;
using UnityEngine;

namespace Editor
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

            if (_startScene)
                EditorGUILayout.HelpBox($"Current Start Scene: {AssetDatabase.GetAssetPath(_startScene)}", 
                    MessageType.Info);
            else
                EditorGUILayout.HelpBox("No start scene set.", MessageType.Warning);

            EditorGUI.EndDisabledGroup();
        }

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new AutoStartSceneSettingsProvider("Project/Custom Tools/Auto Start Scene");
        }
    }
}