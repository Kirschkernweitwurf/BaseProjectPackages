using System;
using Base.AttributePackage.Editor.Core;
using Base.UtilityPackage.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Base.AttributePackage.Editor.Drawers
{
    /// <summary>
    /// Draws a string field as a dropdown of project tags for <see cref="TagAttribute"/>. In
    /// only-existing mode a stored tag that no longer exists is kept, and a compact warning below
    /// points it out instead of silently replacing it.
    /// </summary>
    [CustomPropertyDrawer(typeof(TagAttribute))]
    internal sealed class TagDrawer : WarningFieldDrawer
    {
        protected override string UsageMessage => AttributeNames.Usage<TagAttribute>("a string");

        protected override bool IsSupported(SerializedProperty property)
            => property.propertyType == SerializedPropertyType.String;

        protected override string Evaluate(SerializedProperty property)
        {
            if (!((TagAttribute)attribute).OnlyExisting || string.IsNullOrEmpty(property.stringValue))
                return null;

            return Array.IndexOf(InternalEditorUtility.tags, property.stringValue) < 0
                ? $"Tag '{property.stringValue}' does not exist."
                : null;
        }

        protected override void DrawField(Rect rect, SerializedProperty property, GUIContent label, bool complete)
        {
            if (!((TagAttribute)attribute).OnlyExisting)
            {
                property.stringValue = EditorGUI.TagField(rect, label, property.stringValue);
                return;
            }

            string[] tags = InternalEditorUtility.tags;
            int current = Array.IndexOf(tags, property.stringValue);
            int selected = LabeledField.Popup(rect, label, current, tags);
            if (selected >= 0 && selected < tags.Length && selected != current)
                property.stringValue = tags[selected];
        }
    }
}