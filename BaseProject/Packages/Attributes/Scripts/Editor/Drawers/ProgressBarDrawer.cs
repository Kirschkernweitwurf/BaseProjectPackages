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
    /// Draws a numeric field as a progress bar for <see cref="ProgressBarAttribute"/>.
    /// The bar is draggable unless the attribute is marked read-only.
    /// </summary>
    [CustomPropertyDrawer(typeof(ProgressBarAttribute))]
    public sealed class ProgressBarDrawer : PropertyDrawer
    {
        private static readonly Color DefaultColor = new(0.26f, 0.59f, 0.98f);
        private static GUIStyle _valueStyle;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!IsNumber(property))
            {
                EditorGUI.LabelField(position, label.text, "Use [ProgressBar] with an int or float.");
                return;
            }

            ProgressBarAttribute bar = (ProgressBarAttribute)attribute;
            float max = ResolveMax(property, bar);
            if (max <= 0f)
                max = 1f;

            EditorGUI.BeginProperty(position, label, property);

            Rect barRect = EditorGUI.PrefixLabel(position, label);

            if (!bar.ReadOnly)
                HandleInput(barRect, property, max);

            float value = ReadValue(property);
            float fill = Mathf.Clamp01(value / max);
            Color color = bar.PresetColor == EColor.Default
                ? DefaultColor
                : bar.PresetColor.ToColor();

            EditorGUI.DrawRect(barRect, new Color(0f, 0f, 0f, 0.25f));
            EditorGUI.DrawRect(new Rect(barRect.x, barRect.y, barRect.width * fill, barRect.height), color);

            if (_valueStyle == null)
            {
                _valueStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter
                };

                _valueStyle.normal.textColor = Color.white;
            }

            EditorGUI.LabelField(barRect, Format(value) + " / " + Format(max), _valueStyle);

            EditorGUI.EndProperty();
        }

        private static void HandleInput(Rect rect, SerializedProperty property, float max)
        {
            Event current = Event.current;
            int controlId = GUIUtility.GetControlID(FocusType.Passive);

            bool grab = current.type == EventType.MouseDown
                && current.button == 0
                && rect.Contains(current.mousePosition);

            bool dragging = GUIUtility.hotControl == controlId
                && (current.type == EventType.MouseDrag || current.type == EventType.MouseMove);

            if (grab || dragging)
            {
                GUIUtility.hotControl = controlId;

                float normalized = Mathf.Clamp01((current.mousePosition.x - rect.xMin) / rect.width);
                WriteValue(property, normalized * max, max);

                GUI.changed = true;
                current.Use();
            }
            else if (GUIUtility.hotControl == controlId && current.rawType == EventType.MouseUp)
            {
                GUIUtility.hotControl = 0;
            }
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

        private static bool IsNumber(SerializedProperty property)
            => property.propertyType == SerializedPropertyType.Integer
                || property.propertyType == SerializedPropertyType.Float;

        private static float ReadValue(SerializedProperty property)
            => property.propertyType == SerializedPropertyType.Integer
                ? property.intValue
                : property.floatValue;

        private static void WriteValue(SerializedProperty property, float value, float max)
        {
            value = Mathf.Clamp(value, 0f, max);
            if (property.propertyType == SerializedPropertyType.Integer)
                property.intValue = Mathf.RoundToInt(value);
            else
                property.floatValue = value;
        }

        private static string Format(float value) => Mathf.Approximately(value, Mathf.Round(value))
            ? value.ToString("0")
            : value.ToString("0.#");
    }
}