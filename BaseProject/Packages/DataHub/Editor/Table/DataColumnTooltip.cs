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
        internal const string Check = "Red counts problems, amber counts warnings. Hover a number to read them.";

        /// <summary>The tooltip of a computed column.</summary>
        internal const string Computed = "Worked out by the project. Read only.";

        /// <summary>The tooltip of the name column.</summary>
        internal const string Name = "The file name. Double click or press F2 to rename.";

        private const string BoolText = "Yes or no";
        private const string EnumFormat = "One of {0}";
        private const string FieldFormat = "{0}.";
        private const string FloatText = "A number";
        private const string IntText = "A whole number";
        private const string ListFormat = "A list of {0}";
        private const string ListHint = "Edit it in the Inspector or Contents tab.";
        private const string ListSeparator = ", ";
        private const string ObjectHint = "Drop an asset on a cell to change it.";
        private const string ReadOnlyHint = "Edit it in the Inspector tab.";
        private const string ReferenceFormat = "A {0}";
        private const string Separator = "\n";
        private const string TextText = "Text";
        private const string ValueFormat = "A {0}";

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

        // Only cells that don't simply edit on click need a hint.
        private static string HintFor(Type type, bool isEditable)
        {
            if (!isEditable)
            {
                return DataSerializedFields.ElementType(type) != null
                    ? ListHint
                    : ReadOnlyHint;
            }

            return typeof(Object).IsAssignableFrom(type)
                ? ObjectHint
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