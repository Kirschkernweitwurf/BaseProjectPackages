using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
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

            FolderPathAttribute folder = (FolderPathAttribute)attribute;

            Rect fieldRect = new(position.x, position.y, position.width - ButtonWidth - Spacing, position.height);
            Rect buttonRect = new(fieldRect.xMax + Spacing, position.y, ButtonWidth, position.height);

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PropertyField(fieldRect, property, label);

            if (GUI.Button(buttonRect, "..."))
            {
                string selected = EditorUtility.OpenFolderPanel("Select Folder", Application.dataPath, string.Empty);
                if (!string.IsNullOrEmpty(selected))
                    property.stringValue = folder.Absolute
                        ? selected
                        : ToRelativePath(selected);
            }

            EditorGUI.EndProperty();
        }

        private static string ToRelativePath(string absolute)
        {
            string dataPath = Application.dataPath.Replace("\\", "/");
            string normalized = absolute.Replace("\\", "/");

            if (normalized == dataPath)
                return "Assets";

            if (normalized.StartsWith(dataPath + "/"))
                return "Assets" + normalized.Substring(dataPath.Length);

            return absolute;
        }
    }
}