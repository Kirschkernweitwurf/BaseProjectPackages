using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Marks an object reference field as required. Shows an error box when the reference is null.
    /// </summary>
    /// <remarks>
    /// Set <see cref="FixAction"/> to put a button in that error box. Most missing references have one
    /// obvious answer, and a box that only reports the problem makes the reader go find it by hand.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class RequiredAttribute : PropertyAttribute
    {
        /// <summary>Label used for the fix button when none is given.</summary>
        public const string DefaultFixLabel = "Fix";
        /// <summary>Reason text used when no custom message is set. Shared by the rule and the drawer.</summary>
        public const string DefaultReason = "is required";

        /// <summary>Optional custom message. Null uses a default message.</summary>
        public string Message { get; }

        /// <summary>
        /// Optional name of a parameterless method that fills the reference. When set, the error box
        /// carries a button that runs it.
        /// </summary>
        public string FixAction { get; set; }

        /// <summary>Label of that button. Null uses <see cref="DefaultFixLabel"/>.</summary>
        public string FixActionName { get; set; }

        /// <summary>Creates the attribute with an optional custom message.</summary>
        /// <param name="message">Message shown in the error box.</param>
        public RequiredAttribute(string message = null) => Message = message;
    }
}