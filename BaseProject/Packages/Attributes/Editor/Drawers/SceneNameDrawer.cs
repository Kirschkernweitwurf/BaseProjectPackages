using System;
using System.IO;
using Base.UtilityPackage.Editor;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Drawers
{
    /// <summary>
    /// Property drawer for <see cref="SceneNameAttribute"/>.
    /// Shows a dropdown of the scenes in the Build Settings. On a string field it stores the scene
    /// name, on an int field it stores the build index. Dropdown labels include the build index.
    /// The stored value is only written when the user picks an entry, and a compact warning below
    /// points out values that no longer match the Build Settings. The scene list is cached and
    /// refreshed when the Build Settings change.
    /// </summary>
    [CustomPropertyDrawer(typeof(SceneNameAttribute))]
    internal sealed class SceneNameDrawer : WarningFieldDrawer
    {
        protected override string UsageMessage => AttributeNames.Usage<SceneNameAttribute>("a string or int");

        private static string[] _displayNames;
        private static string[] _sceneNames;

        protected override bool IsSupported(SerializedProperty property)
            => property.propertyType == SerializedPropertyType.String
                || property.propertyType == SerializedPropertyType.Integer;

        protected override string Evaluate(SerializedProperty property)
        {
            Build();

            if (_sceneNames.Length == 0)
                return "No scenes in the Build Settings.";

            if (property.propertyType == SerializedPropertyType.String)
            {
                if (string.IsNullOrEmpty(property.stringValue))
                    return null;

                return Array.IndexOf(_sceneNames, property.stringValue) >= 0
                    ? null
                    : $"Scene '{property.stringValue}' is not in the Build Settings.";
            }

            return property.intValue >= 0 && property.intValue < _sceneNames.Length
                ? null
                : $"Build index {property.intValue} is out of range.";
        }

        protected override void DrawField(Rect rect, SerializedProperty property, GUIContent label, bool complete)
        {
            if (_sceneNames.Length == 0)
                EditorGUI.PropertyField(rect, property, label);
            else if (property.propertyType == SerializedPropertyType.String)
                DrawStringDropdown(rect, property, label);
            else
                DrawIndexDropdown(rect, property, label);
        }

        [InitializeOnLoadMethod]
        private static void Install() => EditorBuildSettings.sceneListChanged += Invalidate;

        private static void DrawStringDropdown(Rect rect, SerializedProperty property, GUIContent label)
        {
            int current = Array.IndexOf(_sceneNames, property.stringValue);
            int selected = LabeledField.Popup(rect, label, current, _displayNames);
            if (selected >= 0 && selected < _sceneNames.Length && selected != current)
                property.stringValue = _sceneNames[selected];
        }

        private static void DrawIndexDropdown(Rect rect, SerializedProperty property, GUIContent label)
        {
            int current = property.intValue;
            if (current < 0 || current >= _sceneNames.Length)
                current = -1;

            int selected = LabeledField.Popup(rect, label, current, _displayNames);
            if (selected >= 0 && selected < _sceneNames.Length && selected != current)
                property.intValue = selected;
        }

        private static void Build()
        {
            if (_sceneNames != null)
                return;

            string[] scenePaths = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);

            _sceneNames = new string[scenePaths.Length];
            _displayNames = new string[scenePaths.Length];
            for (int i = 0; i < scenePaths.Length; i++)
            {
                _sceneNames[i] = Path.GetFileNameWithoutExtension(scenePaths[i]);
                _displayNames[i] = i + ": " + _sceneNames[i];
            }
        }

        private static void Invalidate()
        {
            _sceneNames = null;
            _displayNames = null;
        }
    }
}