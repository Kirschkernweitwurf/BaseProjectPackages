using Base.AttributePackage.Editor.Core;
using Base.UtilityPackage.Editor;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Drawers
{
    /// <summary>
    /// Draws a ranged, optionally tinted curve field for <see cref="CurveRangeAttribute"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(CurveRangeAttribute))]
    internal sealed class CurveRangeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.AnimationCurve)
            {
                LabeledField.Hint(position, label,
                    AttributeNames.Usage<CurveRangeAttribute>("an AnimationCurve"));

                return;
            }

            CurveRangeAttribute range = (CurveRangeAttribute)attribute;

            Color color = range.PresetColor == EColor.Default
                ? Color.green
                : range.PresetColor.ToColor();

            Rect ranges = new(range.MinX, range.MinY, range.MaxX - range.MinX, range.MaxY - range.MinY);

            EditorGUI.BeginProperty(position, label, property);
            property.animationCurveValue =
                EditorGUI.CurveField(position, label, property.animationCurveValue, color, ranges);

            EditorGUI.EndProperty();
        }
    }
}