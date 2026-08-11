using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Lays consecutive fields sharing a group name side by side on one row, at relative widths.
    /// </summary>
    /// <remarks>
    /// Positional like <see cref="FoldoutAttribute"/> rather than path-based: a run of fields carrying
    /// the same group name forms one row, and the run ends where the name changes. That keeps the
    /// grouping visible in the order the fields are written, instead of scattered across declarations
    /// that have to be mentally reassembled.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class HorizontalAttribute : PropertyAttribute
    {
        /// <summary>Share of the row width used when none is given.</summary>
        public const float DefaultWeight = 1f;

        /// <summary>Name of the row this field belongs to.</summary>
        public string Group { get; }

        /// <summary>Share of the row width relative to the other fields on it.</summary>
        public float Weight { get; set; } = DefaultWeight;

        /// <summary>Whether the field keeps its label. False gives the value the whole cell.</summary>
        public bool ShowLabel { get; set; } = true;

        /// <summary>Creates the attribute.</summary>
        /// <param name="group">Name of the row this field belongs to.</param>
        public HorizontalAttribute(string group) => Group = group;
    }
}