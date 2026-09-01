using Base.UtilityPackage.Serialization;
using UnityEditor;
using UnityEngine;

namespace Base.UtilityPackage.Editor.Serialization
{
    /// <summary>
    /// Draws a <see cref="SerializableTimeSpan"/> as a signed day, hour, minute and second row. Add the
    /// millisecond field, or drop the day field, with the Attributes package's <c>[Time]</c> on the
    /// field.
    /// </summary>
    [CustomPropertyDrawer(typeof(SerializableTimeSpan))]
    public sealed class SerializableTimeSpanDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty ticks = TickProperty.Resolve(property, SerializableTimeSpan.TicksField);

            if (ticks == null)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            EditorGUI.BeginProperty(position, label, ticks);

            Rect field = LabeledField.Prefix(position, label);

            // The prefix label already consumed the indent, so the row must not apply it again or every
            // nesting level walks the fields further right than the label they belong to.
            using (new NoIndentScope())
                TimeSpanGui.Draw(field, ticks, true, false);

            EditorGUI.EndProperty();
        }
    }
}