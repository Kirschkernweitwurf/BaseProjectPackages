using System;
using UnityEngine;

namespace Base.AttributePackage.ColorAttributes
{
    /// <summary>Tints the background of a field. Accepts a hex string or an <see cref="EColor"/> preset.</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class GUIColorAttribute : PropertyAttribute
    {
        /// <summary>Optional HTML color, for example "#E74C3C".</summary>
        public string ColorHex { get; }

        /// <summary>Optional preset color.</summary>
        public EColor PresetColor { get; }

        /// <summary>Creates the attribute with an HTML color.</summary>
        public GUIColorAttribute(string colorHex)
        {
            ColorHex = colorHex;
            PresetColor = EColor.Default;
        }

        /// <summary>Creates the attribute with a preset color.</summary>
        public GUIColorAttribute(EColor color)
        {
            ColorHex = null;
            PresetColor = color;
        }
    }
}