using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Adds an entry to the field's right-click menu, next to Unity's own Copy and Paste. For the
    /// per-field actions that do not deserve a visible button eating horizontal space: reset,
    /// normalize, fill from somewhere else.
    /// </summary>
    /// <remarks>
    /// Repeatable, so one field can offer several entries. Use a slash in the label to nest them.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class CustomContextMenuAttribute : PropertyAttribute
    {
        /// <summary>Text of the menu entry. Slashes create submenus.</summary>
        public string Label { get; }

        /// <summary>Name of the parameterless method the entry runs.</summary>
        public string Method { get; }

        /// <summary>Creates the attribute.</summary>
        /// <param name="label">Text of the menu entry.</param>
        /// <param name="method">Name of the parameterless method the entry runs.</param>
        public CustomContextMenuAttribute(string label, string method)
        {
            Label = label;
            Method = method;
        }
    }
}