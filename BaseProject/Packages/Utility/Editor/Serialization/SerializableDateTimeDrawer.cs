using Base.UtilityPackage.Serialization;
using UnityEditor;
using UnityEngine;

namespace Base.UtilityPackage.Editor.Serialization
{
    /// <summary>
    /// Draws a <see cref="SerializableDateTime"/> as a date row with a calendar picker and a time of
    /// day row underneath it. Narrow the display to one of the two with the Attributes package's
    /// <c>[Date]</c> or <c>[Time]</c> on the field.
    /// </summary>
    [CustomPropertyDrawer(typeof(SerializableDateTime))]
    public sealed class SerializableDateTimeDrawer : PropertyDrawer
    {
        private const int RowCount = 2;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (TickProperty.Resolve(property, SerializableDateTime.TicksField) == null)
                return EditorGUIUtility.singleLineHeight;

            return EditorGUIUtility.singleLineHeight * RowCount
                + EditorGUIUtility.standardVerticalSpacing * (RowCount - 1);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty ticks = TickProperty.Resolve(property, SerializableDateTime.TicksField);

            if (ticks == null)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            Rect first = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            EditorGUI.BeginProperty(first, label, ticks);

            Rect field = LabeledField.Prefix(first, label);

            // The prefix label already consumed the indent, so the rows must not apply it again or
            // every nesting level walks them further right than the label they belong to.
            using (new NoIndentScope())
            {
                DateTimeGui.DrawDate(field, ticks);

                Rect second = new(field.x, field.yMax + EditorGUIUtility.standardVerticalSpacing,
                    field.width, EditorGUIUtility.singleLineHeight);

                DateTimeGui.DrawTime(second, ticks, false);
            }

            EditorGUI.EndProperty();
        }
    }
}