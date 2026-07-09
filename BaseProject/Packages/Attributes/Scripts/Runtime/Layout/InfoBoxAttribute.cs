using System;
using UnityEngine;

namespace Base.AttributePackage.Layout
{
    /// <summary>
    /// Draws a help or warning box with a message above the decorated field.
    /// Attached above a field like <c>[Header]</c>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class InfoBoxAttribute : PropertyAttribute
    {
        /// <summary>Message shown inside the box.</summary>
        public string Message { get; }

        /// <summary>Icon and severity of the box.</summary>
        public EInfoBoxType Type { get; }

        /// <summary>Creates the attribute with a message and an optional box type.</summary>
        public InfoBoxAttribute(string message, EInfoBoxType type = EInfoBoxType.Info)
        {
            Message = message;
            Type = type;
        }
    }
}