using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Base.DataHubPackage.Editor.Table;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.Sheets
{
    /// <summary>
    /// Turns one cell into spreadsheet text and back. Numbers keep their full precision, so exporting and
    /// importing changes nothing by itself, and the reading is forgiving the way a designer types: "0,5" and
    /// "0.5", "yes" and "true", a name for an asset reference.
    /// </summary>
    internal static class DataSheetValue
    {
        private const string AmbiguousFormat = "more than one {0} is called '{1}'";
        private const char Comma = ',';
        private const char Dot = '.';
        private const string KeySeparator = "/";
        private const string NotFoundFormat = "no {0} is called '{1}'";
        private const string NotNumberFormat = "'{0}' is not a number";
        private const string NotOptionFormat = "'{0}' is not one of {1}";
        private const string NotWholeFormat = "'{0}' is not a whole number";
        private const string NotYesNoFormat = "'{0}' is not yes or no";
        private const string NoValue = "";
        private const string OptionSeparator = ", ";
        private const string PrefabFilter = "t:Prefab";
        private const string RoundTripFormat = "R";
        private const string TypeFilterFormat = "t:{0}";

        private static readonly string[] NoWords = { "no", "false", "0", "" };
        private static readonly string[] YesWords = { "yes", "true", "1", "x" };

        /// <summary>The cell as spreadsheet text.</summary>
        /// <param name="property">The property of the cell.</param>
        /// <returns>The text.</returns>
        internal static string Export(SerializedProperty property)
        {
            if (property == null)
                return NoValue;

            return property.propertyType switch
            {
                SerializedPropertyType.Float when property.numericType == SerializedPropertyNumericType.Float
                    => property.floatValue.ToString(RoundTripFormat, CultureInfo.InvariantCulture),
                SerializedPropertyType.Float => property.doubleValue.ToString(RoundTripFormat,
                    CultureInfo.InvariantCulture),
                SerializedPropertyType.ObjectReference => property.objectReferenceValue == null
                    ? NoValue
                    : property.objectReferenceValue.name,
                _ => DataCell.Text(property)
            };
        }

        /// <summary>Reads spreadsheet text into the value a property of <paramref name="column"/> takes.</summary>
        /// <param name="column">The editable column.</param>
        /// <param name="property">The property of one row, for its enum names.</param>
        /// <param name="text">The text from the sheet.</param>
        /// <param name="references">Asset references found so far, so each name is looked up once.</param>
        /// <param name="value">The value, boxed.</param>
        /// <param name="error">Why the text could not be read, or null.</param>
        /// <returns>True when the text was read.</returns>
        internal static bool TryRead(DataColumn column, SerializedProperty property, string text,
            Dictionary<string, Object> references, out object value, out string error)
        {
            value = null;
            error = null;
            text = text.Trim();

            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return TryReadWhole(text, out value, out error);

                case SerializedPropertyType.Float:
                    return TryReadNumber(text, out value, out error);

                case SerializedPropertyType.Boolean:
                    return TryReadBool(text, out value, out error);

                case SerializedPropertyType.String:
                    value = text;
                    return true;

                case SerializedPropertyType.Enum:
                    return TryReadEnum(property, text, out value, out error);

                case SerializedPropertyType.ObjectReference:
                    return TryReadReference(column.FieldType, text, references, out value, out error);

                default:
                    return false;
            }
        }

        /// <summary>The current value of a property, boxed the same way <see cref="TryRead"/> boxes it.</summary>
        /// <param name="property">The property.</param>
        /// <returns>The value.</returns>
        internal static object Current(SerializedProperty property) => property.propertyType switch
        {
            SerializedPropertyType.Integer => property.longValue,
            SerializedPropertyType.Float when property.numericType == SerializedPropertyNumericType.Float
                => (double)property.floatValue,
            SerializedPropertyType.Float => property.doubleValue,
            SerializedPropertyType.Boolean => property.boolValue,
            SerializedPropertyType.String => property.stringValue,
            SerializedPropertyType.Enum => property.enumValueIndex,
            SerializedPropertyType.ObjectReference => property.objectReferenceValue,
            _ => null
        };

        /// <summary>True when two boxed values are the same, floats compared at their stored precision.</summary>
        /// <param name="property">The property, for its number type.</param>
        /// <param name="first">The first value.</param>
        /// <param name="second">The second value.</param>
        /// <returns>True when writing the second would change nothing.</returns>
        internal static bool IsSame(SerializedProperty property, object first, object second)
        {
            if (property.propertyType == SerializedPropertyType.Float
                && property.numericType == SerializedPropertyNumericType.Float)
                return (float)(double)first == (float)(double)second;

            if (first is Object firstObject)
                return firstObject == (Object)second;

            return Equals(first, second);
        }

        /// <summary>Writes a value read by <see cref="TryRead"/> into a property.</summary>
        /// <param name="property">The property.</param>
        /// <param name="value">The value.</param>
        internal static void Write(SerializedProperty property, object value)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    property.longValue = (long)value;
                    break;

                case SerializedPropertyType.Float when property.numericType == SerializedPropertyNumericType.Float:
                    property.floatValue = (float)(double)value;
                    break;

                case SerializedPropertyType.Float:
                    property.doubleValue = (double)value;
                    break;

                case SerializedPropertyType.Boolean:
                    property.boolValue = (bool)value;
                    break;

                case SerializedPropertyType.String:
                    property.stringValue = (string)value;
                    break;

                case SerializedPropertyType.Enum:
                    property.enumValueIndex = (int)value;
                    break;

                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = (Object)value;
                    break;
            }
        }

        // German sheets write "0,5". A single comma and no dot can only mean a decimal comma.
        private static string NormalizeNumber(string text) => text.IndexOf(Dot) < 0
            && text.IndexOf(Comma) == text.LastIndexOf(Comma)
                ? text.Replace(Comma, Dot)
                : text;

        private static bool TryReadWhole(string text, out object value, out string error)
        {
            value = null;
            error = null;

            if (long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out long number))
            {
                value = number;
                return true;
            }

            error = string.Format(NotWholeFormat, text);

            return false;
        }

        private static bool TryReadNumber(string text, out object value, out string error)
        {
            value = null;
            error = null;

            if (double.TryParse(NormalizeNumber(text), NumberStyles.Float, CultureInfo.InvariantCulture,
                    out double number))
            {
                value = number;
                return true;
            }

            error = string.Format(NotNumberFormat, text);

            return false;
        }

        private static bool TryReadBool(string text, out object value, out string error)
        {
            value = null;
            error = null;

            string lower = text.ToLowerInvariant();

            if (Array.IndexOf(YesWords, lower) >= 0)
            {
                value = true;
                return true;
            }

            if (Array.IndexOf(NoWords, lower) >= 0)
            {
                value = false;
                return true;
            }

            error = string.Format(NotYesNoFormat, text);

            return false;
        }

        // The shown name or the code name, either way round, case does not matter.
        private static bool TryReadEnum(SerializedProperty property, string text, out object value, out string error)
        {
            value = null;
            error = null;

            for (int i = 0; i < property.enumNames.Length; i++)
            {
                if (string.Equals(property.enumDisplayNames[i], text, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(property.enumNames[i], text, StringComparison.OrdinalIgnoreCase))
                {
                    value = i;
                    return true;
                }
            }

            error = string.Format(NotOptionFormat, text, string.Join(OptionSeparator, property.enumDisplayNames));

            return false;
        }

        private static bool TryReadReference(Type fieldType, string text, Dictionary<string, Object> references,
            out object value, out string error)
        {
            value = null;
            error = null;

            if (text.Length == 0)
                return true;

            string key = fieldType.FullName + KeySeparator + text;

            if (references.TryGetValue(key, out Object cached))
            {
                value = cached;
                return true;
            }

            List<Object> matches = FindByName(fieldType, text);

            if (matches.Count != 1)
            {
                string format = matches.Count == 0
                    ? NotFoundFormat
                    : AmbiguousFormat;

                error = string.Format(format, ObjectNames.NicifyVariableName(fieldType.Name), text);

                return false;
            }

            references.Add(key, matches[0]);
            value = matches[0];

            return true;
        }

        // A component field takes a prefab that carries the component, the same as the object picker offers.
        private static List<Object> FindByName(Type fieldType, string name)
        {
            bool isComponent = typeof(Component).IsAssignableFrom(fieldType);

            string filter = isComponent
                ? PrefabFilter
                : string.Format(TypeFilterFormat, fieldType.Name);

            List<Object> matches = new();

            foreach (string guid in AssetDatabase.FindAssets(filter))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (!string.Equals(Path.GetFileNameWithoutExtension(path), name, StringComparison.OrdinalIgnoreCase))
                    continue;

                Object asset = isComponent
                    ? ComponentOnPrefab(path, fieldType)
                    : AssetDatabase.LoadAssetAtPath(path, fieldType);

                if (asset != null)
                    matches.Add(asset);
            }

            return matches;
        }

        private static Object ComponentOnPrefab(string path, Type componentType)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            return prefab == null
                ? null
                : prefab.GetComponent(componentType);
        }
    }
}