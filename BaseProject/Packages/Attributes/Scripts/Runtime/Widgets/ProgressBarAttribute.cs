using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Draws a numeric field as a progress bar. The maximum is a constant or a numeric member.
    /// The bar is draggable by default; pass readOnly to make it a pure display.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ProgressBarAttribute : PropertyAttribute
    {
        /// <summary>Constant maximum, used when no member is given.</summary>
        public float Max { get; }

        /// <summary>Optional name of a numeric member providing the maximum.</summary>
        public string MaxMember { get; }

        /// <summary>Optional preset fill color.</summary>
        public EColor PresetColor { get; }

        /// <summary>When true, the bar only displays the value and cannot be dragged.</summary>
        public bool ReadOnly { get; }

        /// <summary>Creates the attribute with a constant maximum, an optional color and read-only mode.</summary>
        public ProgressBarAttribute(float max, EColor color = EColor.Default, bool readOnly = false)
        {
            Max = max;
            PresetColor = color;
            ReadOnly = readOnly;
        }

        /// <summary>Creates the attribute with a member-driven maximum, an optional color and read-only mode.</summary>
        public ProgressBarAttribute(string maxMember, EColor color = EColor.Default, bool readOnly = false)
        {
            MaxMember = maxMember;
            PresetColor = color;
            ReadOnly = readOnly;
        }
    }
}