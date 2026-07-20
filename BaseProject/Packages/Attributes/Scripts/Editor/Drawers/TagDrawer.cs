using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws a string field as a dropdown of project tags for <see cref="TagAttribute"/>. In
    /// only-existing mode a stored tag that no longer exists is kept, and a compact warning below
    /// points it out instead of silently replacing it.
    /// </summary>
    [CustomPropertyDrawer(typeof(TagAttribute))]
    public sealed class TagDrawer : PropertyDrawer
    {
        private const float WarningSpacing = 2f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
                return EditorGUIUtility.singleLineHeight;

            return IsUnknownTag(property)
                ? EditorGUIUtility.singleLineHeight + WarningSpacing + CompactHelpBox.Height
                : EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, AttributeNames.Usage<TagAttribute>("a string"));
                return;
            }

            TagAttribute tag = (TagAttribute)attribute;
            bool isUnknown = IsUnknownTag(property);

            Rect fieldRect = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            EditorGUI.BeginProperty(fieldRect, label, property);

            if (tag.OnlyExisting)
            {
                string[] tags = InternalEditorUtility.tags;
                int current = Array.IndexOf(tags, property.stringValue);
                int selected = EditorGUI.Popup(fieldRect, label.text, current, tags);
                if (selected >= 0 && selected < tags.Length && selected != current)
                    property.stringValue = tags[selected];
            }
            else
            {
                property.stringValue = EditorGUI.TagField(fieldRect, label, property.stringValue);
            }

            EditorGUI.EndProperty();

            if (!isUnknown)
                return;

            Rect warningRect = new(position.x, fieldRect.yMax + WarningSpacing, position.width,
                CompactHelpBox.Height);

            CompactHelpBox.Draw(warningRect, $"Tag '{property.stringValue}' does not exist.",
                EInfoBoxType.Warning);
        }

        private bool IsUnknownTag(SerializedProperty property)
        {
            if (!((TagAttribute)attribute).OnlyExisting || string.IsNullOrEmpty(property.stringValue))
                return false;

            return Array.IndexOf(InternalEditorUtility.tags, property.stringValue) < 0;
        }
    }
}