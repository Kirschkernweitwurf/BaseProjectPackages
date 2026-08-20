using System;
using System.Collections.Generic;
using System.Reflection;
using Base.UtilityPackage.Editor;
using UnityEditor;
using UnityEditorInternal;
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
        internal const string IndexHeader = "#";
        private const string UnsupportedMessage = "[Table] needs an array of a serializable type.";

        /// <summary>
        /// The columns of the table being drawn, rebuilt from the first element on every draw and read
        /// by <see cref="TableListCache"/> while it lays out the header and the rows.
        /// </summary>
        internal static List<TableColumn> Columns { get; } = new();

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
                property.isExpanded = true;

            EditorGUILayout.BeginHorizontal();

            GUIContent header = new($"{label.text} ({property.arraySize})", label.tooltip);
            property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, header, true);

            EditorGUILayout.EndHorizontal();

            if (!property.isExpanded)
                return;

            // An empty table has no first element to read columns from, so it draws the list without
            // any: the footer is still there, which is what the first row is added with.
            if (property.arraySize == 0)
            {
                Columns.Clear();
                EditorGUILayout.LabelField(EmptyMessage, EditorStyles.centeredGreyMiniLabel);
                DrawList(property, canResize);
                return;
            }

            CollectColumns(property.GetArrayElementAtIndex(0), elementType);

            if (Columns.Count == 0)
            {
                EditorGUILayout.LabelField(UnsupportedMessage, EditorStyles.centeredGreyMiniLabel);
                return;
            }

            DrawList(property, canResize);
        }

        private static void DrawList(SerializedProperty property, bool canResize)
        {
            ReorderableList list = TableListCache.Get(property, canResize);
            Rect rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, list.GetHeight()));

            using (new NoIndentScope())
                list.DoList(rect);
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






        internal static float TotalWeight()
        {
            float total = 0f;

            foreach (TableColumn column in Columns)
                total += column.Weight;

            return Mathf.Max(total, 0.01f);
        }

    }
}