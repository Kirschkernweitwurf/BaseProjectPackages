using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Requires a string to be non-empty or a list or array to contain at least one element.
    /// Shows an error box when the value is null or empty.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class NotNullOrEmptyAttribute : PropertyAttribute
    {
        /// <summary>Reason text used when no custom message is set. Shared by the rule and the drawer.</summary>
        public const string DefaultReason = "must not be empty";

        /// <summary>Optional custom message. Null uses a default message.</summary>
        public string Message { get; }

        /// <summary>Creates the attribute with an optional custom message.</summary>
        public NotNullOrEmptyAttribute(string message = null) => Message = message;
    }
}