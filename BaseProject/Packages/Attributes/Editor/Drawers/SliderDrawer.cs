using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Drawers
{
    /// <summary>
    /// Draws a slider whose bounds may come from other members, for <see cref="SliderAttribute"/>.
    /// </summary>
    /// <remarks>
    /// A drawer rather than a handler, because it replaces how the value itself is drawn. It resolves
    /// against the inspected object rather than through a member context, since a drawer is given the
    /// property and nothing else.
    /// </remarks>
    [CustomPropertyDrawer(typeof(SliderAttribute))]
    internal sealed class SliderDrawer : WarningFieldDrawer
    {
        private const string MissingBoundMessage = "The bound member was not found, so the field is drawn plain.";

        protected override string UsageMessage => AttributeNames.Usage<SliderAttribute>("a float or int");

        private float _min;
        private float _max;

        protected override bool IsSupported(SerializedProperty property)
            => property.propertyType == SerializedPropertyType.Float
                || property.propertyType == SerializedPropertyType.Integer;

        protected override string Evaluate(SerializedProperty property)
            => Resolve(property, (SliderAttribute)attribute, out _min, out _max)
                ? null
                : MissingBoundMessage;

        protected override void DrawField(Rect rect, SerializedProperty property, GUIContent label, bool complete)
        {
            if (!complete)
            {
                EditorGUI.PropertyField(rect, property, label);
                return;
            }

            SliderAttribute settings = (SliderAttribute)attribute;

            // A backwards range would make the slider unusable rather than merely wrong, so the bounds
            // are ordered before anything is drawn.
            float low = Mathf.Min(_min, _max);
            float high = Mathf.Max(_min, _max);

            if (property.propertyType == SerializedPropertyType.Integer)
                DrawInteger(rect, property, label, low, high, settings.AutoClamp);
            else
                DrawFloat(rect, property, label, low, high, settings.AutoClamp);
        }

        private static void DrawInteger(Rect rect, SerializedProperty property, GUIContent label, float low,
            float high, bool autoClamp)
        {
            int minimum = Mathf.RoundToInt(low);
            int maximum = Mathf.RoundToInt(high);

            if (autoClamp)
                property.intValue = Mathf.Clamp(property.intValue, minimum, maximum);

            property.intValue = EditorGUI.IntSlider(rect, label, property.intValue, minimum, maximum);
        }

        private static void DrawFloat(Rect rect, SerializedProperty property, GUIContent label, float low,
            float high, bool autoClamp)
        {
            if (autoClamp)
                property.floatValue = Mathf.Clamp(property.floatValue, low, high);

            property.floatValue = EditorGUI.Slider(rect, label, property.floatValue, low, high);
        }

        private static bool Resolve(SerializedProperty property, SliderAttribute settings, out float min,
            out float max)
        {
            min = settings.Min;
            max = settings.Max;

            if (!string.IsNullOrEmpty(settings.RangeMember))
                return BoundResolver.TryRange(property, settings.RangeMember, out min, out max);

            bool resolved = true;

            if (!string.IsNullOrEmpty(settings.MinMember))
                resolved &= BoundResolver.TryNumber(property, settings.MinMember, out min);

            if (!string.IsNullOrEmpty(settings.MaxMember))
                resolved &= BoundResolver.TryNumber(property, settings.MaxMember, out max);

            return resolved;
        }
    }
}