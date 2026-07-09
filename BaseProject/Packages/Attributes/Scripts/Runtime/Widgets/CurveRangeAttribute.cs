using System;
using Base.AttributePackage.ColorEnums;
using UnityEngine;

namespace Base.AttributePackage.Widgets
{
    /// <summary>
    /// Constrains an AnimationCurve field to a range and optionally tints the curve.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class CurveRangeAttribute : PropertyAttribute
    {
        /// <summary>Lower bound on the horizontal axis.</summary>
        public float MinX { get; }

        /// <summary>Lower bound on the vertical axis.</summary>
        public float MinY { get; }

        /// <summary>Upper bound on the horizontal axis.</summary>
        public float MaxX { get; }

        /// <summary>Upper bound on the vertical axis.</summary>
        public float MaxY { get; }

        /// <summary>Optional preset color. <see cref="EColor.Default"/> uses the standard curve color.</summary>
        public EColor PresetColor { get; }

        /// <summary>Creates the attribute with an explicit range and an optional preset color.</summary>
        public CurveRangeAttribute(float minX, float minY, float maxX, float maxY, EColor color = EColor.Default)
        {
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
            PresetColor = color;
        }

        /// <summary>Creates the attribute with a uniform range and an optional preset color.</summary>
        public CurveRangeAttribute(float min, float max, EColor color = EColor.Default)
        {
            MinX = min;
            MinY = min;
            MaxX = max;
            MaxY = max;
            PresetColor = color;
        }
    }
}