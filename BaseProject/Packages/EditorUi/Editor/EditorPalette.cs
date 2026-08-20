using UnityEditor;
using UnityEngine;

namespace Base.EditorUiPackage
{
    /// <summary>
    /// The colors every Base editor window shares. A window that needs a color it can name in
    /// general terms takes it from here; a window that needs a color only it understands, such as a
    /// badge for one specific state, still defines that itself.
    /// <para>
    /// Every value resolves against the current editor skin through <see cref="Pick"/>, so a window
    /// never has to branch on <see cref="EditorGUIUtility.isProSkin"/> by hand.
    /// </para>
    /// </summary>
    public static class EditorPalette
    {
        /// <summary>Blue used for selection, drop targets and the primary button.</summary>
        public static Color Accent => Pick(new Color(0.32f, 0.60f, 0.94f), new Color(0.20f, 0.48f, 0.86f));

        /// <summary>Text drawn on top of <see cref="Accent"/>.</summary>
        public static Color AccentText => Color.white;

        /// <summary>Fill behind a whole window.</summary>
        public static Color Background => Pick(new Color(0.17f, 0.17f, 0.19f), new Color(0.83f, 0.83f, 0.85f));

        /// <summary>Outline of a field or a card.</summary>
        public static Color Border => Tint(0.09f, 0.16f);

        /// <summary>Fill of a card or a grouped block.</summary>
        public static Color Card => Pick(new Color(0.22f, 0.22f, 0.24f), new Color(0.85f, 0.85f, 0.87f));

        /// <summary>Red used for errors and destructive actions.</summary>
        public static Color Danger => Pick(new Color(0.86f, 0.34f, 0.36f), new Color(0.80f, 0.26f, 0.28f));

        /// <summary>Secondary text used for paths, counts and hints.</summary>
        public static Color DimText => Pick(new Color(0.56f, 0.56f, 0.61f), new Color(0.42f, 0.42f, 0.47f));

        /// <summary>Draggable line between two columns.</summary>
        public static Color Divider => Pick(new Color(0f, 0f, 0f, 0.35f), new Color(0f, 0f, 0f, 0.16f));

        /// <summary>Fill of a text field or a search box.</summary>
        public static Color Field => Pick(new Color(0.13f, 0.13f, 0.15f), new Color(0.95f, 0.95f, 0.96f));

        /// <summary>Amber used for pins, overrides and anything a window links to.</summary>
        public static Color Focus => new(0.95f, 0.75f, 0.25f);

        /// <summary>Tint of the row under the mouse.</summary>
        public static Color Hover => Tint(0.05f, 0.05f);

        /// <summary>Fill of a keyboard cap or a muted chip.</summary>
        public static Color KeyCap => Tint(0.10f, 0.08f);

        /// <summary>Hairline between two blocks of a window.</summary>
        public static Color Separator => Tint(0.07f, 0.10f);

        /// <summary>Outline of the selected row.</summary>
        public static Color Selection => Pick(new Color(0.32f, 0.60f, 0.94f, 0.90f),
            new Color(0.20f, 0.48f, 0.86f, 0.90f));

        /// <summary>Fill of the selected row.</summary>
        public static Color SelectionFill => Pick(new Color(0.32f, 0.60f, 0.94f, 0.20f),
            new Color(0.20f, 0.48f, 0.86f, 0.16f));

        /// <summary>Tint of every second row.</summary>
        public static Color Stripe => Tint(0.03f, 0.03f);

        /// <summary>Green used for a passed check or an empty problem list.</summary>
        public static Color Success => Pick(new Color(0.42f, 0.80f, 0.50f), new Color(0.20f, 0.58f, 0.30f));

        /// <summary>Primary text color.</summary>
        public static Color Text => Pick(new Color(0.88f, 0.88f, 0.90f), new Color(0.13f, 0.13f, 0.15f));

        /// <summary>Orange used for a warning that is not yet an error.</summary>
        public static Color Warning => Pick(new Color(0.95f, 0.65f, 0.25f), new Color(0.85f, 0.52f, 0.12f));

        /// <summary>
        /// Picks the value matching the current editor skin.
        /// </summary>
        /// <param name="pro">The color for the dark skin.</param>
        /// <param name="personal">The color for the light skin.</param>
        /// <returns>The color for the skin that is currently active.</returns>
        public static Color Pick(Color pro, Color personal) => EditorGUIUtility.isProSkin
            ? pro
            : personal;

        /// <summary>
        /// A neutral overlay that lightens on the dark skin and darkens on the light one, which is
        /// how nearly every subtle background tint in an editor window is built.
        /// </summary>
        /// <param name="proAlpha">The alpha used on the dark skin.</param>
        /// <param name="personalAlpha">The alpha used on the light skin.</param>
        /// <returns>The overlay color for the skin that is currently active.</returns>
        public static Color Tint(float proAlpha, float personalAlpha) => EditorGUIUtility.isProSkin
            ? new Color(1f, 1f, 1f, proAlpha)
            : new Color(0f, 0f, 0f, personalAlpha);

        /// <summary>
        /// A neutral overlay with the same alpha on both skins.
        /// </summary>
        /// <param name="alpha">The alpha used on either skin.</param>
        /// <returns>The overlay color for the skin that is currently active.</returns>
        public static Color Tint(float alpha) => Tint(alpha, alpha);
    }
}