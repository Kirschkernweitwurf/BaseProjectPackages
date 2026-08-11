using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Enables the field only while the referenced bool members satisfy the condition mode.
    /// Members are referenced by name, for example <c>[EnableIf(nameof(_flag))]</c> or
    /// <c>[EnableIf(EConditionMode.Any, nameof(_a), nameof(_b))]</c>, and may be bool fields, bool
    /// properties or parameterless methods returning bool.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class EnableIfAttribute : PropertyAttribute
    {
        /// <summary>Names of the bool members that drive the condition.</summary>
        public string[] Members { get; }

        /// <summary>How the members are combined. Defaults to <see cref="EConditionMode.All"/>.</summary>
        public EConditionMode Mode { get; }

        /// <summary>Creates the attribute requiring every given member to be true.</summary>
        /// <param name="members">Names of the bool members that drive the condition.</param>
        public EnableIfAttribute(params string[] members)
        {
            Mode = EConditionMode.All;
            Members = members;
        }

        /// <summary>Creates the attribute combining the given members with the given mode.</summary>
        /// <param name="mode">How the members are combined.</param>
        /// <param name="members">Names of the bool members that drive the condition.</param>
        public EnableIfAttribute(EConditionMode mode, params string[] members)
        {
            Mode = mode;
            Members = members;
        }
    }
}