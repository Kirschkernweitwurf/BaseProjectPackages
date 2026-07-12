using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Draws a dropdown of values provided by a member (field, property or parameterless method)
    /// that returns an enumerable of the field type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class DropdownAttribute : PropertyAttribute
    {
        /// <summary>Name of the member providing the options.</summary>
        public string Member { get; }

        /// <summary>Creates the attribute referencing the options member.</summary>
        public DropdownAttribute(string member) => Member = member;
    }
}
