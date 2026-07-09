using System;
using UnityEngine;

namespace Base.AttributePackage.Conditional
{
    /// <summary>
    /// Shows the field only while the referenced bool member is true.
    /// The member is referenced by name, for example <c>[ShowIf(nameof(_flag))]</c>, and may be a
    /// bool field, a bool property or a parameterless method returning bool.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ShowIfAttribute : PropertyAttribute
    {
        /// <summary>Name of the bool member that drives the condition.</summary>
        public string Member { get; }

        /// <summary>Creates the attribute referencing the given bool member.</summary>
        public ShowIfAttribute(string member) => Member = member;
    }
}