using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Draws a small prefix label before a field, for example a unit or a sign. The counterpart to
    /// <see cref="SuffixAttribute"/>. Both can be combined on the same field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class PrefixAttribute : PropertyAttribute
    {
        /// <summary>Prefix text.</summary>
        public string Text { get; }

        /// <summary>Creates the attribute with the prefix text.</summary>
        public PrefixAttribute(string text) => Text = text;
    }
}
