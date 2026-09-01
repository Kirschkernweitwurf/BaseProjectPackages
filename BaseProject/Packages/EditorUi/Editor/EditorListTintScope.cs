using System;
using UnityEngine;

namespace Base.EditorUiPackage
{
    /// <summary>
    /// Recolors Unity's own reorderable list to the theme for as long as the scope is open, leaving
    /// every shape, corner, border, drag grip and footer button exactly where Unity draws it.
    /// </summary>
    /// <remarks>
    /// The header strip, the box behind the rows and the footer tab are textures in the built-in
    /// skin. They cannot be replaced from outside, but they can be multiplied:
    /// <see cref="GUI.backgroundColor"/> tints the background of every style drawn while it is set.
    /// One factor, chosen so the grey Unity draws its list surfaces on lands on
    /// <see cref="EditorPalette.Background"/>, moves all three tones together and keeps the
    /// relationship between them, because they were only ever the same grey at different values.
    /// <para>
    /// This is the difference between a list that is Unity's and a list that copies Unity's. Nothing
    /// is repainted, so nothing can be a pixel out, and an array nested inside a row is recolored
    /// with it without anything having to know it is there.
    /// </para>
    /// <para>
    /// The tint reaches every style drawn inside the scope, not only the list, so wrap the list and
    /// not the whole window. Anything with a background of its own drawn inside is tinted with it.
    /// </para>
    /// </remarks>
    public sealed class EditorListTintScope : IDisposable
    {
        /// <summary>
        /// The grey Unity draws the list surfaces on under the dark editor theme, as a fraction. The
        /// header and the window behind it are this value, the box is below it and the footer tab is
        /// above it, which is the relationship the tint carries over.
        /// </summary>
        private const float DarkSurface = 0.22f;

        /// <summary>The same grey under the light editor theme.</summary>
        private const float LightSurface = 0.76f;

        private readonly Color _previous;

        /// <summary>Opens the scope. Always pair with <see cref="Dispose"/>, through a using block.</summary>
        public EditorListTintScope()
        {
            _previous = GUI.backgroundColor;

            GUI.backgroundColor = Tint();
        }

        /// <summary>Hands the tint back to whatever was set before the scope opened.</summary>
        public void Dispose() => GUI.backgroundColor = _previous;

        private static Color Tint()
        {
            float surface = EditorThemeProvider.IsDarkMode
                ? DarkSurface
                : LightSurface;

            Color target = EditorPalette.Background;

            return new Color(target.r / surface, target.g / surface, target.b / surface, 1f);
        }
    }
}