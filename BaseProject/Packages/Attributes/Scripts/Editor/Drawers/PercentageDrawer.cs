using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws a normalized float as a percentage for <see cref="PercentageAttribute"/>. The value is
    /// shown and edited in the zero to one hundred range with a trailing percent sign, while it stays
    /// stored as zero to one. The field keeps its label so the value stays drag editable, and the sign
    /// is drawn at indent level zero so it is not pushed off screen inside indented or foldout sections.
    /// </summary>
    [CustomPropertyDrawer(typeof(PercentageAttribute))]
    public sealed class PercentageDrawer : PropertyDrawer
    {
        private const float Gap = 2f;
        private const float SignWidth = 16f;
        private const float ValueWidth = 50f;

        private static GUIStyle SignStyle => _signStyle ??= new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleLeft
        };

        private static GUIStyle _signStyle;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Float)
            {
                EditorGUI.LabelField(position, label.text, AttributeNames.Usage<PercentageAttribute>("a float"));
                return;
            }

            PercentageAttribute percentage = (PercentageAttribute)attribute;

            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();

            float fullWidth = position.width - SignWidth - Gap;
            float controlWidth = percentage.Slider
                ? fullWidth
                : Mathf.Min(EditorGUIUtility.labelWidth + ValueWidth, fullWidth);

            Rect controlRect = new(position.x, position.y, controlWidth, position.height);
            Rect signRect = new(controlRect.xMax + Gap, position.y, SignWidth, position.height);

            float percent = Mathf.Clamp01(property.floatValue) * 100f;

            // The label is passed to the field on purpose: hovering it gives the drag to scrub cursor.
            float edited = percentage.Slider
                ? EditorGUI.Slider(controlRect, label, percent, 0f, 100f)
                : EditorGUI.FloatField(controlRect, label, percent);

            // Draw the sign at indent level zero, otherwise it is shifted right and clipped in foldouts.
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            EditorGUI.LabelField(signRect, "%", SignStyle);
            EditorGUI.indentLevel = indent;

            if (EditorGUI.EndChangeCheck())
                property.floatValue = Mathf.Clamp01(edited / 100f);

            EditorGUI.EndProperty();
        }
    }
}