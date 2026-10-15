using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.Table
{
    /// <summary>
    /// The tooltip of a column title: what the column holds and how to edit it. A field's own
    /// <see cref="TooltipAttribute"/> comes first, so what a programmer wrote for the inspector shows here too.
    /// </summary>
    internal static class DataColumnTooltip
    {
        /// <summary>The tooltip of the check column.</summary>
        internal const string Check = "Problems found in the asset: a required field left empty, a project check "
            + "that failed, or an asset missing from the collection it belongs in.\nA check mark means nothing is "
            + "wrong. A red number counts the problems, hover it to read them.";

        /// <summary>The tooltip of a computed column.</summary>
        internal const string Computed = "Worked out from the asset by the project, not stored in it. Read only.";

        /// <summary>The tooltip of the name column.</summary>
        internal const string Name = "The file name of the asset.\nClick the icon to find it in the Project window. "
            + "Double-click the name or press F2 to rename it. Drag the name onto a list or a field to use the "
            + "asset there.";

        private const string BoolText = "yes or no";
        private const string EditHint = "Click a cell to edit it. With several rows selected, the new value goes to "
            + "every one of them.";
        private const string EnumFormat = "one of: {0}";
        private const string FieldFormat = "Holds {0}.";
        private const string FloatText = "a number";
        private const string IntText = "a whole number";
        private const string ListFormat = "a list of {0}";
        private const string ListHint = "Read only here. Edit it in the Inspector tab, or in the Contents tab when it "
            + "lists assets.";
        private const string ListSeparator = ", ";
        private const string ObjectHint = "Select the row to pick an asset, or drop one on the cell.";
        private const string ReadOnlyHint = "Read only here. Edit it in the Inspector tab.";
        private const string ReferenceFormat = "a reference to a {0}";
        private const string Separator = "\n";
        private const string TextText = "text";
        private const string ValueFormat = "a {0} value";

        /// <summary>The tooltip of a field column.</summary>
        /// <param name="field">The field the column shows.</param>
        /// <param name="isEditable">True when the cells can be edited in the table.</param>
        /// <returns>The tooltip.</returns>
        internal static string Field(FieldInfo field, bool isEditable)
        {
            TooltipAttribute own = field.GetCustomAttribute<TooltipAttribute>();
            string kind = string.Format(FieldFormat, KindText(field.FieldType));
            string hint = HintFor(field.FieldType, isEditable);

            return own == null
                ? kind + Separator + hint
                : own.tooltip + Separator + kind + Separator + hint;
        }

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
                : EditHint;
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