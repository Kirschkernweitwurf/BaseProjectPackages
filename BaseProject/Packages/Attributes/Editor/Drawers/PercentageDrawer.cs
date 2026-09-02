using Base.AttributesPackage.Editor.Core;
using Base.EditorUIPackage.Editor;
using Base.UtilityPackage.Editor;
using UnityEditor;
using UnityEngine;

namespace Base.AttributesPackage.Editor.Drawers
{
    /// <summary>
    /// Draws a normalized float as a percentage for <see cref="PercentageAttribute"/>. The value is
    /// shown and edited in the zero to one hundred range with a trailing percent sign, while it stays
    /// stored as zero to one. The field keeps its label so the value stays drag editable, and the sign
    /// is drawn at indent level zero so it is not pushed off screen inside indented or foldout sections.
    /// </summary>
    [CustomPropertyDrawer(typeof(PercentageAttribute))]
    internal sealed class PercentageDrawer : PropertyDrawer
    {
        private const float FullPercent = 100f;
        private const float Gap = 2f;
        private const string SignText = "%";
        private const float SignWidth = 16f;
        private const float ValueWidth = 50f;

        private static GUIStyle SignStyle
        {
            get
            {
                EnsureFresh();

                return _signStyle ??= new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleLeft
                };
            }
        }

        private static readonly EditorStyleWatch Watch = new();

        private static GUIStyle _signStyle;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Float)
            {
                LabeledField.Hint(position, label, AttributeNames.Usage<PercentageAttribute>("a float"));
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

            float percent = Mathf.Clamp01(property.floatValue) * FullPercent;

            // The label is passed to the field on purpose: hovering it gives the drag to scrub cursor.
            float edited = percentage.Slider
                ? EditorGUI.Slider(controlRect, label, percent, 0f, FullPercent)
                : EditorGUI.FloatField(controlRect, label, percent);

            // The sign sits in a rect worked out here, so the indent must not be applied to it again,
            // and GUI.Label is used because the LabelField overload taking a string and a style reserves
            // the label width for an empty prefix and would push the sign out of its own rect.
            using (new NoIndentScope())
                GUI.Label(signRect, SignText, SignStyle);

            if (EditorGUI.EndChangeCheck())
                property.floatValue = Mathf.Clamp01(edited / FullPercent);

            EditorGUI.EndProperty();
        }

        // A GUIStyle copies its colors out of EditorStyles when it is built and does not stay
        // linked to them, so a cached one keeps the previous theme's colors after a switch.
        // Dropping it here has the next access rebuild it against the theme actually in use.
        private static void EnsureFresh()
        {
            if (!Watch.IsStale)
                return;

            _signStyle = null;

            Watch.MarkFresh();
        }
    }
}