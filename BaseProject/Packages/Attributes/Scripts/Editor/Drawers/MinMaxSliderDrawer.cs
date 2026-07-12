using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>Draws a Vector2 as a min-max range slider for <see cref="MinMaxSliderAttribute"/>.</summary>
    [CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
    public sealed class MinMaxSliderDrawer : PropertyDrawer
    {
        private const float FieldWidth = 50f;
        private const float Padding = 4f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Vector2)
            {
                EditorGUI.LabelField(position, label.text, "Use [MinMaxSlider] with a Vector2.");
                return;
            }

            MinMaxSliderAttribute attribute = (MinMaxSliderAttribute)this.attribute;

            EditorGUI.BeginProperty(position, label, property);

            Rect content = EditorGUI.PrefixLabel(position, label);

            Rect minRect = new(content.x, content.y, FieldWidth, content.height);
            Rect sliderRect = new(minRect.xMax + Padding, content.y,
                content.width - FieldWidth * 2f - Padding * 2f, content.height);

            Rect maxRect = new(sliderRect.xMax + Padding, content.y, FieldWidth, content.height);

            Vector2 range = property.vector2Value;
            float min = range.x;
            float max = range.y;

            min = EditorGUI.FloatField(minRect, min);
            EditorGUI.MinMaxSlider(sliderRect, ref min, ref max, attribute.Min, attribute.Max);
            max = EditorGUI.FloatField(maxRect, max);

            min = Mathf.Clamp(min, attribute.Min, max);
            max = Mathf.Clamp(max, min, attribute.Max);

            property.vector2Value = new Vector2(min, max);

            EditorGUI.EndProperty();
        }
    }
}