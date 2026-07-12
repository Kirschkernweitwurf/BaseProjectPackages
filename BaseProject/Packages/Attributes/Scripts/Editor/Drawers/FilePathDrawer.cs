using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws a file path field with a browse button for <see cref="FilePathAttribute"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(FilePathAttribute))]
    public sealed class FilePathDrawer : PropertyDrawer
    {
        private const float ButtonWidth = 28f;
        private const float Spacing = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "Use [FilePath] with a string.");
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

            FilePathAttribute file = (FilePathAttribute)attribute;
            SerializedObject serializedObject = property.serializedObject;
            string propertyPath = property.propertyPath;
            bool absolute = file.Absolute;
            string extension = file.Extension;

            // OpenFilePanel clears Unity's internal property stack while open. Running it inside
            // OnGUI corrupts the outer PropertyDrawer.OnGUISafe pop, so defer to delayCall.
            EditorApplication.delayCall += () =>
            {
                string selected = EditorUtility.OpenFilePanel("Select File", Application.dataPath, extension);
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