using System.Collections.Generic;
using Base.UtilityPackage.Collections;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Base.UtilityPackage.Editor.Collections
{
    /// <summary>
    /// Draws a <see cref="SerializableHashSet{T}"/> on Unity's own reorderable list. Duplicate entries
    /// are tinted and summarized, since the runtime set silently keeps only their first occurrence.
    /// </summary>
    [CustomPropertyDrawer(typeof(SerializableHashSet<>), true)]
    public sealed class SerializableHashSetDrawer : PropertyDrawer
    {
        private const string DuplicateMessage = "Duplicate entries are ignored at runtime. Only the first wins.";
        private const string MissingItemsMessage = "Serialized item list not found.";

        // Reused across rows so the drawer allocates one list per repaint instead of one per row.
        private readonly List<SerializedProperty> _items = new();

        private HashSet<int> _duplicates = new();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty items = FindItems(property);
            if (items == null || !property.isExpanded)
                return SerializableCollectionGui.Line;

            Collect(items);

            float height = SerializableCollectionGui.Line + SerializableCollectionGui.Spacing
                + ListFor(items).GetHeight();

            if (_duplicates.Count > 0)
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
            property.isExpanded = SerializableCollectionGui.DrawHeader(header, property, label,
                items.arraySize);

            if (property.isExpanded)
                DrawList(position, items, header.yMax);

            EditorGUI.EndProperty();
        }

        private static SerializedProperty FindItems(SerializedProperty property)
            => property.FindPropertyRelative(SerializableHashSet<int>.ItemsField);

        private ReorderableList ListFor(SerializedProperty items)
            => SerializableListCache.Get(items, DrawRow);

        // The duplicate set is rebuilt once per draw and read here, because the row callback is handed
        // an item rather than its index and cannot work out on its own whether the value repeats.
        private void DrawRow(Rect row, SerializedProperty item)
        {
            if (_duplicates.Contains(item.propertyPath.GetHashCode()))
                SerializableCollectionGui.MarkDuplicate(row);

            EditorGUI.PropertyField(row, item, GUIContent.none, true);
        }

        private void Collect(SerializedProperty items)
        {
            _items.Clear();

            for (int i = 0; i < items.arraySize; i++)
                _items.Add(items.GetArrayElementAtIndex(i));

            HashSet<int> indices = SerializableCollectionGui.FindDuplicates(_items);
            _duplicates = new HashSet<int>();

            foreach (int index in indices)
                _duplicates.Add(items.GetArrayElementAtIndex(index).propertyPath.GetHashCode());
        }

        private void DrawList(Rect position, SerializedProperty items, float top)
        {
            Collect(items);

            ReorderableList list = ListFor(items);
            Rect area = new(position.x, top + SerializableCollectionGui.Spacing, position.width,
                list.GetHeight());

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            list.DoList(area);

            EditorGUI.indentLevel = indent;

            if (_duplicates.Count == 0)
                return;

            Rect warning = new(position.x, area.yMax + SerializableCollectionGui.Spacing, position.width,
                SerializableCollectionGui.Line * 2f);

            EditorGUI.HelpBox(warning, DuplicateMessage, MessageType.Warning);
        }
    }
}