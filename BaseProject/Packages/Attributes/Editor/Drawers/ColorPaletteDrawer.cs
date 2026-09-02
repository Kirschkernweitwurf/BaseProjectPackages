using System.Collections;
using System.Collections.Generic;
using Base.AttributesPackage.Editor.Core;
using Base.UtilityPackage.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributesPackage.Editor.Drawers
{
    /// <summary>
    /// Draws a Color field as a row of swatches read from another member, for
    /// <see cref="ColorPaletteAttribute"/>. The currently selected swatch is outlined.
    /// </summary>
    [CustomPropertyDrawer(typeof(ColorPaletteAttribute))]
    internal sealed class ColorPaletteDrawer : WarningFieldDrawer
    {
        private const float BorderWidth = 2f;
        private const float OutlineBreathing = 3f;
        private const float PickerWidth = 40f;
        private const float SwatchGap = 2f;
        private const float SwatchWidth = 20f;

        protected override string UsageMessage => AttributeNames.Usage<ColorPaletteAttribute>("a Color");

        // The selected swatch is outlined by growing it, so it reaches past the row on every side. The
        // two horizontal sides have the row's own slack; the vertical ones need the room reserved here,
        // and a little beyond the outline itself, or the outline lands hard against the field above and
        // the field below instead of sitting between them.
        protected override float VerticalPadding => BorderWidth + OutlineBreathing;

        private static readonly Color Outline = Color.white;

        private readonly List<Color> _palette = new();

        protected override bool IsSupported(SerializedProperty property)
            => property.propertyType == SerializedPropertyType.Color;

        protected override string Evaluate(SerializedProperty property)
        {
            ColorPaletteAttribute settings = (ColorPaletteAttribute)attribute;
            _palette.Clear();

            Object target = property.serializedObject.targetObject;
            if (target == null)
                return null;

            if (!MemberValueResolver.TryResolve(target.GetType(), target, settings.Member, out object raw))
                return $"'{settings.Member}' does not exist on {target.GetType().Name}.";

            if (raw is not IEnumerable enumerable)
                return $"'{settings.Member}' is not an enumerable of colors.";

            foreach (object item in enumerable)
            {
                if (item is Color color)
                    _palette.Add(color);
            }

            return _palette.Count > 0
                ? null
                : $"'{settings.Member}' yielded no colors.";
        }

        protected override void DrawField(Rect rect, SerializedProperty property, GUIContent label, bool complete)
        {
            if (!complete)
            {
                EditorGUI.PropertyField(rect, property, label);
                return;
            }

            ColorPaletteAttribute settings = (ColorPaletteAttribute)attribute;
            Rect row = LabeledField.Prefix(rect, label);

            using (new NoIndentScope())
            {
                float x = row.x;

                if (settings.AllowCustom)
                {
                    Rect picker = new(x, row.y, PickerWidth, row.height);
                    property.colorValue = EditorGUI.ColorField(picker, GUIContent.none, property.colorValue);
                    x = picker.xMax + SwatchGap * 2f;
                }

                DrawSwatches(new Rect(x, row.y, row.xMax - x, row.height), property);
            }
        }

        private static bool IsSelected(Color value, Color candidate) => Mathf.Approximately(value.r, candidate.r)
            && Mathf.Approximately(value.g, candidate.g)
            && Mathf.Approximately(value.b, candidate.b)
            && Mathf.Approximately(value.a, candidate.a);

        private static Rect Grow(Rect rect, float amount) => new(rect.x - amount, rect.y - amount,
            rect.width + amount * 2f, rect.height + amount * 2f);

        private void DrawSwatches(Rect area, SerializedProperty property)
        {
            for (int i = 0; i < _palette.Count; i++)
            {
                float x = area.x + i * (SwatchWidth + SwatchGap);
                if (x + SwatchWidth > area.xMax)
                    return;

                Rect swatch = new(x, area.y, SwatchWidth, area.height);

                // The selected swatch gets an outline drawn under it, since a color cannot carry a
                // checkmark without becoming unreadable on light entries.
                if (IsSelected(property.colorValue, _palette[i]))
                    EditorGUI.DrawRect(Grow(swatch, BorderWidth), Outline);

                EditorGUI.DrawRect(swatch, _palette[i]);

                if (GUI.Button(swatch, GUIContent.none, GUIStyle.none))
                    property.colorValue = _palette[i];
            }
        }
    }
}