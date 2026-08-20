using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Replaces the label Unity derives from the field name, for the cases where the good field name and
    /// the good label are not the same word.
    /// </summary>
    /// <remarks>
    /// Accepts a member reference, so a label can be computed: <c>[Label("$" + nameof(Caption))]</c>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class LabelAttribute : PropertyAttribute
    {
        /// <summary>The label to show, or a member reference producing it.</summary>
        public string Text { get; }

        /// <summary>Creates the attribute.</summary>
        /// <param name="text">The label to show.</param>
        public LabelAttribute(string text) => Text = text;
    }
}