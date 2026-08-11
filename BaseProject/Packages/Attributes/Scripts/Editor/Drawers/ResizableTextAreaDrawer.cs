using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws a multi-line text field that grows with its content for
    /// <see cref="ResizableTextAreaAttribute"/>. The label sits on its own row above the box, matching
    /// Unity's own text area, so a long string gets the full inspector width.
    /// </summary>
    /// <remarks>
    /// The height is measured from the text at the width the box last had. During the layout pass that
    /// width is not known yet, so the previous one is reused; the result is one frame of lag on the very
    /// first draw and none afterwards.
    /// </remarks>
    [CustomPropertyDrawer(typeof(ResizableTextAreaAttribute))]
    internal sealed class ResizableTextAreaDrawer : PropertyDrawer
    {
        private const float FallbackWidth = 300f;
        private const float Spacing = 2f;

        private float _width = FallbackWidth;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
                return EditorGUIUtility.singleLineHeight;

            return EditorGUIUtility.singleLineHeight + Spacing + BoxHeight(property.stringValue);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                LabeledField.Hint(position, label, AttributeNames.Usage<ResizableTextAreaAttribute>("a string"));
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            Rect labelRect = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(labelRect, label);

            // Indented here rather than by the control, so the box keeps the width left after the
            // indent instead of being shifted out of its own row.
            Rect box = EditorGUI.IndentedRect(new Rect(position.x, labelRect.yMax + Spacing, position.width,
                position.height - labelRect.height - Spacing));

            _width = box.width;

            using (new NoIndentScope())
                property.stringValue = EditorGUI.TextArea(box, property.stringValue, EditorStyles.textArea);

            EditorGUI.EndProperty();
        }

        private float BoxHeight(string text)
        {
            ResizableTextAreaAttribute settings = (ResizableTextAreaAttribute)attribute;
            float line = EditorGUIUtility.singleLineHeight;

            float measured = EditorStyles.textArea.CalcHeight(new GUIContent(text), _width);

            return Mathf.Clamp(measured, settings.MinimumLines * line, settings.MaximumLines * line);
        }
    }
}