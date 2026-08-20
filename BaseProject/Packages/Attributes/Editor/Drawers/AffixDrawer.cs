using Base.UtilityPackage.Editor;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws the label of a <see cref="PrefixAttribute"/> in front of the value.
    /// </summary>
    /// <remarks>
    /// A prefix has to be drawn before the value, so it owns the field and is a property drawer. The
    /// suffix is a widget instead: only one drawer runs per field, and as a drawer it lost every time
    /// the field also carried a slider or a bar.
    /// </remarks>
    [CustomPropertyDrawer(typeof(PrefixAttribute))]
    internal sealed class AffixDrawer : PropertyDrawer
    {
        private const float Padding = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            PrefixAttribute prefix = ReflectionCache.GetAttribute<PrefixAttribute>(fieldInfo);

            EditorGUI.BeginProperty(position, label, property);

            Rect valueRect = EditorGUI.PrefixLabel(position, label);

            // The prefix label already consumed the indent and every rect below is computed by hand, so
            // the whole remainder is drawn flat. Drawing the prefix outside this scope is what used to
            // shrink it to the point of cutting the text off.
            using (new NoIndentScope())
            {
                valueRect = DrawPrefix(valueRect, prefix);

                EditorGUI.PropertyField(valueRect, property, GUIContent.none, true);
            }

            EditorGUI.EndProperty();
        }

        private static Rect DrawPrefix(Rect valueRect, PrefixAttribute prefix)
        {
            if (prefix == null)
                return valueRect;

            float width = EditorStyles.miniLabel.CalcSize(ScratchContent.For(prefix.Text)).x;
            Rect prefixRect = new(valueRect.x, valueRect.y, width, valueRect.height);
            DrawLabel(prefixRect, prefix.Text);

            valueRect.x += width + Padding;
            valueRect.width -= width + Padding;
            return valueRect;
        }

        // GUI.Label rather than EditorGUI.LabelField. The LabelField overload taking a string and a
        // style treats that string as the value half of a labeled row, so it reserves the inspector's
        // whole label width for an empty prefix first and draws the text in whatever is left. In a rect
        // sized to the text itself there is nothing left, which is what cut these labels down to their
        // first two characters.
        private static void DrawLabel(Rect rect, string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            using (new EditorGUI.DisabledScope(true))
                GUI.Label(rect, text, EditorStyles.miniLabel);
        }
    }
}