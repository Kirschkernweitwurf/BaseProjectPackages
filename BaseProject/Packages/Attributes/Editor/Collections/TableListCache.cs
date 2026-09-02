using System.Collections.Generic;
using Base.UtilityPackage.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Base.AttributesPackage.Editor.Collections
{
    /// <summary>
    /// Keeps one Unity reorderable list per drawn table, so a table drags, selects and resizes exactly
    /// the way every other list in the editor does.
    /// </summary>
    /// <remarks>
    /// A table is a list whose rows happen to be laid out in columns, so it gets Unity's list rather
    /// than a second implementation of one. That is also what removes the cross on each row: the footer
    /// removes the selected entry, which is where everyone already looks for it.
    /// <para>
    /// The columns are read from the first element on every draw, because only the serialized property
    /// tree knows which fields Unity actually shows.
    /// </para>
    /// </remarks>
    internal static class TableListCache
    {
        private const float HeaderPadding = 2f;
        private const float IndexWidth = 26f;
        private const float RowPadding = 2f;

        private static List<TableColumn> TableColumns => TableRenderer.Columns;

        private static readonly Dictionary<string, ReorderableList> Lists = new();

        // The SerializedObject a cached list was built against, kept so staleness can be judged without
        // touching the cached property. A SerializedObject is disposed when the inspector rebuilds, and
        // every member of a property belonging to it throws from that moment on, including the equality
        // check that would otherwise be the obvious way to ask whether the cache still applies.
        private static readonly Dictionary<ReorderableList, SerializedObject> Owners = new();

        static TableListCache() => AssemblyReloadEvents.beforeAssemblyReload += Drop;

        /// <summary>Returns the list for the given table, building it on first use.</summary>
        /// <param name="property">The array being drawn.</param>
        /// <param name="canResize">False when [ArraySize] fixes the element count.</param>
        /// <returns>The cached list, configured for this draw.</returns>
        internal static ReorderableList Get(SerializedProperty property, bool canResize)
        {
            string key = property.serializedObject.targetObject.GetInstanceID() + property.propertyPath;

            if (Lists.TryGetValue(key, out ReorderableList cached)
                && Owners.TryGetValue(cached, out SerializedObject owner)
                && ReferenceEquals(owner, property.serializedObject))
            {
                Configure(cached, canResize);
                return cached;
            }

            ReorderableList created = Build(property, canResize);
            Owners[created] = property.serializedObject;
            Lists[key] = created;
            return created;
        }

        private static void Drop()
        {
            Lists.Clear();
            Owners.Clear();
        }

        private static ReorderableList Build(SerializedProperty property, bool canResize)
        {
            ReorderableList list = new(property.serializedObject, property.Copy(), true, true,
                canResize, canResize);

            list.drawHeaderCallback = rect => DrawHeader(rect, list);
            list.elementHeightCallback = index => RowHeight(list, index);
            list.drawElementCallback = (rect, index, active, focused) => DrawRow(rect, list, index);

            Configure(list, canResize);
            return list;
        }

        private static void Configure(ReorderableList list, bool canResize)
        {
            list.displayAdd = canResize;
            list.displayRemove = canResize;
            list.draggable = canResize;
        }


        // The header row carries the column names, laid out with the same widths the cells use, so the
        // two stay aligned however the inspector is resized.
        private static void DrawHeader(Rect rect, ReorderableList list)
        {
            if (TableColumns.Count == 0)
                return;

            GUI.Label(new Rect(rect.x, rect.y, IndexWidth, rect.height), TableRenderer.IndexHeader,
                EditorStyles.miniLabel);

            float x = rect.x + IndexWidth;
            float available = rect.width - IndexWidth;
            float total = TableRenderer.TotalWeight();

            foreach (TableColumn column in TableColumns)
            {
                float width = available * (column.Weight / total);

                GUI.Label(new Rect(x + HeaderPadding, rect.y, width - HeaderPadding * 2f, rect.height),
                    column.Header, EditorStyles.miniLabel);

                x += width;
            }
        }

        private static float RowHeight(ReorderableList list, int index)
        {
            if (index < 0 || index >= list.serializedProperty.arraySize)
                return EditorGUIUtility.singleLineHeight;

            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
            float tallest = EditorGUIUtility.singleLineHeight;

            foreach (TableColumn column in TableColumns)
            {
                SerializedProperty cell = element.FindPropertyRelative(column.PropertyName);

                if (cell != null)
                    tallest = Mathf.Max(tallest, EditorGUI.GetPropertyHeight(cell, true));
            }

            return tallest + RowPadding;
        }

        private static void DrawRow(Rect rect, ReorderableList list, int index)
        {
            if (index < 0 || index >= list.serializedProperty.arraySize)
                return;

            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);

            GUI.Label(new Rect(rect.x, rect.y, IndexWidth, EditorGUIUtility.singleLineHeight),
                index.ToString(), EditorStyles.miniLabel);

            float x = rect.x + IndexWidth;
            float available = rect.width - IndexWidth;
            float total = TableRenderer.TotalWeight();

            using (new NoIndentScope())
            {
                foreach (TableColumn column in TableColumns)
                {
                    float width = available * (column.Weight / total);
                    SerializedProperty cell = element.FindPropertyRelative(column.PropertyName);

                    if (cell != null)
                    {
                        Rect cellRect = new(x + HeaderPadding, rect.y + RowPadding * 0.5f,
                            width - HeaderPadding * 2f, EditorGUI.GetPropertyHeight(cell, true));

                        EditorGUI.PropertyField(cellRect, cell, GUIContent.none, true);
                    }

                    x += width;
                }
            }
        }
    }
}