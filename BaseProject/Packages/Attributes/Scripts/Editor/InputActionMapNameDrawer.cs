#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Attributes.Editor
{
    /// <summary>
    /// Property drawer for <see cref="InputActionMapNameAttribute"/>.
    /// Displays a dropdown of all available Input Action Maps in the project.
    /// </summary>
    [CustomPropertyDrawer(typeof(InputActionMapNameAttribute))]
    public class InputActionMapNameDrawer : PropertyDrawer
    {
        private static string[] _mapNames;
        private static bool _initialized;

        private void Initialize()
        {
            if (_initialized) 
                return;
            
            _initialized = true;

            List<string> allNames = new();
            
            string[] guids = AssetDatabase.FindAssets("t:InputActionAsset");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                InputActionAsset asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
                if (asset == null)
                    continue;

                foreach (InputActionMap map in asset.actionMaps)
                {
                    if (!allNames.Contains(map.name))
                        allNames.Add(map.name);
                }
            }

            _mapNames = allNames.ToArray();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize();

            EditorGUI.BeginProperty(position, label, property);

            int currentIndex = Array.IndexOf(_mapNames, property.stringValue);
            if (currentIndex < 0) 
                currentIndex = 0;

            int newIndex = EditorGUI.Popup(position, label.text, currentIndex, _mapNames);
            property.stringValue = _mapNames.Length > 0 ? _mapNames[newIndex] : string.Empty;

            EditorGUI.EndProperty();
        }
    }
}
#endif