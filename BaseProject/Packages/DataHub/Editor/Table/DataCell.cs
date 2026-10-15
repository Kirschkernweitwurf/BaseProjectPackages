using System;
using System.Globalization;
using UnityEditor;
using UnityEngine;

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
        private const string FloatFormat = "0.###";
        private const string ItemsFormat = "{0} items";
        private const string NoneText = "None";
        private const string OneItemText = "1 item";

        /// <summary>The cell as text, for display, search and sorting.</summary>
        /// <param name="property">The property of the cell.</param>
        /// <returns>The text.</returns>
        internal static string Text(SerializedProperty property)
        {
            if (property == null)
                return string.Empty;

            if (property.isArray
                && property.propertyType != SerializedPropertyType.String)
                return property.arraySize == 1
                    ? OneItemText
                    : string.Format(ItemsFormat, property.arraySize);

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
        /// <param name="cell">The cell rectangle.</param>
        /// <param name="property">The property of the cell.</param>
        /// <param name="column">The column, for its editability and field type.</param>
        /// <param name="textStyle">The style read-only text is drawn with.</param>
        /// <returns>True when the user changed the value.</returns>
        internal static bool Draw(Rect cell, SerializedProperty property, DataColumn column, GUIStyle textStyle)
        {
            if (!column.IsEditable)
            {
                GUI.Label(cell, new GUIContent(Text(property)), textStyle);
                return false;
            }

            EditorGUI.BeginChangeCheck();

            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    property.longValue = EditorGUI.DelayedIntField(cell, (int)property.longValue);
                    break;

                case SerializedPropertyType.Float:
                    property.doubleValue = EditorGUI.DelayedDoubleField(cell, property.doubleValue);
                    break;

                case SerializedPropertyType.Boolean:
                    property.boolValue = EditorGUI.Toggle(cell, property.boolValue);
                    break;

                case SerializedPropertyType.String:
                    property.stringValue = EditorGUI.DelayedTextField(cell, property.stringValue);
                    break;

                case SerializedPropertyType.Enum:
                    property.enumValueIndex = EditorGUI.Popup(cell, property.enumValueIndex,
                        property.enumDisplayNames);

                    break;

                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = EditorGUI.ObjectField(cell, property.objectReferenceValue,
                        column.FieldType, false);

                    break;

                default:
                    GUI.Label(cell, Text(property), textStyle);
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

        // An index of -1 means the stored value has no named member, which happens after an enum
        // member was removed. The raw number is still worth showing.
        private static string EnumText(SerializedProperty property)
        {
            int index = property.enumValueIndex;

            return index >= 0 && index < property.enumDisplayNames.Length
                ? property.enumDisplayNames[index]
                : property.intValue.ToString(CultureInfo.InvariantCulture);
        }

        private static string ObjectText(UnityEngine.Object value) => value == null
            ? NoneText
            : value.name;

        private static string ManagedReferenceText(SerializedProperty property)
        {
            object value = property.managedReferenceValue;

            return value == null
                ? NoneText
                : ObjectNames.NicifyVariableName(value.GetType().Name);
        }
    }
}