using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Shows the field only while the referenced enum member equals one of the given values.
    /// Example: <c>[ShowIfEnum(nameof(_mood), EMood.Elektrisiert)]</c>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ShowIfEnumAttribute : PropertyAttribute
    {
        /// <summary>Name of the enum field or property that drives the condition.</summary>
        public string Member { get; }

        /// <summary>Enum values that make the field visible.</summary>
        public object[] Values { get; }

        /// <summary>Creates the attribute referencing an enum member and the values that show the field.</summary>
        public ShowIfEnumAttribute(string member, params object[] values)
        {
            Member = member;
            Values = values;
        }
    }
}