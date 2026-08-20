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

        // The SerializedObject a cached list was built against, kept so staleness can be judged without
        // touching the cached property. A SerializedObject is disposed when the inspector rebuilds, and
        // every member of a property belonging to it throws from that moment on, including the equality
        // check that would otherwise be the obvious way to ask whether the cache still applies.
        private static readonly Dictionary<ReorderableList, SerializedObject> Owners = new();

        private static readonly Dictionary<ReorderableList, Action<Rect, SerializedProperty>> Rows = new();

        // Supplied by the caller, because only it knows whether an entry is one control or two side by
        // side, and the height of a row of two is the taller one rather than the sum.
        private static readonly Dictionary<ReorderableList, Func<SerializedProperty, float>> Heights = new();

        static SerializableListCache() => AssemblyReloadEvents.beforeAssemblyReload += Drop;

        /// <summary>Returns the list for the given array, building it on first use.</summary>
        /// <param name="entries">The serialized entry array.</param>
        /// <param name="drawRow">Draws one entry into the rect it is given.</param>
        /// <returns>The cached list, ready to draw.</returns>
        internal static ReorderableList Get(SerializedProperty entries,
            Action<Rect, SerializedProperty> drawRow, Func<SerializedProperty, float> rowHeight = null)
        {
            string key = entries.serializedObject.targetObject.GetInstanceID() + entries.propertyPath;

            if (Lists.TryGetValue(key, out ReorderableList cached)
                && Owners.TryGetValue(cached, out SerializedObject owner)
                && ReferenceEquals(owner, entries.serializedObject))
            {
                Rows[cached] = drawRow;
                Heights[cached] = rowHeight;
                return cached;
            }

            ReorderableList created = Build(entries);

            Owners[created] = entries.serializedObject;
            Rows[created] = drawRow;
            Heights[created] = rowHeight;
            Lists[key] = created;

            return created;
        }

        private static void Drop()
        {
            Heights.Clear();
            Lists.Clear();
            Owners.Clear();
            Rows.Clear();
        }

        private static ReorderableList Build(SerializedProperty entries)
        {
            ReorderableList list = new(entries.serializedObject, entries.Copy(), true, false, true, true)
            {
                headerHeight = 0f
            };

            list.elementHeightCallback = index =>
            {
                SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);

                float height = Heights.TryGetValue(list, out Func<SerializedProperty, float> measure)
                    && measure != null
                        ? measure(element)
                        : EditorGUI.GetPropertyHeight(element, true);

                return height + RowPadding;
            };

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