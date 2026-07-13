using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws a normalized float as a percentage for <see cref="PercentageAttribute"/>. The value is
    /// shown and edited in the zero to one hundred range with a trailing percent sign, while it stays
    /// stored as zero to one.
    /// </summary>
    [CustomPropertyDrawer(typeof(PercentageAttribute))]
    public sealed class PercentageDrawer : PropertyDrawer
    {
        private const float SignWidth = 14f;
        private const float Gap = 2f;

        private static GUIStyle _signStyle;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Float)
            {
                EditorGUI.LabelField(position, label.text, "Use [Percentage] with a float.");
                return;
            }

            PercentageAttribute percentage = (PercentageAttribute)attribute;

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();

            Rect controlRect = new(position.x, position.y, position.width - SignWidth - Gap, position.height);
            Rect signRect = new(controlRect.xMax + Gap, position.y, SignWidth, position.height);

            float percent = Mathf.Clamp01(property.floatValue) * 100f;
            float edited = percentage.Slider
                ? EditorGUI.Slider(controlRect, label, percent, 0f, 100f)
                : EditorGUI.FloatField(controlRect, label, percent);

            EditorGUI.LabelField(signRect, "%", SignStyle);

            if (EditorGUI.EndChangeCheck())
                property.floatValue = Mathf.Clamp01(edited / 100f);

            EditorGUI.EndProperty();
        }

        private static GUIStyle SignStyle => _signStyle ??= new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleLeft
        };
    }
}
