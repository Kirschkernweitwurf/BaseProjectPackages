using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Draws a help or warning box with a message next to the decorated field, like <c>[Header]</c>.
    /// Set <see cref="Position"/> to draw above or below. Set <see cref="Compact"/> for a small
    /// single-line box. A custom color (hex or an <see cref="EColor"/> preset) applies in compact mode
    /// only and implies compact.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class InfoBoxAttribute : PropertyAttribute
    {
        /// <summary>Message shown inside the box.</summary>
        public string Message { get; }

        /// <summary>Icon and severity of the box.</summary>
        public EInfoBoxType Type { get; }

        /// <summary>Whether the box draws above or below the field.</summary>
        public EInfoBoxPosition Position { get; }

        /// <summary>Draws the small single-line variant instead of the full HelpBox.</summary>
        public bool Compact { get; }

        /// <summary>Optional HTML color for the compact box, e.g. "#E74C3C". Null uses the type color.</summary>
        public string ColorHex { get; }

        /// <summary>Optional preset color for the compact box. Default uses the type color.</summary>
        public EColor PresetColor { get; }

        /// <summary>True when an explicit hex or preset color was set.</summary>
        public bool HasExplicitColor => !string.IsNullOrEmpty(ColorHex) || PresetColor != EColor.Default;

        /// <summary>Creates the attribute with a message plus optional type, position, compact and hex color.</summary>
        public InfoBoxAttribute(string message, EInfoBoxType type = EInfoBoxType.Info,
            EInfoBoxPosition position = EInfoBoxPosition.Above, bool compact = false, string colorHex = null)
        {
            Message = message;
            Type = type;
            Position = position;
            Compact = compact;
            ColorHex = colorHex;
            PresetColor = EColor.Default;
        }

        /// <summary>Creates a compact colored box from a preset color.</summary>
        public InfoBoxAttribute(string message, EColor color, EInfoBoxType type = EInfoBoxType.Info,
            EInfoBoxPosition position = EInfoBoxPosition.Above, bool compact = true)
        {
            Message = message;
            Type = type;
            Position = position;
            Compact = compact;
            ColorHex = null;
            PresetColor = color;
        }
    }
}