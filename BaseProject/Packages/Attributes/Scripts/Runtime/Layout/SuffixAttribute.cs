using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>Draws a small suffix label after a field, for example a unit like "m/s".</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SuffixAttribute : PropertyAttribute
    {
        /// <summary>Suffix text.</summary>
        public string Text { get; }

        /// <summary>Creates the attribute with the suffix text.</summary>
        public SuffixAttribute(string text) => Text = text;
    }
}