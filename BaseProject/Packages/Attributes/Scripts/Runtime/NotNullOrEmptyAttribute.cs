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
        /// <summary>Optional custom message. Null uses a default message.</summary>
        public string Message { get; }

        /// <summary>Creates the attribute with an optional custom message.</summary>
        public NotNullOrEmptyAttribute(string message = null) => Message = message;
    }
}
