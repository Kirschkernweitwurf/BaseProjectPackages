using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws an enum as toggle buttons for <see cref="EnumToggleButtonsAttribute"/>. Flags enums
    /// become multi-select toggles. Buttons wrap onto additional rows when they would not fit next to
    /// each other, so labels stay readable no matter how many values the enum has. Labels and values
    /// are cached per enum type.
    /// </summary>
    [CustomPropertyDrawer(typeof(EnumToggleButtonsAttribute))]
    public sealed class EnumToggleButtonsDrawer : PropertyDrawer
    {
        private const float ContentWidthMargin = 40f;
        private const float RowSpacing = 2f;

        private static readonly Dictionary<Type, EnumButtonLayout> Layouts = new();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            EnumButtonLayout layout = ResolveLayout(property);
            if (layout == null)
                return EditorGUIUtility.singleLineHeight;

            int rows = RowCount(layout, EstimateContentWidth());
            return rows * EditorGUIUtility.singleLineHeight + (rows - 1) * RowSpacing;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Enum)
            {
                EditorGUI.LabelField(position, label.text, AttributeNames.Usage<EnumToggleButtonsAttribute>("an enum"));
                return;
            }

            EnumButtonLayout layout = ResolveLayout(property);
            if (layout == null)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            Rect firstLine = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            Rect content = EditorGUI.PrefixLabel(firstLine, label);

            EditorGUI.BeginProperty(position, label, property);

            if (layout.IsFlags)
                DrawFlags(content, property, layout);
            else
                DrawSingle(content, property, layout);

            EditorGUI.EndProperty();
        }

        private static void DrawSingle(Rect content, SerializedProperty property, EnumButtonLayout layout)
        {
            int selected = property.enumValueIndex;

            for (int i = 0; i < layout.Labels.Length; i++)
            {
                bool wasOn = i == selected;
                bool nowOn = GUI.Toggle(ButtonRect(content, layout, i), wasOn, layout.Labels[i],
                    ButtonStyle(content, layout, i));

                if (nowOn && !wasOn)
                    property.enumValueIndex = i;
            }
        }

        private static void DrawFlags(Rect content, SerializedProperty property, EnumButtonLayout layout)
        {
            int mask = property.intValue;

            for (int i = 0; i < layout.Labels.Length; i++)
            {
                int bits = layout.Values[i];
                bool wasOn = (mask & bits) == bits;
                bool nowOn = GUI.Toggle(ButtonRect(content, layout, i), wasOn, layout.Labels[i],
                    ButtonStyle(content, layout, i));

                if (nowOn != wasOn)
                    mask = nowOn
                        ? mask | bits
                        : mask & ~bits;
            }

            property.intValue = mask;
        }

        private static Rect ButtonRect(Rect content, EnumButtonLayout layout, int index)
        {
            int columns = ColumnCount(layout, content.width);
            float buttonWidth = content.width / columns;
            float rowHeight = EditorGUIUtility.singleLineHeight;

            int row = index / columns;
            int column = index % columns;

            return new Rect(content.x + column * buttonWidth, content.y + row * (rowHeight + RowSpacing),
                buttonWidth, rowHeight);
        }

        private static GUIStyle ButtonStyle(Rect content, EnumButtonLayout layout, int index)
        {
            int columns = ColumnCount(layout, content.width);
            int row = index / columns;
            int column = index % columns;
            int countInRow = Mathf.Min(columns, layout.Labels.Length - row * columns);

            if (countInRow == 1)
                return EditorStyles.miniButton;

            if (column == 0)
                return EditorStyles.miniButtonLeft;

            if (column == countInRow - 1)
                return EditorStyles.miniButtonRight;

            return EditorStyles.miniButtonMid;
        }

        private EnumButtonLayout ResolveLayout(SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.Enum)
                return null;

            Type enumType = GetEnumType();
            if (enumType == null)
                return null;

            if (Layouts.TryGetValue(enumType, out EnumButtonLayout cached))
                return cached;

            EnumButtonLayout layout = EnumButtonLayout.Build(enumType, property);
            Layouts[enumType] = layout;
            return layout;
        }

        private Type GetEnumType()
        {
            Type type = fieldInfo?.FieldType;
            if (type == null)
                return null;

            if (type.IsArray)
                type = type.GetElementType();
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                type = type.GetGenericArguments()[0];

            return type is { IsEnum: true }
                ? type
                : null;
        }

        private static int ColumnCount(EnumButtonLayout layout, float availableWidth)
        {
            int fitting = Mathf.FloorToInt(availableWidth / layout.MinButtonWidth);
            return Mathf.Clamp(fitting, 1, layout.Labels.Length);
        }

        private static int RowCount(EnumButtonLayout layout, float availableWidth)
            => Mathf.CeilToInt(layout.Labels.Length / (float)ColumnCount(layout, availableWidth));

        private static float EstimateContentWidth()
            => Mathf.Max(1f, EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth - ContentWidthMargin);
    }
}