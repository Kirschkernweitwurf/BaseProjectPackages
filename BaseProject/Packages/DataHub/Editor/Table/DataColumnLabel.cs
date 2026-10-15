using System;
using UnityEditor;

namespace Base.DataHubPackage.Editor.Table
{
    /// <summary>
    /// Turns a serialized field name into a column title. An auto-property with
    /// <c>[field: SerializeField]</c> is stored as "&lt;Electrified&gt;k__BackingField", which no designer
    /// should have to read, so that wrapper is stripped once here.
    /// </summary>
    internal static class DataColumnLabel
    {
        private const string BackingFieldEnd = ">k__BackingField";
        private const char BackingFieldStart = '<';

        /// <summary>The readable title for a serialized field name.</summary>
        /// <param name="fieldName">The name as the serializer stores it.</param>
        /// <returns>The title, nicified the way the inspector does it.</returns>
        internal static string Clean(string fieldName) => ObjectNames.NicifyVariableName(StripBackingField(fieldName));

        private static string StripBackingField(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName)
                || fieldName[0] != BackingFieldStart
                || !fieldName.EndsWith(BackingFieldEnd, StringComparison.Ordinal))
                return fieldName;

            return fieldName.Substring(1, fieldName.Length - 1 - BackingFieldEnd.Length);
        }
    }
}