using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws a string field as a dropdown of project tags for <see cref="TagAttribute"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(TagAttribute))]
    public sealed class TagDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, AttributeNames.Usage<TagAttribute>("a string"));
                return;
            }

            TagAttribute tag = (TagAttribute)attribute;

            EditorGUI.BeginProperty(position, label, property);

            if (tag.OnlyExisting)
            {
                string[] tags = InternalEditorUtility.tags;
                int current = Array.IndexOf(tags, property.stringValue);
                int selected = EditorGUI.Popup(position, label.text, current, tags);
                if (selected >= 0 && selected < tags.Length)
                    property.stringValue = tags[selected];
            }
            else
            {
                property.stringValue = EditorGUI.TagField(position, label, property.stringValue);
            }

            EditorGUI.EndProperty();
        }
    }
}