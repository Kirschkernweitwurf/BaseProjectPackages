using System;
using UnityEngine;

namespace Base.AttributesPackage
{
    /// <summary>
    /// Runs a custom validation method and reports what it says. The method takes either no parameter or
    /// a single parameter matching the field type, for example <c>[ValidateInput(nameof(IsValid))]</c>.
    /// </summary>
    /// <remarks>
    /// The method may return a bool or a <see cref="ValidationResult"/>. Returning a result lets the
    /// method carry its own message and choose between a warning and an error, which a bool cannot do:
    /// with a bool, every failure of every check shares the one message written on the attribute.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ValidateInputAttribute : PropertyAttribute
    {
        /// <summary>Label used for the fix button when none is given.</summary>
        public const string DefaultFixLabel = "Fix";

        /// <summary>Name of the validation method on the same object.</summary>
        public string MethodName { get; }

        /// <summary>Optional message, used when the method returns a bool rather than a result.</summary>
        public string Message { get; }

        /// <summary>
        /// Optional name of a parameterless method that repairs the value. When set, the box carries a
        /// button that runs it.
        /// </summary>
        public string FixAction { get; set; }

        /// <summary>Label of that button. Null uses <see cref="DefaultFixLabel"/>.</summary>
        public string FixActionName { get; set; }

        /// <summary>Creates the attribute with a method name and an optional message.</summary>
        /// <param name="methodName">Name of the validation method.</param>
        /// <param name="message">Message used when the method returns a bool.</param>
        public ValidateInputAttribute(string methodName, string message = null)
        {
            MethodName = methodName;
            Message = message;
        }
    }
}