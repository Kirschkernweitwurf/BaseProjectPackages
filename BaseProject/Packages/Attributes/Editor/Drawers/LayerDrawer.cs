using System;
using Base.UtilityPackage.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Base.AttributePackage.Editor.Drawers
{
    /// <summary>
    /// Draws a single layer picker for <see cref="LayerAttribute"/>. An int field stores the layer
    /// index, a string field the layer name. A stored name that no longer exists is kept and reported
    /// instead of being silently replaced.
    /// </summary>
    [CustomPropertyDrawer(typeof(LayerAttribute))]
    internal sealed class LayerDrawer : WarningFieldDrawer
    {
        protected override string UsageMessage => AttributeNames.Usage<LayerAttribute>("a string or int");

        protected override bool IsSupported(SerializedProperty property)
            => property.propertyType == SerializedPropertyType.String
                || property.propertyType == SerializedPropertyType.Integer;

        protected override string Evaluate(SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.String
                || string.IsNullOrEmpty(property.stringValue))
                return null;

            return Array.IndexOf(InternalEditorUtility.layers, property.stringValue) < 0
                ? $"Layer '{property.stringValue}' does not exist."
                : null;
        }

        protected override void DrawField(Rect rect, SerializedProperty property, GUIContent label, bool complete)
        {
            if (property.propertyType == SerializedPropertyType.Integer)
            {
                property.intValue = EditorGUI.LayerField(rect, label, property.intValue);
                return;
            }

            string[] layers = InternalEditorUtility.layers;
            int current = Array.IndexOf(layers, property.stringValue);
            int selected = LabeledField.Popup(rect, label, current, layers);

            if (selected >= 0 && selected < layers.Length && selected != current)
                property.stringValue = layers[selected];
        }
    }
}