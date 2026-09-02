using System;
using UnityEngine;

namespace Base.AttributesPackage
{
    /// <summary>
    /// Adds a toggle next to a referenced asset that draws the asset's own inspector inline, so a
    /// ScriptableObject can be edited without changing the Project window selection. Only applies to
    /// object reference fields; other field types ignore the attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ExpandableAttribute : PropertyAttribute
    {
        /// <summary>Whether the embedded inspector starts expanded. Defaults to false.</summary>
        public bool DefaultExpanded { get; set; }
    }
}