using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Draws a bold section title with an underline above the decorated field.
    /// Does not render a value of its own and is attached above a field like <c>[Header]</c>.
    /// With <see cref="Foldout"/> enabled the title becomes a collapsible header that hides every
    /// field beneath it, including its own field, until the next field that carries a title.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class TitleAttribute : PropertyAttribute
    {
        /// <summary>Text shown as the bold title.</summary>
        public string Title { get; }

        /// <summary>
        /// Optional HTML color for title and underline, for example "#E74C3C". Null uses the default color.
        /// </summary>
        public string ColorHex { get; }

        /// <summary>Optional preset color. <see cref="EColor.Default"/> uses the default color.</summary>
        public EColor PresetColor { get; }

        /// <summary>
        /// When true, the title is drawn as a collapsible foldout that folds all fields below it, up
        /// to the next titled field, into the header. Set with <c>[Title("Combat", Foldout = true)]</c>.
        /// </summary>
        public bool Foldout { get; set; }

        /// <summary>Expanded state of a collapsible title the first time it is shown. Defaults to true.</summary>
        public bool DefaultExpanded { get; set; } = true;

        /// <summary>Creates the attribute with a title and an optional HTML color.</summary>
        public TitleAttribute(string title, string colorHex = null)
        {
            Title = title;
            ColorHex = colorHex;
            PresetColor = EColor.Default;
        }

        /// <summary>Creates the attribute with a title and a preset color.</summary>
        public TitleAttribute(string title, EColor color)
        {
            Title = title;
            ColorHex = null;
            PresetColor = color;
        }
    }
}