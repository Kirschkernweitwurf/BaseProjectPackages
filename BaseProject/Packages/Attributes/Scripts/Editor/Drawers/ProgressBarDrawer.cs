using System;
using System.Reflection;
using Base.AttributePackage.ColorEnums;
using Base.AttributePackage.Editor.Core;
using Base.AttributePackage.Widgets;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor.Drawers
{
    /// <summary>
    /// Draws a numeric field as a read-only progress bar for <see cref="ProgressBarAttribute"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(ProgressBarAttribute))]
    public sealed class ProgressBarDrawer : PropertyDrawer
    {
        private static readonly Color DefaultColor = new(0.26f, 0.59f, 0.98f);
        private static GUIStyle _valueStyle;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float value;
            if (property.propertyType == SerializedPropertyType.Integer)
            {
                value = property.intValue;
            }
            else if (property.propertyType == SerializedPropertyType.Float)
            {
                value = property.floatValue;
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Use [ProgressBar] with an int or float.");
                return;
            }

            ProgressBarAttribute bar = (ProgressBarAttribute)attribute;
            float max = ResolveMax(property, bar);
            if (max <= 0f)
                max = 1f;

            float fill = Mathf.Clamp01(value / max);
            Color color = bar.PresetColor == EColor.Default
                ? DefaultColor
                : bar.PresetColor.ToColor();

            Rect barRect = EditorGUI.PrefixLabel(position, label);

            EditorGUI.DrawRect(barRect, new Color(0f, 0f, 0f, 0.25f));
            Rect fillRect = new(barRect.x, barRect.y, barRect.width * fill, barRect.height);
            EditorGUI.DrawRect(fillRect, color);

            if (_valueStyle == null)
            {
                _valueStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter
                };

                _valueStyle.normal.textColor = Color.white;
            }

            EditorGUI.LabelField(barRect, Format(value) + " / " + Format(max), _valueStyle);
        }

        private static float ResolveMax(SerializedProperty property, ProgressBarAttribute bar)
        {
            if (string.IsNullOrEmpty(bar.MaxMember))
                return bar.Max;

            Object target = property.serializedObject.targetObject;
            Type type = target.GetType();

            object value = null;
            FieldInfo field = ReflectionCache.GetField(type, bar.MaxMember);
            if (field != null)
            {
                value = field.GetValue(target);
            }
            else
            {
                PropertyInfo info = ReflectionCache.GetProperty(type, bar.MaxMember);
                if (info != null && info.CanRead)
                    value = info.GetValue(target, null);
            }

            if (value is float floatValue)
                return floatValue;

            if (value is int intValue)
                return intValue;

            return bar.Max > 0f
                ? bar.Max
                : 1f;
        }

        private static string Format(float value) => Mathf.Approximately(value, Mathf.Round(value))
            ? value.ToString("0")
            : value.ToString("0.#");
    }
}