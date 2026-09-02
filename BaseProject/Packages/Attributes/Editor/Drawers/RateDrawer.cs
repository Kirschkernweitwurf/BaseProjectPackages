using Base.AttributesPackage.Editor.Core;
using Base.EditorUIPackage.Editor;
using Base.UtilityPackage.Editor;
using UnityEditor;
using UnityEngine;

namespace Base.AttributesPackage.Editor.Drawers
{
    /// <summary>Draws an int as a row of clickable stars for <see cref="RateAttribute"/>.</summary>
    [CustomPropertyDrawer(typeof(RateAttribute))]
    internal sealed class RateDrawer : PropertyDrawer
    {
        private const string EmptyStar = "\u2606";
        private const string FilledStar = "\u2605";
        private const int StarFontSize = 15;
        private const float StarWidth = 18f;

        private static GUIStyle Style
        {
            get
            {
                EnsureFresh();

                return _style ??= new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = StarFontSize,
                    padding = new RectOffset(0, 0, 0, 0)
                };
            }
        }

        private static readonly EditorStyleWatch Watch = new();

        private static GUIStyle _style;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Integer)
            {
                LabeledField.Hint(position, label, AttributeNames.Usage<RateAttribute>("an int"));
                return;
            }

            RateAttribute rate = (RateAttribute)attribute;

            EditorGUI.BeginProperty(position, label, property);

            Rect row = LabeledField.Prefix(position, label);
            int value = Mathf.Clamp(property.intValue, rate.Min, rate.Max);

            using (new NoIndentScope())
                DrawStars(row, property, rate, value);

            EditorGUI.EndProperty();
        }

        private static void DrawStars(Rect row, SerializedProperty property, RateAttribute rate, int value)
        {
            for (int star = rate.Min + 1; star <= rate.Max; star++)
            {
                Rect starRect = new(row.x + (star - rate.Min - 1) * StarWidth, row.y, StarWidth, row.height);

                if (!GUI.Button(starRect, star <= value
                        ? FilledStar
                        : EmptyStar, Style))
                    continue;

                // Clicking the star that is already the value clears back to the minimum, so a rating
                // can be unset without typing into a field that is not there.
                property.intValue = star == value
                    ? rate.Min
                    : star;
            }
        }

        // A GUIStyle copies its colors out of EditorStyles when it is built and does not stay
        // linked to them, so a cached one keeps the previous theme's colors after a switch.
        // Dropping it here has the next access rebuild it against the theme actually in use.
        private static void EnsureFresh()
        {
            if (!Watch.IsStale)
                return;

            _style = null;

            Watch.MarkFresh();
        }
    }
}