using System;
using UnityEngine;

namespace Base.AttributePackage.Conditional
{
    /// <summary>
    /// Hides the field while the referenced bool member is true.
    /// The member is referenced by name, for example <c>[HideIf(nameof(_flag))]</c>, and may be a
    /// bool field, a bool property or a parameterless method returning bool.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class HideIfAttribute : PropertyAttribute
    {
        /// <summary>Name of the bool member that drives the condition.</summary>
        public string Member { get; }

        /// <summary>Creates the attribute referencing the given bool member.</summary>
        public HideIfAttribute(string member) => Member = member;
    }
}