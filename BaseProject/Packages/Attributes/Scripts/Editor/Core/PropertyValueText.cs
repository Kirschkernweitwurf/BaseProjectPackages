using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>Converts a serialized property value to a plain text form for the clipboard.</summary>
    public static class PropertyValueText
    {
        /// <summary>Returns a readable text value for the given property.</summary>
        public static string Read(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                    return property.stringValue;
                case SerializedPropertyType.Integer:
                    return property.intValue.ToString();
                case SerializedPropertyType.Boolean:
                    return property.boolValue.ToString();
                case SerializedPropertyType.Float:
                    return property.floatValue.ToString("R");
                case SerializedPropertyType.Enum:
                    int enumIndex = property.enumValueIndex;
                    return enumIndex >= 0 && enumIndex < property.enumDisplayNames.Length
                        ? property.enumDisplayNames[enumIndex]
                        : string.Empty;
                case SerializedPropertyType.Vector2:
                    return property.vector2Value.ToString("R");
                case SerializedPropertyType.Vector3:
                    return property.vector3Value.ToString("R");
                case SerializedPropertyType.Vector2Int:
                    return property.vector2IntValue.ToString();
                case SerializedPropertyType.Vector3Int:
                    return property.vector3IntValue.ToString();
                case SerializedPropertyType.Color:
                    return "#" + ColorUtility.ToHtmlStringRGBA(property.colorValue);
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue != null
                        ? property.objectReferenceValue.name
                        : string.Empty;
                default:
                    return property.displayName;
            }
        }
    }
}
