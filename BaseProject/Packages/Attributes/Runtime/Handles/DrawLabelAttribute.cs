using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Draws floating text in the scene view at the position held by this Vector3 field. The text is
    /// either a constant or read from another member, which is how a live value ends up on screen.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class DrawLabelAttribute : PropertyAttribute
    {
        /// <summary>Constant text shown at the position. Null falls back to the member or the field name.</summary>
        public string Text { get; }

        /// <summary>Optional name of a member whose value is shown instead of the constant text.</summary>
        public string TextMember { get; set; }

        /// <summary>Preset color of the text.</summary>
        public EColor PresetColor { get; set; } = EColor.Default;

        /// <summary>Whether the position is a local offset or a world position.</summary>
        public ESpace Space { get; set; } = ESpace.Local;

        /// <summary>Creates the attribute.</summary>
        /// <param name="text">Constant text shown at the position.</param>
        public DrawLabelAttribute(string text = null) => Text = text;
    }
}