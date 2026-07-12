using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>Draws a field with a small suffix label for <see cref="SuffixAttribute"/>.</summary>
    [CustomPropertyDrawer(typeof(SuffixAttribute))]
    public sealed class SuffixDrawer : PropertyDrawer
    {
        private const float Padding = 4f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SuffixAttribute suffix = (SuffixAttribute)attribute;

            GUIContent content = new(suffix.Text);
            float suffixWidth = EditorStyles.miniLabel.CalcSize(content).x + Padding;

            Rect fieldRect = new(position.x, position.y, position.width - suffixWidth, position.height);
            Rect suffixRect = new(fieldRect.xMax + Padding, position.y, suffixWidth - Padding, position.height);

            EditorGUI.PropertyField(fieldRect, property, label, true);

            using (new EditorGUI.DisabledScope(true))
                EditorGUI.LabelField(suffixRect, content, EditorStyles.miniLabel);
        }
    }
}