using UnityEditor;
using UnityEngine;

namespace Base.EditorUiPackage
{
    /// <summary>
    /// The colors every Base editor window shares. A window that needs a color it can name in
    /// general terms takes it from here; a window that needs a color only it understands, such as a
    /// badge for one specific state, still defines that itself.
    /// <para>
    /// Every value comes from the theme assigned in the Editor UI Theme project settings page, for
    /// the editor theme that is currently active, so a window never has to branch on
    /// <see cref="EditorGUIUtility.isProSkin"/> by hand and a user can change the whole look without
    /// touching code.
    /// </para>
    /// </summary>
    public static class EditorPalette
    {
        /// <summary>Blue used for selection, drop targets and the primary button.</summary>
        public static Color Accent => EditorThemeProvider.Colors.Accent;

        /// <summary>Text drawn on top of <see cref="Accent"/>.</summary>
        public static Color AccentText => EditorThemeProvider.Colors.AccentText;

        /// <summary>Fill behind a whole window.</summary>
        public static Color Background => EditorThemeProvider.Colors.Background;

        /// <summary>Outline of a field or a card.</summary>
        public static Color Border => EditorThemeProvider.Colors.Border;

        /// <summary>Fill of a card or a grouped block.</summary>
        public static Color Card => EditorThemeProvider.Colors.Card;

        /// <summary>Red used for errors and destructive actions.</summary>
        public static Color Danger => EditorThemeProvider.Colors.Danger;

        /// <summary>Secondary text used for paths, counts and hints.</summary>
        public static Color DimText => EditorThemeProvider.Colors.DimText;

        /// <summary>Draggable line between two columns.</summary>
        public static Color Divider => EditorThemeProvider.Colors.Divider;

        /// <summary>Fill of a text field or a search box.</summary>
        public static Color Field => EditorThemeProvider.Colors.Field;

        /// <summary>Amber used for pins, overrides and anything a window links to.</summary>
        public static Color Focus => EditorThemeProvider.Colors.Focus;

        /// <summary>Tint of the row under the mouse.</summary>
        public static Color Hover => EditorThemeProvider.Colors.Hover;

        /// <summary>Fill of a keyboard cap or a muted chip.</summary>
        public static Color KeyCap => EditorThemeProvider.Colors.KeyCap;

        /// <summary>Fill of a button that is not the primary action.</summary>
        public static Color Secondary => EditorThemeProvider.Colors.Secondary;

        /// <summary>Text drawn on top of <see cref="Secondary"/>.</summary>
        public static Color SecondaryText => EditorThemeProvider.Colors.SecondaryText;

        /// <summary>Outline of the selected row.</summary>
        public static Color Selection => EditorThemeProvider.Colors.Selection;

        /// <summary>Fill of the selected row.</summary>
        public static Color SelectionFill => EditorThemeProvider.Colors.SelectionFill;

        /// <summary>Hairline between two blocks of a window.</summary>
        public static Color Separator => EditorThemeProvider.Colors.Separator;

        /// <summary>Tint of every second row.</summary>
        public static Color Stripe => EditorThemeProvider.Colors.Stripe;

        /// <summary>Green used for a passed check or an empty problem list.</summary>
        public static Color Success => EditorThemeProvider.Colors.Success;

        /// <summary>Primary text color.</summary>
        public static Color Text => EditorThemeProvider.Colors.Text;

        /// <summary>Orange used for a warning that is not yet an error.</summary>
        public static Color Warning => EditorThemeProvider.Colors.Warning;

        /// <summary>
        /// Picks the value matching the current editor theme.
        /// </summary>
        /// <remarks>
        /// For the colors a window defines itself. Anything named in the palette above already
        /// carries a value per editor theme and should be read from there instead.
        /// </remarks>
        /// <param name="pro">The color for the dark editor theme.</param>
        /// <param name="personal">The color for the light editor theme.</param>
        /// <returns>The color for the editor theme that is currently active.</returns>
        public static Color Pick(Color pro, Color personal) => EditorThemeProvider.IsDarkMode
            ? pro
            : personal;

        /// <summary>
        /// A neutral overlay that lightens on the dark editor theme and darkens on the light one, which is
        /// how nearly every subtle background tint in an editor window is built.
        /// </summary>
        /// <param name="proAlpha">The alpha used on the dark editor theme.</param>
        /// <param name="personalAlpha">The alpha used on the light editor theme.</param>
        /// <returns>The overlay color for the editor theme that is currently active.</returns>
        public static Color Tint(float proAlpha, float personalAlpha) => EditorThemeProvider.IsDarkMode
            ? new Color(1f, 1f, 1f, proAlpha)
            : new Color(0f, 0f, 0f, personalAlpha);

        /// <summary>
        /// A neutral overlay with the same alpha under either editor theme.
        /// </summary>
        /// <param name="alpha">The alpha used on either editor theme.</param>
        /// <returns>The overlay color for the editor theme that is currently active.</returns>
        public static Color Tint(float alpha) => Tint(alpha, alpha);

        /// <summary>
        /// The same color at a different opacity, which is how a badge or a row tint is built from a
        /// palette color.
        /// </summary>
        /// <param name="color">The color to fade.</param>
        /// <param name="alpha">The opacity to lay it on with.</param>
        /// <returns>The faded color.</returns>
        public static Color WithAlpha(Color color, float alpha) => new(color.r, color.g, color.b, alpha);
    }
}