using System;
using UnityEngine;

namespace Base.AttributePackage.Layout
{
    /// <summary>
    /// Increases the inspector indent level of the decorated field.
    /// Handled by the attribute package inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class IndentAttribute : PropertyAttribute
    {
        /// <summary>Number of indent steps to add.</summary>
        public int Amount { get; }

        /// <summary>Creates the attribute with an optional indent amount.</summary>
        public IndentAttribute(int amount = 1) => Amount = amount;
    }
}