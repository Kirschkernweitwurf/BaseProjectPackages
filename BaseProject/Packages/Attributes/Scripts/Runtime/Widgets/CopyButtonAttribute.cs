using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Adds a button that copies the current field value to the system clipboard as text. Inline by
    /// default, sitting at the right edge of the field. Pass <c>inline: false</c> for a button on its
    /// own row below the field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class CopyButtonAttribute : PropertyAttribute
    {
        /// <summary>Whether the button sits on the field line instead of a row below it.</summary>
        public bool Inline { get; }

        /// <summary>Creates the attribute, inline by default.</summary>
        public CopyButtonAttribute(bool inline = true) => Inline = inline;
    }
}
