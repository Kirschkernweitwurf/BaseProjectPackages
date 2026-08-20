using System;
using Base.UtilityPackage.Editor;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws a sorting layer picker for <see cref="SortingLayerAttribute"/>. A string field stores the
    /// layer name, an int field the layer id used by renderer sorting. A stored value that no longer
    /// exists is kept and reported instead of being silently replaced.
    /// </summary>
    [CustomPropertyDrawer(typeof(SortingLayerAttribute))]
    internal sealed class SortingLayerDrawer : WarningFieldDrawer
    {
        protected override string UsageMessage => AttributeNames.Usage<SortingLayerAttribute>("a string or int");

        private string[] _names;

        protected override bool IsSupported(SerializedProperty property)
            => property.propertyType == SerializedPropertyType.String
                || property.propertyType == SerializedPropertyType.Integer;

        protected override string Evaluate(SerializedProperty property)
        {
            _names = CollectNames();

            if (_names.Length == 0)
                return "The project has no sorting layers.";

            if (property.propertyType == SerializedPropertyType.String)
                return string.IsNullOrEmpty(property.stringValue)
                    || Array.IndexOf(_names, property.stringValue) >= 0
                        ? null
                        : $"Sorting layer '{property.stringValue}' does not exist.";

            return CurrentIndex(property, _names) >= 0
                ? null
                : $"Sorting layer id {property.intValue} does not exist.";
        }

        protected override void DrawField(Rect rect, SerializedProperty property, GUIContent label, bool complete)
        {
            if (_names == null || _names.Length == 0)
            {
                EditorGUI.PropertyField(rect, property, label);
                return;
            }

            int current = CurrentIndex(property, _names);
            int selected = LabeledField.Popup(rect, label, current, _names);

            if (selected < 0 || selected >= _names.Length || selected == current)
                return;

            if (property.propertyType == SerializedPropertyType.String)
                property.stringValue = _names[selected];
            else
                property.intValue = SortingLayer.NameToID(_names[selected]);
        }

        private static string[] CollectNames()
        {
            SortingLayer[] layers = SortingLayer.layers;
            string[] names = new string[layers.Length];

            for (int i = 0; i < layers.Length; i++)
                names[i] = layers[i].name;

            return names;
        }

        private static int CurrentIndex(SerializedProperty property, string[] names)
        {
            if (property.propertyType == SerializedPropertyType.String)
                return Array.IndexOf(names, property.stringValue);

            int id = property.intValue;
            for (int i = 0; i < names.Length; i++)
            {
                if (SortingLayer.NameToID(names[i]) == id)
                    return i;
            }

            return -1;
        }
    }
}