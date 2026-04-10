#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
namespace Attributes.Editor
{
    /// <summary>
    /// Property drawer for <see cref="SceneNameAttribute"/>.
    /// Displays a dropdown of all scenes included in the Build Settings.
    /// </summary>
    [CustomPropertyDrawer(typeof(SceneNameAttribute))]
    public class SceneNameAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "Use [SceneName] with string.");
                return;
            }

            // Get all scene names from Build Settings
            string[] scenePaths = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);
            string[] sceneNames = new string[scenePaths.Length];
            for (int i = 0; i < scenePaths.Length; i++)
            {
                string path = scenePaths[i];
                string name = System.IO.Path.GetFileNameWithoutExtension(path);
                sceneNames[i] = name;
            }

            // Find current index
            int currentIndex = Mathf.Max(0, System.Array.IndexOf(sceneNames, property.stringValue));
            int selectedIndex = EditorGUI.Popup(position, label.text, currentIndex, sceneNames);

            // Update string value
            if (selectedIndex >= 0 && selectedIndex < sceneNames.Length)
                property.stringValue = sceneNames[selectedIndex];
        }
    }
}
#endif