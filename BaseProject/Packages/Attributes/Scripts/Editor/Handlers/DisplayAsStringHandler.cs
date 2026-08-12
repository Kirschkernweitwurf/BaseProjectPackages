using System.Text;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws a <see cref="DisplayAsStringAttribute"/> member as one read-only line and suppresses the
    /// field itself.
    /// </summary>
    /// <remarks>
    /// A handler rather than a property drawer. Unity applies a drawer for a PropertyAttribute to each
    /// element of an array rather than to the array, so a drawer could only ever restyle the rows and
    /// never replace the list, which is the one thing this attribute is for.
    /// </remarks>
    internal sealed class DisplayAsStringHandler : IFieldReplacementHandler
    {
        private const string EmptyCollection = "()";
        private const string NullText = "null";

        // Reused across repaints, since building the text of a long collection every frame would
        // otherwise allocate a string per element per repaint.
        private static readonly StringBuilder Builder = new();

        public bool TryDraw(in MemberContext context)
        {
            DisplayAsStringAttribute attribute = context.GetAttribute<DisplayAsStringAttribute>();
            if (attribute == null)
                return false;

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.LabelField(context.Label,
                    ScratchContent.For(Describe(context.Property, attribute.Separator)));
            }

            return true;
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