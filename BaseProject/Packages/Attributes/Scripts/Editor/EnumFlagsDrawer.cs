using System;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws a multi-select mask field for <see cref="EnumFlagsAttribute"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
    public sealed class EnumFlagsDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Enum)
            {
                EditorGUI.LabelField(position, label.text, "Use [EnumFlags] with an enum.");
                return;
            }

            Type enumType = fieldInfo.FieldType;
            if (enumType.IsArray)
                enumType = enumType.GetElementType();

            if (enumType == null || !enumType.IsEnum)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            Enum current = (Enum)Enum.ToObject(enumType, property.intValue);

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            Enum result = EditorGUI.EnumFlagsField(position, label, current);
            if (EditorGUI.EndChangeCheck())
                property.intValue = Convert.ToInt32(result);

            EditorGUI.EndProperty();
        }
    }
}