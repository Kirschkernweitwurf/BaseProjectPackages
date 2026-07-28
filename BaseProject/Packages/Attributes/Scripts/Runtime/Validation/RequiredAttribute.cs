using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Marks an object reference field as required. Shows an error box when the reference is null.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class RequiredAttribute : PropertyAttribute
    {
        /// <summary>Reason text used when no custom message is set. Shared by the rule and the drawer.</summary>
        public const string DefaultReason = "is required";

        /// <summary>Optional custom message. Null uses a default message.</summary>
        public string Message { get; }

        /// <summary>Creates the attribute with an optional custom message.</summary>
        public RequiredAttribute(string message = null) => Message = message;
    }
}