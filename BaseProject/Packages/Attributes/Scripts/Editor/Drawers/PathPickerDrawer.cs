using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Shared base for <see cref="FilePathDrawer"/> and <see cref="FolderPathDrawer"/>. Draws the
    /// string field with a browse button and applies the selected path back to the property. The
    /// system panel is deferred to <see cref="EditorApplication.delayCall"/>, since opening it inside
    /// OnGUI clears Unity's internal property stack and corrupts the outer drawer pop.
    /// </summary>
    public abstract class PathPickerDrawer : PropertyDrawer
    {
        private const float ButtonWidth = 28f;
        private const string ButtonLabel = "...";
        private const float Spacing = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, UsageMessage);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            Rect fieldRect = new(position.x, position.y, position.width - ButtonWidth - Spacing, position.height);
            Rect buttonRect = new(fieldRect.xMax + Spacing, position.y, ButtonWidth, position.height);

            EditorGUI.PropertyField(fieldRect, property, label);
            bool browseClicked = GUI.Button(buttonRect, ButtonLabel);

            EditorGUI.EndProperty();

            if (!browseClicked)
                return;

            SerializedObject serializedObject = property.serializedObject;
            string propertyPath = property.propertyPath;
            bool absolute = IsAbsolute;

            EditorApplication.delayCall += () =>
            {
                string selected = OpenPanel();
                if (string.IsNullOrEmpty(selected))
                    return;

                Apply(serializedObject, propertyPath, absolute, selected);
                EditorGUIUtility.editingTextField = false;
            };
        }

        /// <summary>Whether the selected path is stored absolute instead of project relative.</summary>
        protected abstract bool IsAbsolute { get; }

        /// <summary>Message shown when the attribute sits on a non-string field.</summary>
        protected abstract string UsageMessage { get; }

        /// <summary>Opens the system panel and returns the selected path, or empty when cancelled.</summary>
        protected abstract string OpenPanel();

        private static void Apply(SerializedObject serializedObject, string propertyPath, bool absolute,
            string selected)
        {
            if (serializedObject.targetObject == null)
                return;

            serializedObject.Update();
            SerializedProperty property = serializedObject.FindProperty(propertyPath);
            if (property == null)
                return;

            property.stringValue = absolute
                ? PathUtility.Normalize(selected)
                : PathUtility.ToProjectRelative(selected);

            serializedObject.ApplyModifiedProperties();
        }
    }
}