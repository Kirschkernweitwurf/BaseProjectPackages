using System;
using System.Collections.Generic;
using Base.AttributesPackage.Editor.Core;
using Base.UtilityPackage.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributesPackage.Editor.Drawers
{
    /// <summary>
    /// Draws a string field as an object picker that stores a Resources-relative path for
    /// <see cref="ResourcesAssetAttribute"/>.
    /// </summary>
    /// <remarks>
    /// The picker offers every asset of the type, because Unity's object picker cannot be told to show
    /// only what lives under a Resources folder. An asset from anywhere else therefore has to be refused
    /// after the fact, and refusing it silently is what made the field look broken: the picker closed,
    /// nothing changed, and nothing said why. The refusal is reported until the next valid pick.
    /// </remarks>
    [CustomPropertyDrawer(typeof(ResourcesAssetAttribute))]
    internal sealed class ResourcesAssetDrawer : PropertyDrawer
    {
        private const float Spacing = 2f;
        private const string WarningFormat = "{0} is not under a Resources folder, so it cannot be loaded "
            + "by path. It was not assigned.";

        // Keyed by property path so two fields on one object report their own refusal rather than
        // sharing one. Cleared as soon as that field accepts something.
        private static readonly Dictionary<string, string> Rejected = new();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;

            if (!Rejected.ContainsKey(property.propertyPath))
                return height;

            return height + Spacing + CompactHelpBox.Height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                LabeledField.Hint(position, label, AttributeNames.Usage<ResourcesAssetAttribute>("a string"));
                return;
            }

            ResourcesAssetAttribute attribute = (ResourcesAssetAttribute)this.attribute;
            Type type = attribute.Type ?? typeof(Object);

            Object current = string.IsNullOrEmpty(property.stringValue)
                ? null
                : Resources.Load(property.stringValue, type);

            Rect row = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();

            Object picked = EditorGUI.ObjectField(row, label, current, type, false);

            if (EditorGUI.EndChangeCheck())
                Assign(property, picked);

            EditorGUI.EndProperty();

            if (!Rejected.ContainsKey(property.propertyPath))
                return;

            Rect box = new(position.x, row.yMax + Spacing, position.width,
                position.height - row.height - Spacing);

            CompactHelpBox.Draw(box, Warning(property), EInfoBoxType.Warning);
        }

        private static void Assign(SerializedProperty property, Object picked)
        {
            if (picked == null)
            {
                Rejected.Remove(property.propertyPath);
                property.stringValue = string.Empty;

                return;
            }

            string resourcesPath = PathUtility.ToResourcesPath(AssetDatabase.GetAssetPath(picked));

            if (string.IsNullOrEmpty(resourcesPath))
            {
                Rejected[property.propertyPath] = picked.name;
                return;
            }

            Rejected.Remove(property.propertyPath);
            property.stringValue = resourcesPath;
        }

        private static string Warning(SerializedProperty property)
            => string.Format(WarningFormat, Rejected[property.propertyPath]);
    }
}