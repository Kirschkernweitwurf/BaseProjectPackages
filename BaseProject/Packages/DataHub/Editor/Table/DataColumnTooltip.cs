using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.Table
{
    /// <summary>
    /// The tooltip of a column title, kept to one short line. A field's own <see cref="TooltipAttribute"/>
    /// replaces the generic text, so what a programmer wrote for the inspector shows here too. A hint on how to
    /// edit is only added where clicking the cell is not enough.
    /// </summary>
    internal static class DataColumnTooltip
    {
        /// <summary>The tooltip of the check column.</summary>
        internal const string Check = "A quick health check for every asset.\n"
            + "A green tick means all good. A red badge counts problems to fix, an amber one things worth a look. "
            + "Hover a badge to read them.";

        /// <summary>The tooltip of the name column.</summary>
        internal const string Name = "The asset's file name, as it shows in the Project window.\n"
            + "Double click or press F2 to rename it. Click the icon to find the file.";

        private const string BoolText = "Yes or no";
        private const string ComputedHint = "Worked out from the asset for you, so it is read only.";
        private const string EnumFormat = "One of {0}";
        private const string FieldFormat = "{0}.";
        private const string FloatText = "A number";
        private const string IntText = "A whole number";
        private const string ListFormat = "A list of {0}";
        private const string ListHint = "Open the Contents tab on the right to fill it.";
        private const string ListSeparator = ", ";
        private const string NumberHint = "Click a cell to type a new value. Right click to change many rows at once.";
        private const string ObjectHint = "Drag an asset from the Project window onto a cell to swap it.";
        private const string ReadOnlyHint = "Read only here. Change it in the Inspector tab on the right.";
        private const string ReferenceFormat = "A {0}";
        private const string Separator = "\n";
        private const string TextText = "Text";
        private const string ValueFormat = "A {0}";

        /// <summary>The tooltip of a computed column: what it means, then that it is read only.</summary>
        /// <param name="own">The column's own explanation, or null.</param>
        /// <returns>The tooltip.</returns>
        internal static string Computed(string own) => string.IsNullOrEmpty(own)
            ? ComputedHint
            : own + Separator + ComputedHint;

        /// <summary>The tooltip of a field column.</summary>
        /// <param name="field">The field the column shows.</param>
        /// <param name="isEditable">True when the cells can be edited in the table.</param>
        /// <returns>The tooltip.</returns>
        internal static string Field(FieldInfo field, bool isEditable)
        {
            TooltipAttribute own = field.GetCustomAttribute<TooltipAttribute>();

            string text = own == null
                ? string.Format(FieldFormat, KindText(field.FieldType))
                : own.tooltip;

            string hint = HintFor(field.FieldType, isEditable);

            return hint == null
                ? text
                : text + Separator + hint;
        }

        // A hint where the cell needs more than a click, or where a hidden power is worth knowing about.
        private static string HintFor(Type type, bool isEditable)
        {
            if (!isEditable)
            {
                return DataSerializedFields.ElementType(type) != null
                    ? ListHint
                    : ReadOnlyHint;
            }

            if (typeof(Object).IsAssignableFrom(type))
                return ObjectHint;

            return DataColumn.IsNumberType(type)
                ? NumberHint
                : null;
        }

        private static string KindText(Type type)
        {
            Type element = DataSerializedFields.ElementType(type);

            if (element != null)
                return string.Format(ListFormat, ObjectNames.NicifyVariableName(element.Name));

            if (type == typeof(bool))
                return BoolText;

            if (type == typeof(string))
                return TextText;

            if (type == typeof(int)
                || type == typeof(long))
                return IntText;

            if (type == typeof(float)
                || type == typeof(double))
                return FloatText;

            if (type.IsEnum)
                return string.Format(EnumFormat, string.Join(ListSeparator, NicifiedNames(type)));

            string format = typeof(Object).IsAssignableFrom(type)
                ? ReferenceFormat
                : ValueFormat;

            return string.Format(format, ObjectNames.NicifyVariableName(type.Name));
        }

        private static string[] NicifiedNames(Type enumType)
        {
            string[] names = Enum.GetNames(enumType);

            for (int i = 0; i < names.Length; i++)
                names[i] = ObjectNames.NicifyVariableName(names[i]);

            return names;
        }
    }
}