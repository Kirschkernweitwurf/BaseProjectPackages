using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Property drawer for <see cref="SceneNameAttribute"/>.
    /// Shows a dropdown of the scenes in the Build Settings. On a string field it stores the scene
    /// name, on an int field it stores the build index. Dropdown labels include the build index.
    /// </summary>
    [CustomPropertyDrawer(typeof(SceneNameAttribute))]
    public sealed class SceneNameAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            string[] scenePaths = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);

            if (scenePaths.Length == 0)
            {
                EditorGUI.LabelField(position, label.text, "No scenes in Build Settings.");
                return;
            }

            string[] sceneNames = new string[scenePaths.Length];
            string[] displayNames = new string[scenePaths.Length];
            for (int i = 0; i < scenePaths.Length; i++)
            {
                sceneNames[i] = Path.GetFileNameWithoutExtension(scenePaths[i]);
                displayNames[i] = i + ": " + sceneNames[i];
            }

            EditorGUI.BeginProperty(position, label, property);

            if (property.propertyType == SerializedPropertyType.String)
            {
                int current = Mathf.Max(0, Array.IndexOf(sceneNames, property.stringValue));
                int selected = EditorGUI.Popup(position, label.text, current, displayNames);
                property.stringValue = sceneNames[selected];
            }
            else if (property.propertyType == SerializedPropertyType.Integer)
            {
                int current = Mathf.Clamp(property.intValue, 0, sceneNames.Length - 1);
                int selected = EditorGUI.Popup(position, label.text, current, displayNames);
                property.intValue = selected;
            }
            else
            {
                EditorGUI.LabelField(position, label.text, AttributeNames.Usage<SceneNameAttribute>("a string or int"));
            }

            EditorGUI.EndProperty();
        }
    }
}