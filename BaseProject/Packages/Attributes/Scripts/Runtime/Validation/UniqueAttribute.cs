using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Requires every entry of a list or array to be unique. Shows an error box naming the first
    /// duplicate pair. Null and empty entries are ignored, so a list that is still being filled stays
    /// quiet. Object references compare by reference, strings and value types by value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class UniqueAttribute : PropertyAttribute
    {
        /// <summary>Optional custom message. Null uses a default message.</summary>
        public string Message { get; }

        /// <summary>Creates the attribute with an optional custom message.</summary>
        public UniqueAttribute(string message = null) => Message = message;
    }
}