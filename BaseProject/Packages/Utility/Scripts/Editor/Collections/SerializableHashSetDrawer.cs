using System.Collections.Generic;
using Base.UtilityPackage.Collections;
using UnityEditor;
using UnityEngine;

namespace Base.UtilityPackage.Editor.Collections
{
    /// <summary>
    /// Draws a <see cref="SerializableHashSet{T}"/> as a flat item list. Duplicates are tinted and
    /// summarized, since the runtime set silently keeps only their first occurrence.
    /// </summary>
    [CustomPropertyDrawer(typeof(SerializableHashSet<>), true)]
    public sealed class SerializableHashSetDrawer : PropertyDrawer
    {
        private const string DuplicateMessage = "Duplicate items are ignored at runtime. Only the first wins.";
        private const string MissingItemsMessage = "Serialized item list not found.";

        // Reused across rows so the drawer allocates one list per repaint instead of one per row.
        private readonly List<SerializedProperty> _items = new();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty items = FindItems(property);
            if (items == null)
                return SerializableCollectionGui.Line;

            float height = SerializableCollectionGui.Line;
            if (!property.isExpanded)
                return height;

            Collect(items);

            for (int i = 0; i < items.arraySize; i++)
            {
                height += SerializableCollectionGui.Spacing
                    + EditorGUI.GetPropertyHeight(items.GetArrayElementAtIndex(i), true);
            }

            if (SerializableCollectionGui.FindDuplicates(_items).Count > 0)
                height += SerializableCollectionGui.Spacing + SerializableCollectionGui.Line * 2f;

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty items = FindItems(property);
            if (items == null)
            {
                LabeledField.Hint(position, label, MissingItemsMessage);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            Rect header = new(position.x, position.y, position.width, SerializableCollectionGui.Line);
            property.isExpanded = SerializableCollectionGui.DrawHeader(header, property, label, items.arraySize);

            if (SerializableCollectionGui.DrawAddButton(header))
                items.arraySize++;

            if (property.isExpanded)
                DrawRows(position, items, header.yMax);

            EditorGUI.EndProperty();
        }

        private static SerializedProperty FindItems(SerializedProperty property)
            => property.FindPropertyRelative(SerializableHashSet<int>.ItemsField);

        private void Collect(SerializedProperty items)
        {
            _items.Clear();

            for (int i = 0; i < items.arraySize; i++)
                _items.Add(items.GetArrayElementAtIndex(i));
        }

        private void DrawRows(Rect position, SerializedProperty items, float top)
        {
            Collect(items);
            HashSet<int> duplicates = SerializableCollectionGui.FindDuplicates(_items);

            float x = position.x + SerializableCollectionGui.Indent;
            float width = position.width - SerializableCollectionGui.Indent;
            float y = top;
            int removeAt = -1;

            // Rows are positioned from explicit rects, so the ambient indent has to be neutralized.
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            for (int i = 0; i < items.arraySize; i++)
            {
                SerializedProperty item = items.GetArrayElementAtIndex(i);
                y += SerializableCollectionGui.Spacing;

                Rect row = new(x, y, width, EditorGUI.GetPropertyHeight(item, true));
                if (duplicates.Contains(i))
                    SerializableCollectionGui.MarkDuplicate(row);

                float fieldWidth = row.width
                    - SerializableCollectionGui.ButtonWidth
                    - SerializableCollectionGui.Gap;

                EditorGUI.PropertyField(new Rect(row.x, row.y, fieldWidth, row.height), item, GUIContent.none, true);

                if (SerializableCollectionGui.DrawRemoveButton(row))
                    removeAt = i;

                y = row.yMax;
            }

            EditorGUI.indentLevel = indent;

            if (duplicates.Count > 0)
            {
                Rect warning = new(x, y + SerializableCollectionGui.Spacing, width,
                    SerializableCollectionGui.Line * 2f);

                EditorGUI.HelpBox(warning, DuplicateMessage, MessageType.Warning);
            }

            if (removeAt >= 0)
                SerializableCollectionGui.DeleteElement(items, removeAt);
        }
    }
}