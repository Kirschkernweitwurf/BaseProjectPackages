using System.Text;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws a value as read-only text for <see cref="DisplayAsStringAttribute"/>, collapsing a whole
    /// collection onto the one line rather than expanding it into rows.
    /// </summary>
    [CustomPropertyDrawer(typeof(DisplayAsStringAttribute))]
    internal sealed class DisplayAsStringDrawer : PropertyDrawer
    {
        private const string EmptyCollection = "()";
        private const string NullText = "null";

        // Reused across repaints, since building the text of a long collection every frame would
        // otherwise allocate a string per element per repaint.
        private static readonly StringBuilder Builder = new();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DisplayAsStringAttribute settings = (DisplayAsStringAttribute)attribute;

            EditorGUI.BeginProperty(position, label, property);

            using (new EditorGUI.DisabledScope(true))
                EditorGUI.LabelField(position, label, ScratchContent.For(Describe(property, settings.Separator)));

            EditorGUI.EndProperty();
        }

        private static string Describe(SerializedProperty property, string separator)
        {
            if (!property.isArray || property.propertyType == SerializedPropertyType.String)
                return Read(property);

            if (property.arraySize == 0)
                return EmptyCollection;

            Builder.Clear();

            for (int i = 0; i < property.arraySize; i++)
            {
                if (i > 0)
                    Builder.Append(separator);

                Builder.Append(Read(property.GetArrayElementAtIndex(i)));
            }

            return Builder.ToString();
        }

        private static string Read(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                    return property.stringValue;
                case SerializedPropertyType.Integer:
                    return property.longValue.ToString();
                case SerializedPropertyType.Float:
                    return property.doubleValue.ToString("0.###");
                case SerializedPropertyType.Boolean:
                    return property.boolValue.ToString();
                case SerializedPropertyType.Enum:
                    return property.enumValueIndex >= 0 && property.enumValueIndex < property.enumDisplayNames.Length
                        ? property.enumDisplayNames[property.enumValueIndex]
                        : property.intValue.ToString();
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue == null
                        ? NullText
                        : property.objectReferenceValue.name;
                case SerializedPropertyType.Vector2:
                    return property.vector2Value.ToString();
                case SerializedPropertyType.Vector3:
                    return property.vector3Value.ToString();
                case SerializedPropertyType.Color:
                    return property.colorValue.ToString();
                default:
                    return property.displayName;
            }
        }
    }
}