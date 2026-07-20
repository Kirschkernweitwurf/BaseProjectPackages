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
    public sealed class AffixDrawer : PropertyDrawer
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
            valueRect = DrawPrefix(valueRect, prefix);
            valueRect = ReserveSuffix(valueRect, suffix, out Rect suffixRect);

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            EditorGUI.PropertyField(valueRect, property, GUIContent.none, true);
            DrawLabel(suffixRect, suffix?.Text);

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        private static Rect DrawPrefix(Rect valueRect, PrefixAttribute prefix)
        {
            if (prefix == null)
                return valueRect;

            float width = EditorStyles.miniLabel.CalcSize(new GUIContent(prefix.Text)).x;
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

            float width = EditorStyles.miniLabel.CalcSize(new GUIContent(suffix.Text)).x + Padding;
            valueRect.width -= width;
            suffixRect = new Rect(valueRect.xMax + Padding, valueRect.y, width - Padding, valueRect.height);
            return valueRect;
        }

        private static void DrawLabel(Rect rect, string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            using (new EditorGUI.DisabledScope(true))
                EditorGUI.LabelField(rect, text, EditorStyles.miniLabel);
        }
    }
}