using Base.AttributePackage.Editor.Core;
using Base.AttributePackage.References;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Drawers
{
    /// <summary>
    /// Draws a folder path field with a browse button for <see cref="FolderPathAttribute"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(FolderPathAttribute))]
    public sealed class FolderPathDrawer : PropertyDrawer
    {
        private const float ButtonWidth = 28f;
        private const float Spacing = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "Use [FolderPath] with a string.");
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            Rect fieldRect = new(position.x, position.y, position.width - ButtonWidth - Spacing, position.height);
            Rect buttonRect = new(fieldRect.xMax + Spacing, position.y, ButtonWidth, position.height);

            EditorGUI.PropertyField(fieldRect, property, label);
            bool browseClicked = GUI.Button(buttonRect, "...");

            EditorGUI.EndProperty();

            if (!browseClicked)
                return;

            FolderPathAttribute folder = (FolderPathAttribute)attribute;
            SerializedObject serializedObject = property.serializedObject;
            string propertyPath = property.propertyPath;
            bool absolute = folder.Absolute;

            // OpenFolderPanel clears Unity's internal property stack while open. Running it inside
            // OnGUI corrupts the outer PropertyDrawer.OnGUISafe pop, so defer to delayCall.
            EditorApplication.delayCall += () =>
            {
                string selected = EditorUtility.OpenFolderPanel("Select Folder", Application.dataPath, string.Empty);
                if (string.IsNullOrEmpty(selected))
                    return;

                if (serializedObject.targetObject == null)
                    return;

                serializedObject.Update();
                SerializedProperty deferred = serializedObject.FindProperty(propertyPath);
                if (deferred != null)
                {
                    deferred.stringValue = absolute
                        ? PathUtility.Normalize(selected)
                        : PathUtility.ToProjectRelative(selected);

                    serializedObject.ApplyModifiedProperties();
                }

                EditorGUIUtility.editingTextField = false;
            };
        }
    }
}