using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Adds a button that opens the referenced asset in its default editor, the same as a double click
    /// in the project window. Works on object reference fields and on string fields that hold a project
    /// asset path. Inline by default, sitting at the right edge of the field. Pass <c>inline: false</c>
    /// for a button on its own row below the field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class OpenAssetAttribute : PropertyAttribute
    {
        /// <summary>Optional label for the non-inline button. Null uses a default label.</summary>
        public string Label { get; }

        /// <summary>Whether the button sits on the field line instead of a row below it.</summary>
        public bool Inline { get; }

        /// <summary>Creates the attribute, inline by default, with an optional non-inline label.</summary>
        public OpenAssetAttribute(bool inline = true, string label = null)
        {
            Inline = inline;
            Label = label;
        }
    }
}