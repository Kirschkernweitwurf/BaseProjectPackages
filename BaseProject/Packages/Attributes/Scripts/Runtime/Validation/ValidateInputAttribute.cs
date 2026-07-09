using System;
using UnityEngine;

namespace Base.AttributePackage.Validation
{
    /// <summary>
    /// Runs a custom validation method and shows an error box when it returns false.
    /// The method must return <see cref="bool"/> and take either no parameter or a single
    /// parameter matching the field type. Example: <c>[ValidateInput(nameof(IsValid))]</c>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ValidateInputAttribute : PropertyAttribute
    {
        /// <summary>Name of the validation method on the same object.</summary>
        public string MethodName { get; }

        /// <summary>Optional custom message. Null uses a default message.</summary>
        public string Message { get; }

        /// <summary>Creates the attribute with a method name and an optional custom message.</summary>
        public ValidateInputAttribute(string methodName, string message = null)
        {
            MethodName = methodName;
            Message = message;
        }
    }
}