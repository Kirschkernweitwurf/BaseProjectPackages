using System;
using Base.AttributePackage.ColorEnums;
using UnityEngine;

namespace Base.AttributePackage.Widgets
{
    /// <summary>
    /// Draws a numeric field as a read-only progress bar. The maximum is a constant or a numeric member.
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

        /// <summary>Creates the attribute with a constant maximum and an optional color.</summary>
        public ProgressBarAttribute(float max, EColor color = EColor.Default)
        {
            Max = max;
            PresetColor = color;
        }

        /// <summary>Creates the attribute with a member-driven maximum and an optional color.</summary>
        public ProgressBarAttribute(string maxMember, EColor color = EColor.Default)
        {
            MaxMember = maxMember;
            PresetColor = color;
        }
    }
}