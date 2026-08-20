using Base.UtilityPackage.Editor;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Drawers
{
    /// <summary>Draws a Vector2 as a min-max range slider for <see cref="MinMaxSliderAttribute"/>.</summary>
    [CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
    internal sealed class MinMaxSliderDrawer : PropertyDrawer
    {
        private const float FieldWidth = 50f;
        private const float Padding = 4f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Vector2)
            {
                LabeledField.Hint(position, label, AttributeNames.Usage<MinMaxSliderAttribute>("a Vector2"));
                return;
            }

            MinMaxSliderAttribute settings = (MinMaxSliderAttribute)attribute;
            Resolve(property, settings, out float low, out float high);

            // A backwards range would make the slider unusable rather than merely wrong.
            float bottom = Mathf.Min(low, high);
            float top = Mathf.Max(low, high);

            EditorGUI.BeginProperty(position, label, property);

            Rect content = EditorGUI.PrefixLabel(position, label);

            Rect minRect = new(content.x, content.y, FieldWidth, content.height);
            Rect sliderRect = new(minRect.xMax + Padding, content.y,
                content.width - FieldWidth * 2f - Padding * 2f, content.height);

            Rect maxRect = new(sliderRect.xMax + Padding, content.y, FieldWidth, content.height);

            Vector2 range = property.vector2Value;
            float min = range.x;
            float max = range.y;

            if (settings.AutoClamp)
            {
                min = Mathf.Clamp(min, bottom, top);
                max = Mathf.Clamp(max, bottom, top);
            }

            using (new NoIndentScope())
            {
                EditorGUI.BeginChangeCheck();

                min = EditorGUI.FloatField(minRect, min);
                EditorGUI.MinMaxSlider(sliderRect, ref min, ref max, bottom, top);
                max = EditorGUI.FloatField(maxRect, max);

                if (EditorGUI.EndChangeCheck() || settings.AutoClamp)
                    property.vector2Value = new Vector2(Mathf.Min(min, max), Mathf.Max(min, max));
            }

            EditorGUI.EndProperty();
        }

        private static void Resolve(SerializedProperty property, MinMaxSliderAttribute settings, out float min,
            out float max)
        {
            min = settings.Min;
            max = settings.Max;

            if (!string.IsNullOrEmpty(settings.RangeMember))
            {
                BoundResolver.TryRange(property, settings.RangeMember, out min, out max);
                return;
            }

            if (!string.IsNullOrEmpty(settings.MinMember))
                BoundResolver.TryNumber(property, settings.MinMember, out min);

            if (!string.IsNullOrEmpty(settings.MaxMember))
                BoundResolver.TryNumber(property, settings.MaxMember, out max);
        }
    }
}