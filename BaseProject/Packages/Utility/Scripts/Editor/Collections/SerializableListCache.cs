using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Base.UtilityPackage.Editor.Collections
{
    /// <summary>
    /// Keeps one Unity reorderable list per drawn serializable collection, so a dictionary or a set
    /// drags, selects and resizes the way every other list in the editor does.
    /// </summary>
    /// <remarks>
    /// These collections are arrays underneath, so there was never a reason for them to look like
    /// anything else. Drawing the rows by hand meant reimplementing selection, dragging, the footer and
    /// the empty state, all slightly differently from the real thing.
    /// <para>
    /// The row drawing is supplied by the caller, because a dictionary row is two cells and a set row is
    /// one, and that is the only part the two do not share.
    /// </para>
    /// </remarks>
    internal static class SerializableListCache
    {
        private const float RowPadding = 2f;

        private static readonly Dictionary<string, ReorderableList> Lists = new();

        private static readonly Dictionary<ReorderableList, Action<Rect, SerializedProperty>> Rows = new();

        static SerializableListCache() => AssemblyReloadEvents.beforeAssemblyReload += Drop;

        /// <summary>Returns the list for the given array, building it on first use.</summary>
        /// <param name="entries">The serialized entry array.</param>
        /// <param name="drawRow">Draws one entry into the rect it is given.</param>
        /// <returns>The cached list, ready to draw.</returns>
        internal static ReorderableList Get(SerializedProperty entries,
            Action<Rect, SerializedProperty> drawRow)
        {
            string key = entries.serializedObject.targetObject.GetInstanceID() + entries.propertyPath;

            if (Lists.TryGetValue(key, out ReorderableList cached)
                && cached.serializedProperty != null
                && SerializedProperty.EqualContents(cached.serializedProperty, entries))
            {
                Rows[cached] = drawRow;
                return cached;
            }

            ReorderableList created = Build(entries);
            Rows[created] = drawRow;
            Lists[key] = created;
            return created;
        }

        private static void Drop()
        {
            Lists.Clear();
            Rows.Clear();
        }

        private static ReorderableList Build(SerializedProperty entries)
        {
            ReorderableList list = new(entries.serializedObject, entries.Copy(), true, false, true, true)
            {
                headerHeight = 0f
            };

            list.elementHeightCallback = index =>
                EditorGUI.GetPropertyHeight(list.serializedProperty.GetArrayElementAtIndex(index), true)
                + RowPadding;

            list.drawElementCallback = (rect, index, active, focused) =>
            {
                if (!Rows.TryGetValue(list, out Action<Rect, SerializedProperty> drawRow))
                    return;

                Rect row = new(rect.x, rect.y + RowPadding * 0.5f, rect.width, rect.height - RowPadding);

                drawRow(row, list.serializedProperty.GetArrayElementAtIndex(index));
            };

            return list;
        }
    }
}