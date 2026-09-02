using Base.AttributesPackage.Editor.Core;
using Base.UtilityPackage.Editor;
using Base.UtilityPackage.Editor.Serialization;
using Base.UtilityPackage.Serialization;
using UnityEditor;
using UnityEngine;

namespace Base.AttributesPackage.Editor.Drawers
{
    /// <summary>
    /// Draws a tick count as a date row with a calendar picker and an optional time of day row, for
    /// <see cref="DateAttribute"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(DateAttribute))]
    internal sealed class DateDrawer : PropertyDrawer
    {
        private const string Requirement = "a long of DateTime ticks or a SerializableDateTime";

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (Resolve(property) == null)
                return EditorGUIUtility.singleLineHeight;

            DateAttribute settings = (DateAttribute)attribute;

            if (settings.Display != EDateDisplay.DateAndTime)
                return EditorGUIUtility.singleLineHeight;

            return EditorGUIUtility.singleLineHeight * 2f + EditorGUIUtility.standardVerticalSpacing;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty ticks = Resolve(property);

            if (ticks == null)
            {
                LabeledField.Hint(position, label, AttributeNames.Usage<DateAttribute>(Requirement));
                return;
            }

            DateAttribute settings = (DateAttribute)attribute;
            Rect first = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            EditorGUI.BeginProperty(first, label, ticks);

            Rect field = LabeledField.Prefix(first, label);

            // The prefix label already consumed the indent, so the rows must not apply it again or
            // every nesting level walks them further right than the label they belong to.
            using (new NoIndentScope())
                DrawRows(field, ticks, settings);

            EditorGUI.EndProperty();
        }

        private static SerializedProperty Resolve(SerializedProperty property)
            => TickProperty.Resolve(property, SerializableDateTime.TicksField);

        private static void DrawRows(Rect field, SerializedProperty ticks, DateAttribute settings)
        {
            if (settings.Display == EDateDisplay.TimeOnly)
            {
                DateTimeGui.DrawTime(field, ticks, settings.ShowMilliseconds);
                return;
            }

            DateTimeGui.DrawDate(field, ticks);

            if (settings.Display != EDateDisplay.DateAndTime)
                return;

            Rect second = new(field.x, field.yMax + EditorGUIUtility.standardVerticalSpacing, field.width,
                EditorGUIUtility.singleLineHeight);

            DateTimeGui.DrawTime(second, ticks, settings.ShowMilliseconds);
        }
    }
}