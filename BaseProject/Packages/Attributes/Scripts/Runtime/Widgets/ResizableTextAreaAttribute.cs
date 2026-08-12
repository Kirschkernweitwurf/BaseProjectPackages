using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// A multi-line text field that grows with its content. Unity's own text area fixes the visible line
    /// count at compile time, so pasting forty lines into a three-line box means scrolling forever.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ResizableTextAreaAttribute : PropertyAttribute
    {
        /// <summary>Line count the box never grows beyond, after which it scrolls.</summary>
        public const int DefaultMaximumLines = 20;
        /// <summary>Line count the box never shrinks below.</summary>
        public const int DefaultMinimumLines = 3;

        /// <summary>Smallest height in lines.</summary>
        public int MinimumLines { get; }

        /// <summary>Largest height in lines.</summary>
        public int MaximumLines { get; }

        /// <summary>Creates the attribute.</summary>
        /// <param name="minimumLines">Smallest height in lines.</param>
        /// <param name="maximumLines">Largest height in lines.</param>
        public ResizableTextAreaAttribute(int minimumLines = DefaultMinimumLines,
            int maximumLines = DefaultMaximumLines)
        {
            MinimumLines = Mathf.Max(1, minimumLines);
            MaximumLines = Mathf.Max(MinimumLines, maximumLines);
        }
    }
}