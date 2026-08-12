using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Collections
{
    /// <summary>
    /// Draws an array for <see cref="TableAttribute"/> as a grid, one row per element and one column per
    /// serialized field of the element type.
    /// </summary>
    /// <remarks>
    /// Columns come from the first element rather than from reflection, because only the serialized
    /// property tree knows which fields Unity actually shows. An empty table therefore has no columns to
    /// draw yet, which is why it shows only its header until the first row exists.
    /// </remarks>
    internal static class TableRenderer
    {
        private const string EmptyMessage = "Empty. Add a row to see the columns.";
        private const float HeaderPadding = 2f;
        private const string IndexHeader = "#";
        private const float IndexWidth = 26f;
        private const string UnsupportedMessage = "[Table] needs an array of a serializable type.";

        private static readonly Color HeaderBackground = new(0.5f, 0.5f, 0.5f, 0.12f);

        private static readonly List<TableColumn> Columns = new();

        /// <summary>Draws the table.</summary>
        /// <param name="property">The array property.</param>
        /// <param name="label">The label and tooltip shown in the header.</param>
        /// <param name="elementType">The element type, used to read the column attributes.</param>
        /// <param name="attribute">The settings that shape the table.</param>
        /// <param name="canResize">False when [ArraySize] fixes the element count.</param>
        public static void Draw(SerializedProperty property, GUIContent label, Type elementType,
            TableAttribute attribute, bool canResize = true)
        {
            if (ListDrawerState.IsFirstDraw(property))
                property.isExpanded = attribute.DefaultExpanded;

            EditorGUILayout.BeginHorizontal();

            GUIContent header = new($"{label.text} ({property.arraySize})", label.tooltip);
            property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, header, true);

            EditorGUILayout.EndHorizontal();

            if (!property.isExpanded)
                return;

            if (property.arraySize == 0)
            {
                EditorGUILayout.LabelField(EmptyMessage, EditorStyles.centeredGreyMiniLabel);
                DrawFooter(property, attribute, canResize);
                return;
            }

            CollectColumns(property.GetArrayElementAtIndex(0), elementType);

            if (Columns.Count == 0)
            {
                EditorGUILayout.LabelField(UnsupportedMessage, EditorStyles.centeredGreyMiniLabel);
                return;
            }

            DrawHeaderRow(attribute, canResize);
            DrawRows(property, attribute, canResize);
            DrawFooter(property, attribute, canResize);
        }

        private static void CollectColumns(SerializedProperty element, Type elementType)
        {
            Columns.Clear();

            if (element.propertyType != SerializedPropertyType.Generic)
                return;

            SerializedProperty iterator = element.Copy();
            SerializedProperty end = element.GetEndProperty();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
            {
                enterChildren = false;

                TableColumnAttribute settings = ResolveSettings(elementType, iterator.name);
                if (settings != null && settings.Hidden)
                    continue;

                string header = settings?.Header ?? ObjectNames.NicifyVariableName(iterator.name);
                float weight = settings?.Weight ?? TableColumnAttribute.DefaultWeight;

                Columns.Add(new TableColumn(iterator.name, header, Mathf.Max(weight, 0.01f)));
            }
        }

        private static TableColumnAttribute ResolveSettings(Type elementType, string fieldName)
        {
            if (elementType == null)
                return null;

            FieldInfo field = ReflectionCache.GetField(elementType, fieldName);

            return field == null
                ? null
                : ReflectionCache.GetAttribute<TableColumnAttribute>(field);
        }

        // The same add and remove pair Unity puts under a list, for the same reasons: it works on an
        // empty table, it sits where people look for it, and it cannot collide with the header row.
        private static void DrawFooter(SerializedProperty property, TableAttribute attribute, bool canResize)
        {
            bool canAdd = canResize && !attribute.HideAddButton;
            bool canRemove = canResize && !attribute.HideRemoveButton && property.arraySize > 0;

            if (!canAdd && !canRemove)
                return;

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (canAdd && FooterButton(CollectionGui.AddLabel))
                property.arraySize++;

            if (canRemove && FooterButton(CollectionGui.RemoveLabel))
                CollectionGui.DeleteElement(property, property.arraySize - 1);

            EditorGUILayout.EndHorizontal();
        }

        private static bool FooterButton(string label)
        {
            Rect rect = GUILayoutUtility.GetRect(CollectionGui.ButtonWidth, CollectionGui.Line,
                GUILayout.Width(CollectionGui.ButtonWidth), GUILayout.Height(CollectionGui.Line));

            return CollectionGui.SmallButton(rect, label);
        }

        private static void DrawHeaderRow(TableAttribute attribute, bool canResize)
        {
            Rect row = EditorGUILayout.GetControlRect(false, CollectionGui.Line);
            EditorGUI.DrawRect(row, HeaderBackground);

            float x = row.x;

            if (attribute.ShowRowIndex)
            {
                GUI.Label(new Rect(x, row.y, IndexWidth, row.height), IndexHeader, EditorStyles.miniBoldLabel);
                x += IndexWidth;
            }

            float available = AvailableWidth(row.width, attribute, canResize);
            float totalWeight = TotalWeight();

            foreach (TableColumn column in Columns)
            {
                float width = available * (column.Weight / totalWeight);

                GUI.Label(new Rect(x + HeaderPadding, row.y, width - HeaderPadding, row.height), column.Header,
                    EditorStyles.miniBoldLabel);

                x += width;
            }
        }

        private static void DrawRows(SerializedProperty property, TableAttribute attribute, bool canResize)
        {
            int removeAt = -1;

            for (int i = 0; i < property.arraySize; i++)
            {
                SerializedProperty element = property.GetArrayElementAtIndex(i);

                Rect row = EditorGUILayout.GetControlRect(false, RowHeight(element));
                if (DrawRow(row, element, i, attribute, canResize))
                    removeAt = i;
            }

            if (removeAt >= 0)
                CollectionGui.DeleteElement(property, removeAt);
        }

        // Returns whether the row's remove button was pressed.
        private static bool DrawRow(Rect row, SerializedProperty element, int index, TableAttribute attribute,
            bool canResize)
        {
            // Cells are positioned from explicit rects, so the ambient indent has to be neutralized.
            using (new NoIndentScope())
            {
                float x = row.x;

                if (attribute.ShowRowIndex)
                {
                    GUI.Label(new Rect(x, row.y, IndexWidth, CollectionGui.Line), index.ToString(),
                        EditorStyles.miniLabel);

                    x += IndexWidth;
                }

                float available = AvailableWidth(row.width, attribute, canResize);
                float totalWeight = TotalWeight();

                foreach (TableColumn column in Columns)
                {
                    float width = available * (column.Weight / totalWeight);
                    SerializedProperty cell = element.FindPropertyRelative(column.PropertyName);

                    if (cell != null)
                    {
                        Rect cellRect = new(x + HeaderPadding, row.y, width - HeaderPadding * 2f,
                            EditorGUI.GetPropertyHeight(cell, true));

                        EditorGUI.PropertyField(cellRect, cell, GUIContent.none, true);
                    }

                    x += width;
                }
            }

            if (!canResize || attribute.HideRemoveButton)
                return false;

            Rect removeRect = new(row.xMax - CollectionGui.SmallButtonWidth, row.y,
                CollectionGui.SmallButtonWidth, CollectionGui.Line);

            return CollectionGui.SmallButton(removeRect, CollectionGui.RemoveLabel);
        }

        private static float AvailableWidth(float rowWidth, TableAttribute attribute, bool canResize)
        {
            float width = rowWidth;

            if (attribute.ShowRowIndex)
                width -= IndexWidth;

            if (canResize && !attribute.HideRemoveButton)
                width -= CollectionGui.SmallButtonWidth + CollectionGui.Gap;

            return Mathf.Max(width, CollectionGui.ButtonWidth);
        }

        private static float TotalWeight()
        {
            float total = 0f;

            foreach (TableColumn column in Columns)
                total += column.Weight;

            return Mathf.Max(total, 0.01f);
        }

        private static float RowHeight(SerializedProperty element)
        {
            float height = CollectionGui.Line;

            foreach (TableColumn column in Columns)
            {
                SerializedProperty cell = element.FindPropertyRelative(column.PropertyName);
                if (cell != null)
                    height = Mathf.Max(height, EditorGUI.GetPropertyHeight(cell, true));
            }

            return height;
        }
    }
}