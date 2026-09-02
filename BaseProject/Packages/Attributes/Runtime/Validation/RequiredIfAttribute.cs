using System;
using UnityEngine;

namespace Base.AttributesPackage
{
    /// <summary>
    /// Marks an object reference as required only while the referenced bool members satisfy the
    /// condition mode, for example <c>[RequiredIf(nameof(_usesCustomIcon))]</c>. Use this instead of
    /// <see cref="RequiredAttribute"/> when a reference is mandatory in one setup and meaningless in
    /// another, so the error box does not fire on configurations that never use the field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class RequiredIfAttribute : PropertyAttribute
    {
        /// <summary>Reason text used when no custom message is set. Shared by the rule and the handler.</summary>
        public const string DefaultReason = "is required in this configuration";

        /// <summary>Names of the bool members that drive the condition.</summary>
        public string[] Members { get; }

        /// <summary>How the members are combined. Defaults to <see cref="EConditionMode.All"/>.</summary>
        public EConditionMode Mode { get; }

        /// <summary>Optional custom message. Null uses a default message.</summary>
        public string Message { get; set; }

        /// <summary>Creates the attribute requiring every given member to be true.</summary>
        /// <param name="members">Names of the bool members that drive the condition.</param>
        public RequiredIfAttribute(params string[] members)
        {
            Mode = EConditionMode.All;
            Members = members;
        }

        /// <summary>Creates the attribute combining the given members with the given mode.</summary>
        /// <param name="mode">How the members are combined.</param>
        /// <param name="members">Names of the bool members that drive the condition.</param>
        public RequiredIfAttribute(EConditionMode mode, params string[] members)
        {
            Mode = mode;
            Members = members;
        }
    }
}