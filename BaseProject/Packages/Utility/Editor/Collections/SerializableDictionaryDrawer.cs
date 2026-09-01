using System.Collections.Generic;
using Base.UtilityPackage.Collections;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Base.UtilityPackage.Editor.Collections
{
    /// <summary>
    /// Draws a <see cref="SerializableDictionary{TKey,TValue}"/> as key-value rows on Unity's own
    /// reorderable list, instead of the nested entry list Unity would show by default. Duplicate keys
    /// are tinted and summarized, since the runtime dictionary silently keeps only their first
    /// occurrence.
    /// </summary>
    /// <remarks>
    /// The rows sit on the standard list because the entries are an array underneath and there was never
    /// a reason for them to look like anything else. Drawing them by hand meant a second, slightly
    /// different version of selection, dragging, the add and remove buttons and the empty state.
    /// </remarks>
    [CustomPropertyDrawer(typeof(SerializableDictionary<,>), true)]
    public sealed class SerializableDictionaryDrawer : PropertyDrawer
    {
        private const string DuplicateMessage = "Duplicate keys are ignored at runtime. Only the first wins.";
        private const float FoldoutInset = 12f;
        private const float KeyWeight = 0.4f;
        private const string MissingEntriesMessage = "Serialized entry list not found.";

        // Reused across rows so the drawer allocates one list per repaint instead of one per row.
        private readonly List<SerializedProperty> _keys = new();

        private HashSet<int> _duplicates = new();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty entries = FindEntries(property);
            if (entries == null)
                return SerializableCollectionGui.Line;

            if (!property.isExpanded)
                return SerializableCollectionGui.Line;

            CollectKeys(entries);

            float height = SerializableCollectionGui.Line
                + SerializableCollectionGui.Spacing
                + ListFor(entries).GetHeight();

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
            property.isExpanded = SerializableCollectionGui.DrawHeader(header, property, label,
                entries.arraySize);

            if (property.isExpanded)
                DrawList(position, entries, header.yMax);

            EditorGUI.EndProperty();
        }

        private static SerializedProperty FindEntries(SerializedProperty property)
            => property.FindPropertyRelative(SerializableDictionary<int, int>.EntriesField);

        private static SerializedProperty KeyOf(SerializedProperty entry)
            => entry.FindPropertyRelative(SerializableDictionaryEntry<int, int>.KeyField);

        private static SerializedProperty ValueOf(SerializedProperty entry)
            => entry.FindPropertyRelative(SerializableDictionaryEntry<int, int>.ValueField);

        private static float HeightOf(SerializedProperty property) => property == null
            ? SerializableCollectionGui.Line
            : EditorGUI.GetPropertyHeight(property, true);

        // The key and the value sit side by side, so the row is as tall as the taller of the two. Asking
        // the entry for its own height sums them instead, which is what left a nested value overlapping
        // the rows under it.
        private static float RowHeight(SerializedProperty entry)
            => Mathf.Max(HeightOf(KeyOf(entry)), HeightOf(ValueOf(entry)));

        private ReorderableList ListFor(SerializedProperty entries)
            => SerializableListCache.Get(entries, DrawRow, RowHeight);

        // The duplicate set is rebuilt once per draw and read here, because the row callback is handed
        // an entry rather than its index and cannot work out on its own whether the key repeats.
        private void DrawRow(Rect row, SerializedProperty entry)
        {
            if (_duplicates.Contains(entry.propertyPath.GetHashCode()))
                SerializableCollectionGui.MarkDuplicate(row);

            float available = row.width - SerializableCollectionGui.Gap;
            float keyWidth = available * KeyWeight;
            float valueWidth = available - keyWidth;

            SerializedProperty key = KeyOf(entry);
            SerializedProperty value = ValueOf(entry);

            // Each column keeps its own height, so a one-line key beside a nested value does not get
            // stretched to match it.
            if (key != null)
                EditorGUI.PropertyField(new Rect(row.x, row.y, keyWidth, HeightOf(key)), key,
                    GUIContent.none, true);

            if (value == null)
                return;

            // A value that can expand draws its own foldout arrow at the left edge of its rect, which is
            // where the key column ends. The inset gives the arrow room of its own instead of letting it
            // sit on top of the field beside it.
            float inset = value.hasVisibleChildren
                ? FoldoutInset
                : 0f;

            Rect valueRect = new(row.x + keyWidth + SerializableCollectionGui.Gap + inset, row.y,
                valueWidth - inset, HeightOf(value));

            EditorGUI.PropertyField(valueRect, value, GUIContent.none, true);
        }

        private void CollectKeys(SerializedProperty entries)
        {
            _keys.Clear();

            for (int i = 0; i < entries.arraySize; i++)
                _keys.Add(KeyOf(entries.GetArrayElementAtIndex(i)));

            HashSet<int> indices = SerializableCollectionGui.FindDuplicates(_keys);
            _duplicates = new HashSet<int>();

            foreach (int index in indices)
                _duplicates.Add(entries.GetArrayElementAtIndex(index).propertyPath.GetHashCode());
        }

        private void DrawList(Rect position, SerializedProperty entries, float top)
        {
            CollectKeys(entries);

            ReorderableList list = ListFor(entries);
            Rect area = new(position.x, top + SerializableCollectionGui.Spacing, position.width,
                list.GetHeight());

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            list.DoList(area);

            EditorGUI.indentLevel = indent;

            if (_keys.Count == 0 || _duplicates.Count == 0)
                return;

            Rect warning = new(position.x, area.yMax + SerializableCollectionGui.Spacing, position.width,
                SerializableCollectionGui.Line * 2f);

            EditorGUI.HelpBox(warning, DuplicateMessage, MessageType.Warning);
        }
    }
}