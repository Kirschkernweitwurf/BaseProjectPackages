using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws an enum as toolbar buttons for <see cref="EnumToggleButtonsAttribute"/>.
    /// Flags enums become multi-select toggles.
    /// </summary>
    [CustomPropertyDrawer(typeof(EnumToggleButtonsAttribute))]
    public sealed class EnumToggleButtonsDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Enum)
            {
                EditorGUI.LabelField(position, label.text, "Use [EnumToggleButtons] with an enum.");
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

            Rect content = EditorGUI.PrefixLabel(position, label);

            EditorGUI.BeginProperty(position, label, property);
            if (Attribute.IsDefined(enumType, typeof(FlagsAttribute)))
                DrawFlags(content, property, enumType);
            else
                DrawSingle(content, property);

            EditorGUI.EndProperty();
        }

        private static void DrawSingle(Rect content, SerializedProperty property)
        {
            int index = GUI.Toolbar(content, property.enumValueIndex, property.enumDisplayNames);
            if (index != property.enumValueIndex && index >= 0)
                property.enumValueIndex = index;
        }

        private static void DrawFlags(Rect content, SerializedProperty property, Type enumType)
        {
            Array values = Enum.GetValues(enumType);
            string[] names = Enum.GetNames(enumType);

            List<int> bits = new();
            List<string> labels = new();
            for (int i = 0; i < values.Length; i++)
            {
                int value = Convert.ToInt32(values.GetValue(i));
                if (value == 0)
                    continue;

                bits.Add(value);
                labels.Add(ObjectNames.NicifyVariableName(names[i]));
            }

            if (bits.Count == 0)
                return;

            int mask = property.intValue;
            float width = content.width / bits.Count;

            for (int i = 0; i < bits.Count; i++)
            {
                Rect rect = new(content.x + i * width, content.y, width, content.height);
                GUIStyle style = ButtonStyle(i, bits.Count);

                bool isOn = (mask & bits[i]) == bits[i];
                bool nowOn = GUI.Toggle(rect, isOn, labels[i], style);
                if (nowOn != isOn)
                    mask = nowOn
                        ? mask | bits[i]
                        : mask & ~bits[i];
            }

            property.intValue = mask;
        }

        private static GUIStyle ButtonStyle(int index, int count)
        {
            if (count == 1)
                return EditorStyles.miniButton;

            if (index == 0)
                return EditorStyles.miniButtonLeft;

            if (index == count - 1)
                return EditorStyles.miniButtonRight;

            return EditorStyles.miniButtonMid;
        }
    }
}