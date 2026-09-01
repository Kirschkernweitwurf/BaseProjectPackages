using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Draws a nested serializable type on the field's own line instead of behind a foldout.
    /// </summary>
    /// <remarks>
    /// A two-field value object costs three rows and a click as a foldout, to show two numbers. Inline
    /// it reads as one row, which is what it is.
    /// <para>
    /// Only leaf children are laid out this way. A nested type holding another nested type, a list or
    /// anything else that needs its own height falls back to the foldout, because a row cannot honestly
    /// contain it.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class InlinePropertyAttribute : PropertyAttribute
    {
        /// <summary>Label width used for each child when none is given.</summary>
    }
}