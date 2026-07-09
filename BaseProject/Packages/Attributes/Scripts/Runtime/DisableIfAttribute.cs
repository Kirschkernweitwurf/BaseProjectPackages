using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Disables the field while the referenced bool member is true.
    /// The member is referenced by name, for example <c>[DisableIf(nameof(_flag))]</c>, and may be a
    /// bool field, a bool property or a parameterless method returning bool.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class DisableIfAttribute : PropertyAttribute
    {
        /// <summary>Name of the bool member that drives the condition.</summary>
        public string Member { get; }

        /// <summary>Creates the attribute referencing the given bool member.</summary>
        public DisableIfAttribute(string member) => Member = member;
    }
}
