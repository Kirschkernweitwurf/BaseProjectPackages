using System;
using System.Collections.Generic;
using System.Reflection;
using Base.DataHubPackage.Editor.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.Table
{
    /// <summary>
    /// Works out the columns of a table from the serialized fields of its type: one column per field,
    /// nested serializable structs flattened into one column per inner field, then the computed columns.
    /// </summary>
    /// <remarks>
    /// The layout is read from the type rather than from an asset, so an abstract type gets exactly the
    /// columns it declares and an empty table still has its headers. Values are never written through
    /// reflection: every column carries a property path, and cells go through
    /// <see cref="UnityEditor.SerializedProperty"/>.
    /// </remarks>
    internal static class DataColumnBuilder
    {
        private const float BoolWidth = 56f;
        private const string CheckHeader = "Check";
        private const string CheckKey = "check";
        private const float CheckWidth = 58f;
        private const string ComputedKeyPrefix = "computed:";
        private const float ComputedWidth = 200f;
        private const float EnumWidth = 110f;
        private const int MaxNestingDepth = 3;
        private const string NameHeader = "Name";
        private const string NameKey = "name";
        private const float NameWidth = 200f;
        private const float NestedNumberWidth = 76f;
        private const float NumberWidth = 80f;
        private const float ObjectWidth = 160f;
        private const char PathSeparator = '.';
        private const float StringWidth = 180f;
        private const float SummaryWidth = 90f;

        /// <summary>The columns of a table listing <paramref name="dataType"/>.</summary>
        /// <param name="dataType">The type the table lists.</param>
        /// <param name="computed">The computed columns that apply to the type, in their order.</param>
        /// <returns>The name and check columns, the field columns, then the computed columns.</returns>
        internal static List<DataColumn> Build(Type dataType, IReadOnlyList<IDataColumn> computed)
        {
            List<DataColumn> columns = new()
            {
                new DataColumn(EDataColumnKind.Name, NameKey, NameHeader, null, NameWidth)
                {
                    Tooltip = DataColumnTooltip.Name
                },
                new DataColumn(EDataColumnKind.Check, CheckKey, CheckHeader, null, CheckWidth)
                {
                    Alignment = TextAnchor.MiddleCenter,
                    Tooltip = DataColumnTooltip.Check
                }
            };

            AddFields(dataType, null, null, 0, columns);

            foreach (IDataColumn column in computed)
            {
                columns.Add(new DataColumn(EDataColumnKind.Computed, ComputedKeyPrefix + column.GetType().FullName,
                    column.Header, null, ComputedWidth)
                {
                    Computed = column,
                    Tooltip = DataColumnTooltip.Computed
                });
            }

            return columns;
        }

        private static void AddFields(Type type, string prefix, string group, int depth, List<DataColumn> columns)
        {
            foreach (FieldInfo field in DataSerializedFields.Of(type))
            {
                string path = prefix == null
                    ? field.Name
                    : prefix + PathSeparator + field.Name;

                string label = DataColumnLabel.Clean(field.Name);
                Type fieldType = field.FieldType;

                bool isReference = Attribute.IsDefined(field, typeof(SerializeReference));

                if (!isReference
                    && depth < MaxNestingDepth
                    && IsFlattenable(fieldType))
                {
                    string innerGroup = group == null
                        ? label
                        : group + " " + label;

                    AddFields(fieldType, path, innerGroup, depth + 1, columns);
                    continue;
                }

                bool isEditable = !isReference
                    && IsEditable(fieldType);

                columns.Add(new DataColumn(EDataColumnKind.Field, path, label, group,
                    WidthFor(fieldType, isEditable, group != null))
                {
                    PropertyPath = path,
                    FieldType = fieldType,
                    IsEditable = isEditable,
                    Alignment = AlignmentFor(fieldType),
                    Tooltip = DataColumnTooltip.Field(field, isEditable),
                    MinValue = MinOf(field),
                    MaxValue = MaxOf(field)
                });
            }
        }

        private static bool IsCollection(Type type) => DataSerializedFields.ElementType(type) != null;

        private static bool IsFlattenable(Type type) => !type.IsPrimitive
            && !type.IsEnum
            && type != typeof(string)
            && !typeof(Object).IsAssignableFrom(type)
            && !IsCollection(type)
            && !DataSerializedFields.IsUnityType(type)
            && Attribute.IsDefined(type, typeof(SerializableAttribute));

        private static bool IsEditable(Type type) => DataColumn.IsNumberType(type)
            || type == typeof(bool)
            || type == typeof(string)
            || type.IsEnum
            || typeof(Object).IsAssignableFrom(type);

        private static float? MinOf(FieldInfo field)
        {
            RangeAttribute range = field.GetCustomAttribute<RangeAttribute>();

            if (range != null)
                return range.min;

            MinAttribute min = field.GetCustomAttribute<MinAttribute>();

            return min?.min;
        }

        private static float? MaxOf(FieldInfo field) => field.GetCustomAttribute<RangeAttribute>()?.max;

        private static TextAnchor AlignmentFor(Type type)
        {
            if (type == typeof(bool))
                return TextAnchor.MiddleCenter;

            return DataColumn.IsNumberType(type)
                ? TextAnchor.MiddleRight
                : TextAnchor.MiddleLeft;
        }

        private static float WidthFor(Type type, bool isEditable, bool isNested)
        {
            if (!isEditable)
                return SummaryWidth;

            if (type == typeof(bool))
                return BoolWidth;

            if (type == typeof(string))
                return StringWidth;

            if (type.IsEnum)
                return EnumWidth;

            if (typeof(Object).IsAssignableFrom(type))
                return ObjectWidth;

            return isNested
                ? NestedNumberWidth
                : NumberWidth;
        }
    }
}