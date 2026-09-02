using UnityEngine;

namespace Base.EditorUIPackage.Editor
{
    /// <summary>
    /// Five complete looks a project can start from, and the values behind them.
    /// </summary>
    /// <remarks>
    /// Every color was fitted to a contrast target rather than picked by eye, against both models
    /// that matter, because they disagree exactly where editor themes live:
    /// <list type="bullet">
    /// <item>
    /// WCAG 2 contrast ratio, the accessibility standard regulators actually measure against.
    /// Every preset clears 4.5 : 1 on text, and Ink clears 7 : 1.
    /// </item>
    /// <item>
    /// APCA lightness contrast, the perceptual model behind the draft WCAG 3. It matters here
    /// because WCAG 2 overstates contrast on dark surfaces, so a dark theme can pass 4.5 : 1 and
    /// still be hard to read. Every preset reaches Lc 90 on body text and Lc 60 on secondary text
    /// and status colors, and Ink reaches Lc 100 and Lc 75.
    /// </item>
    /// </list>
    /// <para>
    /// The dark surfaces sit near <c>#1C1C1F</c> rather than at black on purpose: light text on a
    /// near-black background blooms, which is tiring to read even though the measured contrast is
    /// enormous.
    /// </para>
    /// <para>
    /// Only Harbor keeps its good, warning and bad colors apart under simulated deuteranopia and
    /// protanopia. The others rely on the words next to them, which is why a Base window always
    /// spells a state out rather than leaving a bare colored dot to carry it.
    /// </para>
    /// </remarks>
    public static class EditorThemePresets
    {
        /// <summary>
        /// Overwrites a theme with one of the presets.
        /// </summary>
        /// <param name="theme">The theme to fill in.</param>
        /// <param name="preset">The look to apply.</param>
        public static void Apply(EditorTheme theme, EEditorThemePreset preset)
        {
            if (theme == null)
                return;

            theme.SetColors(CreateColors(preset, true), CreateColors(preset, false));
            theme.SetMetrics(CreateMetrics(preset), EditorThemeDefaults.CreateTable());
        }

        /// <summary>
        /// Every preset, in the order they are meant to be read: the Base look, then the two that
        /// solve a specific problem, then the two that are a matter of taste.
        /// </summary>
        /// <returns>The presets in display order.</returns>
        public static EEditorThemePreset[] CreateOrder() => new[]
        {
            EEditorThemePreset.Slate,
            EEditorThemePreset.Ink,
            EEditorThemePreset.Harbor,
            EEditorThemePreset.Rose,
            EEditorThemePreset.Ember
        };

        /// <summary>
        /// The handful of colors that say what a preset looks like at a glance, for the swatch strip
        /// on its button.
        /// </summary>
        /// <param name="preset">The preset to sample.</param>
        /// <param name="isDarkMode">
        /// True to sample the dark mode colors, so the strip matches what is being previewed.
        /// </param>
        /// <returns>The card, accent, good, warning and bad colors, in that order.</returns>
        public static Color[] CreateSwatches(EEditorThemePreset preset, bool isDarkMode)
        {
            EditorThemeColors colors = CreateColors(preset, isDarkMode);

            return new[]
            {
                colors.Card,
                colors.Accent,
                colors.Success,
                colors.Warning,
                colors.Danger
            };
        }

        /// <summary>
        /// Works out which preset a theme still matches.
        /// </summary>
        /// <remarks>
        /// Compares the colors of both editor themes only. The metrics are left out on purpose, so nudging a
        /// row height does not stop a theme being recognised as the palette it plainly still is.
        /// </remarks>
        /// <param name="theme">The theme to identify.</param>
        /// <param name="preset">The preset it matches, when this returns true.</param>
        /// <returns>True when the theme matches one of the presets exactly.</returns>
        public static bool TryIdentify(EditorTheme theme, out EEditorThemePreset preset)
        {
            preset = EEditorThemePreset.Slate;

            if (theme == null)
                return false;

            foreach (EEditorThemePreset candidate in CreateOrder())
            {
                if (!Matches(theme, candidate))
                    continue;

                preset = candidate;

                return true;
            }

            return false;
        }

        /// <summary>
        /// The name shown on the button that applies a preset.
        /// </summary>
        /// <param name="preset">The preset to name.</param>
        /// <returns>The display name.</returns>
        public static string DisplayName(EEditorThemePreset preset) => preset switch
        {
            EEditorThemePreset.Ember => "Ember",
            EEditorThemePreset.Harbor => "Harbor",
            EEditorThemePreset.Ink => "Ink",
            EEditorThemePreset.Rose => "Rosé",
            _ => "Slate"
        };

        /// <summary>
        /// One sentence saying who each preset is for, shown as the button's tooltip.
        /// </summary>
        /// <param name="preset">The preset to describe.</param>
        /// <returns>The description.</returns>
        public static string Description(EEditorThemePreset preset) => preset switch
        {
            EEditorThemePreset.Ember => "Warm greys under an amber accent. Less blue light than the "
                + "others, for evening work.",
            EEditorThemePreset.Harbor => "Emerald, gold and violet rather than green, amber and red, "
                + "so the three states stay apart with red-green color blindness.",
            EEditorThemePreset.Ink => "Near black on white, heavier hairlines and squarer corners. The "
                + "most legible of the five, for bright rooms, projectors and tired eyes.",
            EEditorThemePreset.Rose => "Muted plum and rose, after the Rosé Pine palette. Low glare "
                + "and warm, for a long session.",
            _ => "Neutral greys under a blue accent. The Base look, retuned so nothing sits below the "
                + "readable floor either way."
        };

        /// <summary>
        /// The colors of one preset for one editor theme.
        /// </summary>
        /// <param name="preset">The look to build.</param>
        /// <param name="isDarkMode">True for the dark mode colors, false for the light mode ones.</param>
        /// <returns>A fresh set, safe for the caller to keep.</returns>
        public static EditorThemeColors CreateColors(EEditorThemePreset preset, bool isDarkMode) => preset switch
        {
            EEditorThemePreset.Ember => isDarkMode
                ? CreateEmberDark()
                : CreateEmberLight(),
            EEditorThemePreset.Harbor => isDarkMode
                ? CreateHarborDark()
                : CreateHarborLight(),
            EEditorThemePreset.Ink => isDarkMode
                ? CreateInkDark()
                : CreateInkLight(),
            EEditorThemePreset.Rose => isDarkMode
                ? CreateRoseDark()
                : CreateRoseLight(),
            _ => isDarkMode
                ? CreateSlateDark()
                : CreateSlateLight()
        };

        private static bool Matches(EditorTheme theme, EEditorThemePreset preset)
        {
            if (theme.DarkColors == null || theme.LightColors == null)
                return false;

            return theme.DarkColors.Matches(CreateColors(preset, true))
                && theme.LightColors.Matches(CreateColors(preset, false));
        }

        // Ink squares the corners and thickens the hairlines: at this contrast a soft edge reads as a
        // smudge rather than as a boundary. The other four keep the built-in layout.
        private static EditorThemeMetrics CreateMetrics(EEditorThemePreset preset)
        {
            if (preset != EEditorThemePreset.Ink)
                return EditorThemeDefaults.CreateMetrics();

            return new EditorThemeMetrics(16f,
                14f,
                2,
                11,
                8f,
                2f,
                20f,
                0.10f,
                14f,
                8f,
                3,
                18f,
                0.12f,
                22f,
                6f,
                12f,
                2f,
                8f,
                4f,
                15);
        }

        /// <summary>The dark editor colors of the Slate preset.</summary>
        /// <returns>A fresh set, safe for the caller to keep.</returns>
        private static EditorThemeColors CreateSlateDark() => new(new Color(0.608f, 0.753f, 0.988f),
            new Color(0.067f, 0.067f, 0.078f),
            new Color(0.169f, 0.169f, 0.192f),
            new Color(1.000f, 1.000f, 1.000f, 0.14f),
            new Color(0.220f, 0.220f, 0.243f),
            new Color(1.000f, 0.655f, 0.627f),
            new Color(0.745f, 0.745f, 0.796f),
            new Color(0.000f, 0.000f, 0.000f, 0.35f),
            new Color(0.129f, 0.129f, 0.149f),
            new Color(1.000f, 0.678f, 0.125f),
            new Color(1.000f, 1.000f, 1.000f, 0.06f),
            new Color(1.000f, 1.000f, 1.000f, 0.10f),
            new Color(0.302f, 0.302f, 0.333f),
            new Color(0.859f, 0.859f, 0.882f),
            new Color(0.608f, 0.753f, 0.988f, 0.90f),
            new Color(0.608f, 0.753f, 0.988f, 0.22f),
            new Color(1.000f, 1.000f, 1.000f, 0.09f),
            new Color(1.000f, 1.000f, 1.000f, 0.035f),
            new Color(0.263f, 0.847f, 0.420f),
            new Color(0.937f, 0.937f, 0.961f),
            new Color(1.000f, 0.678f, 0.125f));

        /// <summary>The light editor colors of the Slate preset.</summary>
        /// <returns>A fresh set, safe for the caller to keep.</returns>
        private static EditorThemeColors CreateSlateLight() => new(new Color(0.090f, 0.396f, 0.800f),
            new Color(1.000f, 1.000f, 1.000f),
            new Color(0.831f, 0.831f, 0.847f),
            new Color(0.000f, 0.000f, 0.000f, 0.20f),
            new Color(0.914f, 0.914f, 0.925f),
            new Color(0.784f, 0.157f, 0.118f),
            new Color(0.400f, 0.400f, 0.427f),
            new Color(0.000f, 0.000f, 0.000f, 0.22f),
            new Color(0.980f, 0.980f, 0.984f),
            new Color(0.596f, 0.357f, 0.000f),
            new Color(0.000f, 0.000f, 0.000f, 0.06f),
            new Color(0.000f, 0.000f, 0.000f, 0.08f),
            new Color(0.878f, 0.878f, 0.894f),
            new Color(0.149f, 0.149f, 0.173f),
            new Color(0.090f, 0.396f, 0.800f, 0.90f),
            new Color(0.090f, 0.396f, 0.800f, 0.18f),
            new Color(0.000f, 0.000f, 0.000f, 0.12f),
            new Color(0.000f, 0.000f, 0.000f, 0.035f),
            new Color(0.047f, 0.471f, 0.227f),
            new Color(0.110f, 0.110f, 0.129f),
            new Color(0.596f, 0.357f, 0.000f));

        /// <summary>The dark editor colors of the Ink preset.</summary>
        /// <returns>A fresh set, safe for the caller to keep.</returns>
        private static EditorThemeColors CreateInkDark() => new(new Color(0.659f, 0.824f, 1.000f),
            new Color(0.067f, 0.067f, 0.078f),
            new Color(0.071f, 0.071f, 0.078f),
            new Color(1.000f, 1.000f, 1.000f, 0.14f),
            new Color(0.110f, 0.110f, 0.122f),
            new Color(1.000f, 0.745f, 0.745f),
            new Color(0.804f, 0.804f, 0.839f),
            new Color(0.000f, 0.000f, 0.000f, 0.35f),
            new Color(0.051f, 0.051f, 0.059f),
            new Color(0.996f, 0.773f, 0.243f),
            new Color(1.000f, 1.000f, 1.000f, 0.06f),
            new Color(1.000f, 1.000f, 1.000f, 0.10f),
            new Color(0.227f, 0.227f, 0.259f),
            new Color(1.000f, 1.000f, 1.000f),
            new Color(0.659f, 0.824f, 1.000f, 0.90f),
            new Color(0.659f, 0.824f, 1.000f, 0.22f),
            new Color(1.000f, 1.000f, 1.000f, 0.09f),
            new Color(1.000f, 1.000f, 1.000f, 0.035f),
            new Color(0.271f, 0.910f, 0.592f),
            new Color(1.000f, 1.000f, 1.000f),
            new Color(0.996f, 0.773f, 0.243f));

        /// <summary>The light editor colors of the Ink preset.</summary>
        /// <returns>A fresh set, safe for the caller to keep.</returns>
        private static EditorThemeColors CreateInkLight() => new(new Color(0.000f, 0.310f, 0.690f),
            new Color(1.000f, 1.000f, 1.000f),
            new Color(1.000f, 1.000f, 1.000f),
            new Color(0.000f, 0.000f, 0.000f, 0.20f),
            new Color(0.969f, 0.969f, 0.973f),
            new Color(0.655f, 0.086f, 0.086f),
            new Color(0.325f, 0.325f, 0.349f),
            new Color(0.000f, 0.000f, 0.000f, 0.22f),
            new Color(1.000f, 1.000f, 1.000f),
            new Color(0.459f, 0.290f, 0.000f),
            new Color(0.000f, 0.000f, 0.000f, 0.06f),
            new Color(0.000f, 0.000f, 0.000f, 0.08f),
            new Color(0.886f, 0.886f, 0.902f),
            new Color(0.000f, 0.000f, 0.000f),
            new Color(0.000f, 0.310f, 0.690f, 0.90f),
            new Color(0.000f, 0.310f, 0.690f, 0.18f),
            new Color(0.000f, 0.000f, 0.000f, 0.12f),
            new Color(0.000f, 0.000f, 0.000f, 0.035f),
            new Color(0.000f, 0.380f, 0.220f),
            new Color(0.000f, 0.000f, 0.000f),
            new Color(0.459f, 0.290f, 0.000f));

        /// <summary>The dark editor colors of the Harbor preset.</summary>
        /// <returns>A fresh set, safe for the caller to keep.</returns>
        private static EditorThemeColors CreateHarborDark() => new(new Color(0.400f, 0.784f, 1.000f),
            new Color(0.067f, 0.067f, 0.078f),
            new Color(0.149f, 0.165f, 0.188f),
            new Color(1.000f, 1.000f, 1.000f, 0.14f),
            new Color(0.204f, 0.224f, 0.251f),
            new Color(0.855f, 0.678f, 0.988f),
            new Color(0.718f, 0.749f, 0.796f),
            new Color(0.000f, 0.000f, 0.000f, 0.35f),
            new Color(0.110f, 0.122f, 0.141f),
            new Color(0.898f, 0.725f, 0.000f),
            new Color(1.000f, 1.000f, 1.000f, 0.06f),
            new Color(1.000f, 1.000f, 1.000f, 0.10f),
            new Color(0.290f, 0.314f, 0.349f),
            new Color(0.863f, 0.878f, 0.902f),
            new Color(0.400f, 0.784f, 1.000f, 0.90f),
            new Color(0.400f, 0.784f, 1.000f, 0.22f),
            new Color(1.000f, 1.000f, 1.000f, 0.09f),
            new Color(1.000f, 1.000f, 1.000f, 0.035f),
            new Color(0.000f, 0.851f, 0.549f),
            new Color(0.929f, 0.941f, 0.957f),
            new Color(0.898f, 0.725f, 0.000f));

        /// <summary>The light editor colors of the Harbor preset.</summary>
        /// <returns>A fresh set, safe for the caller to keep.</returns>
        private static EditorThemeColors CreateHarborLight() => new(new Color(0.000f, 0.420f, 0.725f),
            new Color(1.000f, 1.000f, 1.000f),
            new Color(0.839f, 0.855f, 0.875f),
            new Color(0.000f, 0.000f, 0.000f, 0.20f),
            new Color(0.918f, 0.929f, 0.945f),
            new Color(0.549f, 0.235f, 0.784f),
            new Color(0.400f, 0.424f, 0.463f),
            new Color(0.000f, 0.000f, 0.000f, 0.22f),
            new Color(0.980f, 0.984f, 0.988f),
            new Color(0.506f, 0.408f, 0.000f),
            new Color(0.000f, 0.000f, 0.000f, 0.06f),
            new Color(0.000f, 0.000f, 0.000f, 0.08f),
            new Color(0.875f, 0.890f, 0.910f),
            new Color(0.141f, 0.157f, 0.184f),
            new Color(0.000f, 0.420f, 0.725f, 0.90f),
            new Color(0.000f, 0.420f, 0.725f, 0.18f),
            new Color(0.000f, 0.000f, 0.000f, 0.12f),
            new Color(0.000f, 0.000f, 0.000f, 0.035f),
            new Color(0.000f, 0.471f, 0.341f),
            new Color(0.102f, 0.118f, 0.141f),
            new Color(0.506f, 0.408f, 0.000f));

        /// <summary>The dark editor colors of the Rose preset.</summary>
        /// <returns>A fresh set, safe for the caller to keep.</returns>
        private static EditorThemeColors CreateRoseDark() => new(new Color(0.776f, 0.659f, 0.914f),
            new Color(0.067f, 0.067f, 0.078f),
            new Color(0.098f, 0.090f, 0.141f),
            new Color(1.000f, 1.000f, 1.000f, 0.14f),
            new Color(0.122f, 0.114f, 0.180f),
            new Color(0.992f, 0.588f, 0.702f),
            new Color(0.714f, 0.694f, 0.843f),
            new Color(0.000f, 0.000f, 0.000f, 0.35f),
            new Color(0.086f, 0.078f, 0.122f),
            new Color(0.965f, 0.757f, 0.467f),
            new Color(1.000f, 1.000f, 1.000f, 0.06f),
            new Color(1.000f, 1.000f, 1.000f, 0.10f),
            new Color(0.251f, 0.239f, 0.322f),
            new Color(0.878f, 0.871f, 0.957f),
            new Color(0.776f, 0.659f, 0.914f, 0.90f),
            new Color(0.776f, 0.659f, 0.914f, 0.22f),
            new Color(1.000f, 1.000f, 1.000f, 0.09f),
            new Color(1.000f, 1.000f, 1.000f, 0.035f),
            new Color(0.612f, 0.812f, 0.847f),
            new Color(0.906f, 0.898f, 0.984f),
            new Color(0.965f, 0.757f, 0.467f));

        /// <summary>The light editor colors of the Rose preset.</summary>
        /// <returns>A fresh set, safe for the caller to keep.</returns>
        private static EditorThemeColors CreateRoseLight() => new(new Color(0.498f, 0.420f, 0.584f),
            new Color(1.000f, 1.000f, 1.000f),
            new Color(0.980f, 0.957f, 0.929f),
            new Color(0.000f, 0.000f, 0.000f, 0.20f),
            new Color(1.000f, 0.980f, 0.953f),
            new Color(0.655f, 0.361f, 0.443f),
            new Color(0.451f, 0.435f, 0.549f),
            new Color(0.000f, 0.000f, 0.000f, 0.22f),
            new Color(1.000f, 0.980f, 0.953f),
            new Color(0.608f, 0.408f, 0.133f),
            new Color(0.000f, 0.000f, 0.000f, 0.06f),
            new Color(0.000f, 0.000f, 0.000f, 0.08f),
            new Color(0.875f, 0.855f, 0.851f),
            new Color(0.341f, 0.322f, 0.475f),
            new Color(0.498f, 0.420f, 0.584f, 0.90f),
            new Color(0.498f, 0.420f, 0.584f, 0.18f),
            new Color(0.000f, 0.000f, 0.000f, 0.12f),
            new Color(0.000f, 0.000f, 0.000f, 0.035f),
            new Color(0.157f, 0.412f, 0.514f),
            new Color(0.267f, 0.251f, 0.373f),
            new Color(0.608f, 0.408f, 0.133f));

        /// <summary>The dark editor colors of the Ember preset.</summary>
        /// <returns>A fresh set, safe for the caller to keep.</returns>
        private static EditorThemeColors CreateEmberDark() => new(new Color(0.957f, 0.659f, 0.247f),
            new Color(0.067f, 0.067f, 0.078f),
            new Color(0.129f, 0.125f, 0.114f),
            new Color(1.000f, 1.000f, 1.000f, 0.14f),
            new Color(0.173f, 0.165f, 0.149f),
            new Color(0.992f, 0.624f, 0.565f),
            new Color(0.757f, 0.722f, 0.655f),
            new Color(0.000f, 0.000f, 0.000f, 0.35f),
            new Color(0.102f, 0.098f, 0.090f),
            new Color(0.941f, 0.745f, 0.314f),
            new Color(1.000f, 1.000f, 1.000f, 0.06f),
            new Color(1.000f, 1.000f, 1.000f, 0.10f),
            new Color(0.275f, 0.259f, 0.227f),
            new Color(0.902f, 0.871f, 0.808f),
            new Color(0.957f, 0.659f, 0.247f, 0.90f),
            new Color(0.957f, 0.659f, 0.247f, 0.22f),
            new Color(1.000f, 1.000f, 1.000f, 0.09f),
            new Color(1.000f, 1.000f, 1.000f, 0.035f),
            new Color(0.612f, 0.776f, 0.369f),
            new Color(0.945f, 0.914f, 0.855f),
            new Color(0.941f, 0.745f, 0.314f));

        /// <summary>The light editor colors of the Ember preset.</summary>
        /// <returns>A fresh set, safe for the caller to keep.</returns>
        private static EditorThemeColors CreateEmberLight() => new(new Color(0.651f, 0.365f, 0.000f),
            new Color(1.000f, 1.000f, 1.000f),
            new Color(0.949f, 0.922f, 0.867f),
            new Color(0.000f, 0.000f, 0.000f, 0.20f),
            new Color(0.984f, 0.961f, 0.914f),
            new Color(0.706f, 0.196f, 0.118f),
            new Color(0.471f, 0.431f, 0.373f),
            new Color(0.000f, 0.000f, 0.000f, 0.22f),
            new Color(1.000f, 0.992f, 0.965f),
            new Color(0.561f, 0.412f, 0.000f),
            new Color(0.000f, 0.000f, 0.000f, 0.06f),
            new Color(0.000f, 0.000f, 0.000f, 0.08f),
            new Color(0.902f, 0.863f, 0.784f),
            new Color(0.169f, 0.149f, 0.125f),
            new Color(0.651f, 0.365f, 0.000f, 0.90f),
            new Color(0.651f, 0.365f, 0.000f, 0.18f),
            new Color(0.000f, 0.000f, 0.000f, 0.12f),
            new Color(0.000f, 0.000f, 0.000f, 0.035f),
            new Color(0.314f, 0.471f, 0.157f),
            new Color(0.169f, 0.149f, 0.125f),
            new Color(0.561f, 0.412f, 0.000f));
    }
}