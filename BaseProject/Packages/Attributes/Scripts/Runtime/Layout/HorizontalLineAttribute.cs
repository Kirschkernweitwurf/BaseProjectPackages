using System;
using Base.AttributePackage.ColorEnums;
using UnityEngine;

namespace Base.AttributePackage.Layout
{
    /// <summary>
    /// Draws a plain horizontal separator line above the decorated field.
    /// Attached above a field like <c>[Header]</c>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class HorizontalLineAttribute : PropertyAttribute
    {
        /// <summary>Optional HTML color for the line, for example "#3498DB". Null uses the default color.</summary>
        public string ColorHex { get; }

        /// <summary>Optional preset color. <see cref="EColor.Default"/> uses the default color.</summary>
        public EColor PresetColor { get; }

        /// <summary>Line thickness in pixels.</summary>
        public float Thickness { get; }

        /// <summary>Vertical padding above and below the line in pixels.</summary>
        public float Padding { get; }

        /// <summary>Creates the attribute with an optional HTML color, thickness and padding.</summary>
        public HorizontalLineAttribute(string colorHex = null, float thickness = 1f, float padding = 8f)
        {
            ColorHex = colorHex;
            PresetColor = EColor.Default;
            Thickness = thickness;
            Padding = padding;
        }

        /// <summary>Creates the attribute with a preset color and optional thickness and padding.</summary>
        public HorizontalLineAttribute(EColor color, float thickness = 1f, float padding = 8f)
        {
            ColorHex = null;
            PresetColor = color;
            Thickness = thickness;
            Padding = padding;
        }
    }
}