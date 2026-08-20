using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Adds a button at the right edge of the field that opens the referenced asset in its default
    /// editor, the same as a double click in the project window. Works on object reference fields and on
    /// string fields that hold a project asset path.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class OpenAssetAttribute : PropertyAttribute
    {
        /// <summary>Optional label shown on the button. Null uses a default label.</summary>
        public string Label { get; }

        /// <summary>Creates the attribute with an optional custom label.</summary>
        /// <param name="label">Label shown on the button.</param>
        public OpenAssetAttribute(string label = null) => Label = label;
    }
}