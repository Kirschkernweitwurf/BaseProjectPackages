using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.Lists
{
    /// <summary>
    /// Reads and changes an asset list as counts: how often is this card in the deck, add three, remove
    /// one, set it to five. Every change goes through the serialized object, so it is one undo step and
    /// marks the asset dirty like an edit in the inspector.
    /// </summary>
    internal static class DataListEditing
    {
        private const string UndoFormat = "Change {0}";

        /// <summary>How often <paramref name="item"/> is in the list.</summary>
        /// <param name="list">The list property.</param>
        /// <param name="item">The asset to count.</param>
        /// <returns>The number of entries that point to it.</returns>
        internal static int Count(SerializedProperty list, Object item)
        {
            int count = 0;

            for (int i = 0; i < list.arraySize; i++)
            {
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == item)
                    count++;
            }

            return count;
        }

        /// <summary>
        /// Fills <paramref name="groups"/> with every distinct entry, in the order they first appear.
        /// </summary>
        /// <param name="list">The list property.</param>
        /// <param name="groups">The list the groups are written to. Cleared first.</param>
        internal static void Group(SerializedProperty list, List<DataListGroup> groups)
        {
            groups.Clear();

            List<Object> order = new();
            Dictionary<Object, int> counts = new();
            int emptyCount = 0;

            for (int i = 0; i < list.arraySize; i++)
            {
                Object item = list.GetArrayElementAtIndex(i).objectReferenceValue;

                // A dictionary cannot take null as a key, and an empty slot is worth showing on its own.
                if (item == null)
                {
                    emptyCount++;
                    continue;
                }

                if (counts.TryGetValue(item, out int count))
                {
                    counts[item] = count + 1;
                    continue;
                }

                counts.Add(item, 1);
                order.Add(item);
            }

            foreach (Object item in order)
                groups.Add(new DataListGroup(item, counts[item]));

            if (emptyCount > 0)
                groups.Add(new DataListGroup(null, emptyCount));
        }

        /// <summary>
        /// Changes how often <paramref name="item"/> is in the list to <paramref name="target"/>. New entries
        /// go to the end, removed ones come off the end first, so the order of everything else stays.
        /// </summary>
        /// <param name="serialized">The serialized owner of the list.</param>
        /// <param name="field">The list.</param>
        /// <param name="item">The asset, or null for empty slots.</param>
        /// <param name="target">The count wanted, clamped at zero.</param>
        internal static void SetCount(SerializedObject serialized, DataListField field, Object item, int target)
        {
            serialized.Update();

            SerializedProperty list = serialized.FindProperty(field.PropertyPath);

            if (list == null)
                return;

            int difference = Mathf.Max(0, target) - Count(list, item);

            if (difference == 0)
                return;

            if (difference > 0)
                Append(list, item, difference);
            else
                RemoveFromEnd(list, item, -difference);

            Undo.SetCurrentGroupName(string.Format(UndoFormat, field.Label));
            serialized.ApplyModifiedProperties();
        }

        /// <summary>Changes how often <paramref name="item"/> is in the list by <paramref name="amount"/>.</summary>
        /// <param name="serialized">The serialized owner of the list.</param>
        /// <param name="field">The list.</param>
        /// <param name="item">The asset, or null for empty slots.</param>
        /// <param name="amount">How many to add, negative to remove.</param>
        internal static void Change(SerializedObject serialized, DataListField field, Object item, int amount)
        {
            serialized.Update();

            SerializedProperty list = serialized.FindProperty(field.PropertyPath);

            if (list == null)
                return;

            SetCount(serialized, field, item, Count(list, item) + amount);
        }

        private static void Append(SerializedProperty list, Object item, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                list.arraySize++;
                list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = item;
            }
        }

        private static void RemoveFromEnd(SerializedProperty list, Object item, int amount)
        {
            for (int i = list.arraySize - 1; i >= 0 && amount > 0; i--)
            {
                if (list.GetArrayElementAtIndex(i).objectReferenceValue != item)
                    continue;

                list.DeleteArrayElementAtIndex(i);
                amount--;
            }
        }
    }
}