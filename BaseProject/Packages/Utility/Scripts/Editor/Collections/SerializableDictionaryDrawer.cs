using System.Collections.Generic;
using Base.UtilityPackage.Collections;
using UnityEditor;
using UnityEngine;

namespace Base.UtilityPackage.Editor.Collections
{
    /// <summary>
    /// Draws a <see cref="SerializableDictionary{TKey,TValue}"/> as key-value rows instead of the
    /// nested entry list Unity would show by default. Duplicate keys are tinted and summarized, since
    /// the runtime dictionary silently keeps only their first occurrence.
    /// </summary>
    [CustomPropertyDrawer(typeof(SerializableDictionary<,>), true)]
    public sealed class SerializableDictionaryDrawer : PropertyDrawer
    {
        private const string DuplicateMessage = "Duplicate keys are ignored at runtime. Only the first wins.";
        private const float KeyWeight = 0.4f;
        private const string MissingEntriesMessage = "Serialized entry list not found.";

        // Reused across rows so the drawer allocates one list per repaint instead of one per row.
        private readonly List<SerializedProperty> _keys = new();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty entries = FindEntries(property);
            if (entries == null)
                return SerializableCollectionGui.Line;

            float height = SerializableCollectionGui.Line;
            if (!property.isExpanded)
                return height;

            CollectKeys(entries);

            for (int i = 0; i < entries.arraySize; i++)
                height += SerializableCollectionGui.Spacing + RowHeight(entries.GetArrayElementAtIndex(i));

            if (SerializableCollectionGui.FindDuplicates(_keys).Count > 0)
                height += SerializableCollectionGui.Spacing + SerializableCollectionGui.Line * 2f;

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty entries = FindEntries(property);
            if (entries == null)
            {
                LabeledField.Hint(position, label, MissingEntriesMessage);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            Rect header = new(position.x, position.y, position.width, SerializableCollectionGui.Line);
            property.isExpanded = SerializableCollectionGui.DrawHeader(header, property, label, entries.arraySize);

            if (SerializableCollectionGui.DrawAddButton(header))
                entries.arraySize++;

            if (property.isExpanded)
                DrawRows(position, entries, header.yMax);

            EditorGUI.EndProperty();
        }

        private static SerializedProperty FindEntries(SerializedProperty property)
            => property.FindPropertyRelative(SerializableDictionary<int, int>.EntriesField);

        private static SerializedProperty KeyOf(SerializedProperty entry)
            => entry.FindPropertyRelative(SerializableDictionaryEntry<int, int>.KeyField);

        private static SerializedProperty ValueOf(SerializedProperty entry)
            => entry.FindPropertyRelative(SerializableDictionaryEntry<int, int>.ValueField);

        private static float RowHeight(SerializedProperty entry)
        {
            float key = HeightOf(KeyOf(entry));
            float value = HeightOf(ValueOf(entry));

            return Mathf.Max(key, value);
        }

        private static float HeightOf(SerializedProperty property) => property == null
            ? SerializableCollectionGui.Line
            : EditorGUI.GetPropertyHeight(property, true);

        // Returns true when the row's remove button was pressed.
        private static bool DrawRow(Rect row, SerializedProperty entry)
        {
            float available = row.width
                - SerializableCollectionGui.ButtonWidth
                - SerializableCollectionGui.Gap * 2f;

            float keyWidth = available * KeyWeight;
            float valueWidth = available - keyWidth;

            SerializedProperty key = KeyOf(entry);
            SerializedProperty value = ValueOf(entry);

            if (key != null)
                EditorGUI.PropertyField(new Rect(row.x, row.y, keyWidth, HeightOf(key)), key, GUIContent.none, true);

            if (value != null)
            {
                Rect valueRect = new(row.x + keyWidth + SerializableCollectionGui.Gap, row.y, valueWidth,
                    HeightOf(value));

                EditorGUI.PropertyField(valueRect, value, GUIContent.none, true);
            }

            return SerializableCollectionGui.DrawRemoveButton(row);
        }

        private void CollectKeys(SerializedProperty entries)
        {
            _keys.Clear();

            for (int i = 0; i < entries.arraySize; i++)
                _keys.Add(KeyOf(entries.GetArrayElementAtIndex(i)));
        }

        private void DrawRows(Rect position, SerializedProperty entries, float top)
        {
            CollectKeys(entries);
            HashSet<int> duplicates = SerializableCollectionGui.FindDuplicates(_keys);

            float x = position.x + SerializableCollectionGui.Indent;
            float width = position.width - SerializableCollectionGui.Indent;
            float y = top;
            int removeAt = -1;

            // Rows are positioned from explicit rects, so the ambient indent has to be neutralized.
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            for (int i = 0; i < entries.arraySize; i++)
            {
                SerializedProperty entry = entries.GetArrayElementAtIndex(i);
                y += SerializableCollectionGui.Spacing;

                Rect row = new(x, y, width, RowHeight(entry));
                if (duplicates.Contains(i))
                    SerializableCollectionGui.MarkDuplicate(row);

                if (DrawRow(row, entry))
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
                SerializableCollectionGui.DeleteElement(entries, removeAt);
        }
    }
}