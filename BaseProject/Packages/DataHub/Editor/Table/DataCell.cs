using System;
using System.Globalization;
using Base.EditorUIPackage.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.Table
{
    /// <summary>
    /// Reads, draws, edits and copies one cell through its <see cref="SerializedProperty"/>, so every
    /// change in the table gets Undo, multi-edit and dirty-marking exactly like the inspector.
    /// </summary>
    internal static class DataCell
    {
        private const string BoolNo = "No";
        private const string BoolYes = "Yes";
        private const string DropdownArrow = "▾";
        private const float DropdownArrowWidth = 12f;
        private const string FloatFormat = "0.###";
        private const string ItemsFormat = "{0} items";
        private const string NoItemsText = "Empty";
        private const string NoneText = "None";
        private const string OneItemText = "1 item";
        private const string ReferenceTooltipFormat = "{0}\n\nSelect the row to pick another asset, or drop one on "
            + "the cell.";
        private const float ToggleSize = 14f;

        /// <summary>The cell as text, for display, search and sorting.</summary>
        /// <param name="property">The property of the cell.</param>
        /// <returns>The text.</returns>
        internal static string Text(SerializedProperty property)
        {
            if (property == null)
                return string.Empty;

            if (property.isArray
                && property.propertyType != SerializedPropertyType.String)
                return ItemsText(property.arraySize);

            return property.propertyType switch
            {
                SerializedPropertyType.Integer => property.longValue.ToString(CultureInfo.InvariantCulture),
                SerializedPropertyType.Float => property.doubleValue.ToString(FloatFormat,
                    CultureInfo.InvariantCulture),
                SerializedPropertyType.Boolean => property.boolValue
                    ? BoolYes
                    : BoolNo,
                SerializedPropertyType.String => property.stringValue,
                SerializedPropertyType.Enum => EnumText(property),
                SerializedPropertyType.ObjectReference => ObjectText(property.objectReferenceValue),
                SerializedPropertyType.ManagedReference => ManagedReferenceText(property),
                _ => property.type
            };
        }

        /// <summary>
        /// Draws the cell as an editor control, or as plain text when it cannot be edited in a table cell.
        /// </summary>
        /// <param name="cell">The cell rectangle, one line high.</param>
        /// <param name="property">The property of the cell.</param>
        /// <param name="column">The column, for its editability, field type and alignment.</param>
        /// <param name="isRowSelected">
        /// True on a selected row, where an asset reference becomes a real picker. Elsewhere it stays text,
        /// because a column of boxed pickers buries the names they hold.
        /// </param>
        /// <param name="styles">The built window styles.</param>
        /// <returns>True when the user changed the value.</returns>
        internal static bool Draw(Rect cell, SerializedProperty property, DataColumn column, bool isRowSelected,
            DataHubStyles styles)
        {
            if (!column.IsEditable)
            {
                DrawReadOnly(cell, property, styles);
                return false;
            }

            EditorGUI.BeginChangeCheck();

            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    property.longValue = EditorGUI.DelayedIntField(cell, (int)property.longValue, styles.CellNumber);
                    break;

                // A float goes through the float field, or 0.3 would show as 0.300000011920929.
                case SerializedPropertyType.Float when column.FieldType == typeof(float):
                    property.floatValue = EditorGUI.DelayedFloatField(cell, property.floatValue, styles.CellNumber);
                    break;

                case SerializedPropertyType.Float:
                    property.doubleValue = EditorGUI.DelayedDoubleField(cell, property.doubleValue, styles.CellNumber);
                    break;

                case SerializedPropertyType.Boolean:
                    property.boolValue = EditorGUI.Toggle(CenterToggle(cell), property.boolValue);
                    break;

                case SerializedPropertyType.String:
                    property.stringValue = EditorGUI.DelayedTextField(cell, property.stringValue, styles.CellText);
                    break;

                case SerializedPropertyType.Enum:
                    property.enumValueIndex = EditorGUI.Popup(cell, property.enumValueIndex,
                        property.enumDisplayNames, styles.CellPopup);

                    DrawDropdownArrow(cell, styles);

                    break;

                case SerializedPropertyType.ObjectReference:
                    DrawObject(cell, property, column.FieldType, isRowSelected, styles);
                    break;

                default:
                    DrawReadOnly(cell, property, styles);
                    break;
            }

            return EditorGUI.EndChangeCheck();
        }

        /// <summary>Writes the value of <paramref name="source"/> into <paramref name="target"/>.</summary>
        /// <param name="source">The property that was edited.</param>
        /// <param name="target">The property of another selected row.</param>
        internal static void Copy(SerializedProperty source, SerializedProperty target)
        {
            if (target == null
                || source.propertyType != target.propertyType)
                return;

            switch (source.propertyType)
            {
                case SerializedPropertyType.Integer:
                    target.longValue = source.longValue;
                    break;

                case SerializedPropertyType.Float:
                    target.doubleValue = source.doubleValue;
                    break;

                case SerializedPropertyType.Boolean:
                    target.boolValue = source.boolValue;
                    break;

                case SerializedPropertyType.String:
                    target.stringValue = source.stringValue;
                    break;

                case SerializedPropertyType.Enum:
                    target.enumValueIndex = source.enumValueIndex;
                    break;

                case SerializedPropertyType.ObjectReference:
                    target.objectReferenceValue = source.objectReferenceValue;
                    break;
            }
        }

        /// <summary>Orders two cell texts, as numbers when both are numbers.</summary>
        /// <param name="first">The first text.</param>
        /// <param name="second">The second text.</param>
        /// <returns>The comparison result.</returns>
        internal static int Compare(string first, string second)
        {
            if (double.TryParse(first, NumberStyles.Float, CultureInfo.InvariantCulture, out double firstNumber)
                && double.TryParse(second, NumberStyles.Float, CultureInfo.InvariantCulture, out double secondNumber))
                return firstNumber.CompareTo(secondNumber);

            return string.Compare(first, second, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Draws an asset as its icon and name, the way a row of the table shows its own asset.</summary>
        /// <param name="cell">The cell rectangle.</param>
        /// <param name="asset">The asset.</param>
        /// <param name="style">The style of the name.</param>
        /// <param name="tooltip">The tooltip of the name.</param>
        /// <returns>The rectangle of the icon, so a click on it can ping the asset.</returns>
        internal static Rect DrawAssetName(Rect cell, Object asset, GUIStyle style, string tooltip)
        {
            float size = EditorTableStyles.IconSize;
            Rect icon = new(cell.x, cell.y + (cell.height - size) * 0.5f, size, size);

            DataHubGui.Icon(icon, DataHubIcons.Of(asset));

            Rect label = new(icon.xMax + EditorTableStyles.IconGap, cell.y,
                Mathf.Max(0f, cell.xMax - icon.xMax - EditorTableStyles.IconGap), cell.height);

            GUI.Label(label, new GUIContent(asset.name, tooltip), style);

            return icon;
        }

        private static string ItemsText(int count) => count switch
        {
            0 => NoItemsText,
            1 => OneItemText,
            _ => string.Format(ItemsFormat, count)
        };

        // An index of -1 means the stored value has no named member, which happens after an enum
        // member was removed. The raw number is still worth showing.
        private static string EnumText(SerializedProperty property)
        {
            int index = property.enumValueIndex;

            return index >= 0 && index < property.enumDisplayNames.Length
                ? property.enumDisplayNames[index]
                : property.intValue.ToString(CultureInfo.InvariantCulture);
        }

        private static string ObjectText(Object value) => value == null
            ? NoneText
            : value.name;

        private static string ManagedReferenceText(SerializedProperty property)
        {
            object value = property.managedReferenceValue;

            return value == null
                ? NoneText
                : ObjectNames.NicifyVariableName(value.GetType().Name);
        }

        private static Rect CenterToggle(Rect cell) => new(cell.center.x - ToggleSize * 0.5f,
            cell.center.y - ToggleSize * 0.5f, ToggleSize, ToggleSize);

        private static void DrawReadOnly(Rect cell, SerializedProperty property, DataHubStyles styles)
            => GUI.Label(cell, Text(property), styles.Detail);

        private static void DrawDropdownArrow(Rect cell, DataHubStyles styles)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            Rect arrow = new(cell.xMax - DropdownArrowWidth, cell.y, DropdownArrowWidth, cell.height);
            GUI.Label(arrow, DropdownArrow, styles.Detail);
        }

        private static void DrawObject(Rect cell, SerializedProperty property, Type fieldType, bool isRowSelected,
            DataHubStyles styles)
        {
            if (isRowSelected)
            {
                property.objectReferenceValue = EditorGUI.ObjectField(cell, property.objectReferenceValue,
                    fieldType, false);

                return;
            }

            Object value = property.objectReferenceValue;

            if (value == null)
                GUI.Label(cell, NoneText, styles.Detail);
            else
                DrawAssetName(cell, value, styles.Name, string.Format(ReferenceTooltipFormat,
                    AssetDatabase.GetAssetPath(value)));

            // Dropping still works on a row that is not selected, the way it does on any object field.
            if (TryTakeDrop(cell, fieldType, out Object dropped))
                property.objectReferenceValue = dropped;
        }

        private static bool TryTakeDrop(Rect cell, Type fieldType, out Object dropped)
        {
            dropped = null;
            Event current = Event.current;

            if (current.type != EventType.DragUpdated
                && current.type != EventType.DragPerform)
                return false;

            if (!cell.Contains(current.mousePosition)
                || DragAndDrop.objectReferences.Length != 1)
                return false;

            Object candidate = Fit(DragAndDrop.objectReferences[0], fieldType);

            if (candidate == null)
                return false;

            DragAndDrop.visualMode = DragAndDropVisualMode.Link;

            if (current.type == EventType.DragUpdated)
            {
                current.Use();
                return false;
            }

            DragAndDrop.AcceptDrag();
            current.Use();
            dropped = candidate;

            return true;
        }

        // A prefab dropped on a component field means the component on it, as in any object field.
        private static Object Fit(Object candidate, Type fieldType)
        {
            if (fieldType.IsInstanceOfType(candidate))
                return candidate;

            if (candidate is GameObject gameObject
                && typeof(Component).IsAssignableFrom(fieldType))
                return gameObject.GetComponent(fieldType);

            return null;
        }
    }
}