using Base.AttributesPackage.Editor.Core;
using Base.UtilityPackage.Editor;
using Base.UtilityPackage.Editor.Serialization;
using Base.UtilityPackage.Serialization;
using UnityEditor;
using UnityEngine;

namespace Base.AttributesPackage.Editor.Drawers
{
    /// <summary>
    /// Draws a tick count as a signed duration row for <see cref="TimeAttribute"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(TimeAttribute))]
    internal sealed class TimeDrawer : PropertyDrawer
    {
        private const string Requirement = "a long of TimeSpan ticks or a SerializableTimeSpan";

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty ticks = TickProperty.Resolve(property, SerializableTimeSpan.TicksField);

            if (ticks == null)
            {
                LabeledField.Hint(position, label, AttributeNames.Usage<TimeAttribute>(Requirement));
                return;
            }

            TimeAttribute settings = (TimeAttribute)attribute;

            EditorGUI.BeginProperty(position, label, ticks);

            Rect field = LabeledField.Prefix(position, label);

            // The prefix label already consumed the indent, so the row must not apply it again or every
            // nesting level walks the fields further right than the label they belong to.
            using (new NoIndentScope())
                TimeSpanGui.Draw(field, ticks, settings.ShowDays, settings.ShowMilliseconds);

            EditorGUI.EndProperty();
        }
    }
}