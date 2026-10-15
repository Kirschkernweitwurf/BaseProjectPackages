using System;
using System.Collections.Generic;
using Base.DataHubPackage.Editor.Table;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.BulkEdit
{
    /// <summary>
    /// Works out and applies a bulk edit of one number column: add, multiply or set, rounded for whole
    /// number fields and kept inside the field's <c>[Min]</c> and <c>[Range]</c>, so an edit can never write a
    /// value the inspector would not allow either.
    /// </summary>
    internal static class DataBulkEdit
    {
        private const string UndoFormat = "Adjust {0}";

        /// <summary>The value a row gets.</summary>
        /// <param name="column">The number column.</param>
        /// <param name="mode">How to change it.</param>
        /// <param name="current">The value now.</param>
        /// <param name="value">The value typed into the edit.</param>
        /// <param name="isClamped">True when a limit of the field cut the result.</param>
        /// <param name="isRounded">True when the result was rounded to a whole number.</param>
        /// <returns>The new value.</returns>
        internal static double Compute(DataColumn column, EDataBulkMode mode, double current, double value,
            out bool isClamped, out bool isRounded)
        {
            double result = mode switch
            {
                EDataBulkMode.Add => current + value,
                EDataBulkMode.Multiply => current * value,
                _ => value
            };

            isRounded = false;

            if (column.IsWholeNumber)
            {
                double rounded = Math.Round(result, MidpointRounding.AwayFromZero);
                isRounded = rounded != result;
                result = rounded;
            }

            double clamped = Clamp(result, column);
            isClamped = clamped != result;

            return clamped;
        }

        /// <summary>Reads the column's value from every target, for the preview and the edit.</summary>
        /// <param name="targets">The assets.</param>
        /// <param name="column">The number column.</param>
        /// <param name="values">Receives one value per target. Cleared first.</param>
        internal static void Read(IReadOnlyList<Object> targets, DataColumn column, List<double> values)
        {
            values.Clear();

            foreach (Object target in targets)
            {
                using SerializedObject serialized = new(target);
                SerializedProperty property = serialized.FindProperty(column.PropertyPath);

                values.Add(property == null
                    ? 0d
                    : ReadNumber(property));
            }
        }

        /// <summary>Applies the edit to every target as one undo step.</summary>
        /// <param name="targets">The assets.</param>
        /// <param name="column">The number column.</param>
        /// <param name="mode">How to change it.</param>
        /// <param name="value">The value typed into the edit.</param>
        /// <returns>How many assets changed.</returns>
        internal static int Apply(IReadOnlyList<Object> targets, DataColumn column, EDataBulkMode mode, double value)
        {
            int group = Undo.GetCurrentGroup();
            int changed = 0;

            foreach (Object target in targets)
            {
                using SerializedObject serialized = new(target);
                SerializedProperty property = serialized.FindProperty(column.PropertyPath);

                if (property == null)
                    continue;

                double current = ReadNumber(property);
                double next = Compute(column, mode, current, value, out _, out _);

                if (next == current)
                    continue;

                WriteNumber(property, next);
                serialized.ApplyModifiedProperties();
                changed++;
            }

            Undo.SetCurrentGroupName(string.Format(UndoFormat, column.Header));
            Undo.CollapseUndoOperations(group);

            return changed;
        }

        private static double Clamp(double value, DataColumn column)
        {
            if (column.MinValue.HasValue
                && value < column.MinValue.Value)
                return column.MinValue.Value;

            if (column.MaxValue.HasValue
                && value > column.MaxValue.Value)
                return column.MaxValue.Value;

            return value;
        }

        private static double ReadNumber(SerializedProperty property) => property.propertyType switch
        {
            SerializedPropertyType.Integer => property.longValue,
            SerializedPropertyType.Float when property.numericType == SerializedPropertyNumericType.Float
                => property.floatValue,
            _ => property.doubleValue
        };

        private static void WriteNumber(SerializedProperty property, double value)
        {
            if (property.propertyType == SerializedPropertyType.Integer)
            {
                property.longValue = (long)value;
                return;
            }

            if (property.numericType == SerializedPropertyNumericType.Float)
            {
                property.floatValue = (float)value;
                return;
            }

            property.doubleValue = value;
        }
    }
}