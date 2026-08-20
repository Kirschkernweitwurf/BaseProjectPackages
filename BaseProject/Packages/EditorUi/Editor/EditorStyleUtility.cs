using UnityEditor;
using UnityEngine;

namespace Base.EditorUiPackage
{
    /// <summary>
    /// Small helpers every Base editor window needs when it builds its own styles.
    /// </summary>
    public static class EditorStyleUtility
    {
        /// <summary>
        /// Pins a style's text color across all four states.
        /// </summary>
        /// <remarks>
        /// Labels inherit hover, active and focused colors from the editor skin, which makes plain
        /// text light up like a button when the mouse passes over it. Pinning every state fixes it.
        /// </remarks>
        /// <param name="style">The style to pin. Returned so this can be chained onto a style initializer.</param>
        /// <param name="color">The color used in every state.</param>
        /// <returns>The same style, for chaining.</returns>
        public static GUIStyle PinTextColor(GUIStyle style, Color color)
        {
            if (style == null)
                return null;

            style.normal.textColor = color;
            style.hover.textColor = color;
            style.active.textColor = color;
            style.focused.textColor = color;

            return style;
        }

        /// <summary>
        /// Brightens a color while hovered and darkens it while pressed, so a hand-drawn button
        /// reacts the way a real one does.
        /// </summary>
        /// <param name="color">The resting color.</param>
        /// <param name="isHovered">Whether the mouse sits on the control.</param>
        /// <param name="isPressed">Whether the control is being held down.</param>
        /// <returns>The color to draw in the current state.</returns>
        public static Color Shade(Color color, bool isHovered, bool isPressed)
        {
            if (isPressed)
                return Offset(color, -EditorMetrics.PressDrop);

            return isHovered
                ? Offset(color, EditorMetrics.HoverLift)
                : color;
        }

        /// <summary>
        /// The muted gray the editor uses for secondary labels, taken from the skin rather than
        /// guessed, so it matches Unity's own inspectors.
        /// </summary>
        /// <returns>The muted label color of the active skin.</returns>
        public static Color MutedTextColor() => EditorStyles.centeredGreyMiniLabel.normal.textColor;

        /// <summary>
        /// A padding with the same value on every side.
        /// </summary>
        /// <param name="value">The padding in pixels.</param>
        /// <returns>The uniform padding.</returns>
        public static RectOffset UniformPadding(int value) => new(value, value, value, value);

        /// <summary>
        /// A padding applied on the left and right only.
        /// </summary>
        /// <param name="value">The padding in pixels.</param>
        /// <returns>The horizontal padding.</returns>
        public static RectOffset HorizontalPadding(int value) => new(value, value, 0, 0);

        private static Color Offset(Color color, float amount)
            => new(color.r + amount, color.g + amount, color.b + amount, color.a);
    }
}