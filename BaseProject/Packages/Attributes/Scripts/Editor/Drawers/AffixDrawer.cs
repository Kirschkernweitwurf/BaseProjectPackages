using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws a field with an optional prefix and suffix label for <see cref="PrefixAttribute"/> and
    /// <see cref="SuffixAttribute"/>. Registered for both, so a field with both attributes causes Unity
    /// to invoke this drawer twice as a chain. Only one invocation draws the labels, the other just
    /// draws the value, which keeps each label from appearing twice.
    /// </summary>
    [CustomPropertyDrawer(typeof(PrefixAttribute))]
    [CustomPropertyDrawer(typeof(SuffixAttribute))]
    internal sealed class AffixDrawer : PropertyDrawer
    {
        private const float Padding = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            PrefixAttribute prefix = ReflectionCache.GetAttribute<PrefixAttribute>(fieldInfo);
            SuffixAttribute suffix = ReflectionCache.GetAttribute<SuffixAttribute>(fieldInfo);

            // When both attributes are present Unity chains two invocations. The suffix invocation owns
            // the drawing, the prefix invocation only forwards the value, so nothing is drawn twice.
            bool ownsDrawing = attribute is SuffixAttribute || suffix == null;
            if (!ownsDrawing)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            Rect valueRect = EditorGUI.PrefixLabel(position, label);

            // The prefix label already consumed the indent and every rect below is computed by hand, so
            // the whole remainder is drawn flat. Drawing the prefix outside this scope is what used to
            // shrink it to the point of cutting the text off.
            using (new NoIndentScope())
            {
                valueRect = DrawPrefix(valueRect, prefix);
                valueRect = ReserveSuffix(valueRect, suffix, out Rect suffixRect);

                EditorGUI.PropertyField(valueRect, property, GUIContent.none, true);
                DrawLabel(suffixRect, suffix?.Text);
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

        private static Rect ReserveSuffix(Rect valueRect, SuffixAttribute suffix, out Rect suffixRect)
        {
            if (suffix == null)
            {
                suffixRect = default(Rect);
                return valueRect;
            }

            float width = EditorStyles.miniLabel.CalcSize(ScratchContent.For(suffix.Text)).x + Padding;
            valueRect.width -= width;
            suffixRect = new Rect(valueRect.xMax + Padding, valueRect.y, width - Padding, valueRect.height);
            return valueRect;
        }

        // GUI.Label rather than EditorGUI.LabelField. The LabelField overload taking a string and a
        // style treats that string as the value half of a labelled row, so it reserves the inspector's
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