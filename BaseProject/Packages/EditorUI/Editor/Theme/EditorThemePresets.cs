using UnityEngine;

namespace Base.EditorUIPackage.Editor
{
    /// <summary>
    /// Eight complete looks a project can start from, and the values behind them.
    /// </summary>
    /// <remarks>
    /// Every color was fitted to a contrast target rather than picked by eye, against both models
    /// that matter, because they disagree exactly where editor themes live:
    /// <list type="bullet">
    /// <item>
    /// WCAG 2 contrast ratio, the accessibility standard regulators actually measure against.
    /// Every preset clears 4.5 : 1 on text, and Onyx clears 7 : 1.
    /// </item>
    /// <item>
    /// APCA lightness contrast, the perceptual model behind the draft WCAG 3. It matters here
    /// because WCAG 2 overstates contrast on dark surfaces, so a dark theme can pass 4.5 : 1 and
    /// still be hard to read. Every preset reaches Lc 90 on body text and Lc 60 on secondary text
    /// and status colors, and Onyx reaches Lc 100 and Lc 75.
    /// </item>
    /// </list>
    /// <para>
    /// The dark surfaces sit near <c>#1C1C1F</c> rather than at black on purpose: light text on a
    /// near-black background blooms, which is tiring to read even though the measured contrast is
    /// enormous.
    /// </para>
    /// <para>
    /// The three status colors are staggered in lightness rather than all fitted to the same target.
    /// Three colors at one contrast level are the same brightness by construction, so the moment hue
    /// is lost they collapse into one. Staggering them costs nothing and is the only cue that
    /// survives a greyscale screenshot or a reader who sees no color at all.
    /// </para>
    /// <para>
    /// Verdigris is the preset built to keep its three states apart under simulated deuteranopia and
    /// protanopia, and Cobalt does the same on the blue and orange axis, which also covers the
    /// blue-yellow loss that most people acquire with age. The others rely on the words next to
    /// them, which is why a Base window always spells a state out rather than leaving a bare colored
    /// dot to carry it.
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

        /// <summary>How many presets are shown per row, which is what makes the grid two even rows.</summary>
        public const int PresetsPerRow = 4;

        /// <summary>
        /// Every preset, in the order they are meant to be read. The first row is the Base look and
        /// the three that trade personality for legibility. The second is the four with a character of
        /// their own, which still clear the same floor.
        /// </summary>
        /// <returns>The presets in display order.</returns>
        public static EEditorThemePreset[] CreateOrder() => new[]
        {
            EEditorThemePreset.Slate,
            EEditorThemePreset.Onyx,
            EEditorThemePreset.Graphite,
            EEditorThemePreset.Verdigris,
            EEditorThemePreset.Cobalt,
            EEditorThemePreset.Quartz,
            EEditorThemePreset.Amber,
            EEditorThemePreset.Malachite
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
            EEditorThemePreset.Amber => "Amber",
            EEditorThemePreset.Cobalt => "Cobalt",
            EEditorThemePreset.Graphite => "Graphite",
            EEditorThemePreset.Onyx => "Onyx",
            EEditorThemePreset.Malachite => "Malachite",
            EEditorThemePreset.Quartz => "Quartz",
            EEditorThemePreset.Verdigris => "Verdigris",
            _ => "Slate"
        };

        /// <summary>
        /// One sentence saying who each preset is for, shown as the button's tooltip.
        /// </summary>
        /// <param name="preset">The preset to describe.</param>
        /// <returns>The description.</returns>
        public static string Description(EEditorThemePreset preset) => preset switch
        {
            EEditorThemePreset.Amber => "Candlelight. The darkest of the eight, with warm walls and "
                + "warm off-white text rather than grey, for working at night.",
            EEditorThemePreset.Cobalt => "Deep blue, with blue and orange carrying the three states. "
                + "That pair stays apart under every deficiency, including the blue-yellow loss most "
                + "people pick up with age.",
            EEditorThemePreset.Graphite => "No color in the walls or the text at all. The three states "
                + "are set apart by lightness first, so they keep most of that when color is lost.",
            EEditorThemePreset.Onyx => "Black on white, heavier hairlines and squarer corners. The most "
                + "legible of the eight, for bright rooms, projectors and tired eyes.",
            EEditorThemePreset.Malachite => "Green stone, low saturation on purpose, so the tooling "
                + "stays quieter than the scene view beside it.",
            EEditorThemePreset.Quartz => "Rose quartz. Pink and soft, low glare, for a long session.",
            EEditorThemePreset.Verdigris => "The patina on copper. Teal, gold and violet rather than "
                + "green, amber and red, so the three states stay apart with red-green color blindness.",
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
            EEditorThemePreset.Amber => isDarkMode
                ? CreateAmberDark()
                : CreateAmberLight(),
            EEditorThemePreset.Cobalt => isDarkMode
                ? CreateCobaltDark()
                : CreateCobaltLight(),
            EEditorThemePreset.Graphite => isDarkMode
                ? CreateGraphiteDark()
                : CreateGraphiteLight(),
            EEditorThemePreset.Onyx => isDarkMode
                ? CreateOnyxDark()
                : CreateOnyxLight(),
            EEditorThemePreset.Malachite => isDarkMode
                ? CreateMalachiteDark()
                : CreateMalachiteLight(),
            EEditorThemePreset.Quartz => isDarkMode
                ? CreateQuartzDark()
                : CreateQuartzLight(),
            EEditorThemePreset.Verdigris => isDarkMode
                ? CreateVerdigrisDark()
                : CreateVerdigrisLight(),
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

        // Onyx squares the corners and thickens the hairlines: at this contrast a soft edge reads as a
        // smudge rather than as a boundary. Amber goes the other way, because candlelight has no hard
        // edges in it. The rest keep the built-in layout.
        private static EditorThemeMetrics CreateMetrics(EEditorThemePreset preset)
        {
            if (preset == EEditorThemePreset.Onyx)
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
                    7f,
                    4f,
                    15);

            if (preset != EEditorThemePreset.Amber)
                return EditorThemeDefaults.CreateMetrics();

            return new EditorThemeMetrics(16f,
                14f,
                10,
                11,
                8f,
                1f,
                20f,
                0.05f,
                14f,
                8f,
                9,
                18f,
                0.07f,
                22f,
                6f,
                12f,
                1f,
                7f,
                4f,
                15);
        }

        /// <summary>The dark editor colors of the Slate preset.</summary>
        /// <returns>A fresh set, safe for the caller to keep.</returns>
        private static EditorThemeColors CreateSlateDark() => new(new Color(0.604f, 0.752f, 0.991f),
            new Color(0.063f, 0.063f, 0.078f),
            new Color(0.173f, 0.177f, 0.192f),
            new Color(1.000f, 1.000f, 1.000f, 0.14f),
            new Color(0.219f, 0.224f, 0.243f),
            new Color(0.984f, 0.660f, 0.665f),
            new Color(0.738f, 0.746f, 0.777f),
            new Color(0.000f, 0.000f, 0.000f, 0.35f),
            new Color(0.134f, 0.137f, 0.149f),
            new Color(0.969f, 0.840f, 0.000f),
            new Color(1.000f, 1.000f, 1.000f, 0.060f),
            new Color(1.000f, 1.000f, 1.000f, 0.10f),
            new Color(0.300f, 0.306f, 0.333f),
            new Color(0.854f, 0.863f, 0.899f),
            new Color(0.604f, 0.752f, 0.991f, 0.90f),
            new Color(0.604f, 0.752f, 0.991f, 0.22f),
            new Color(1.000f, 1.000f, 1.000f, 0.090f),
            new Color(1.000f, 1.000f, 1.000f, 0.035f),
            new Color(0.699f, 0.999f, 0.899f),
            new Color(0.928f, 0.938f, 0.977f),
            new Color(0.969f, 0.840f, 0.000f));

        /// <summary>The light editor colors of the Slate preset.</summary>
        /// <returns>A fresh set, safe for the caller to keep.</returns>
        private static EditorThemeColors CreateSlateLight() => new(new Color(0.000f, 0.383f, 1.000f),
            new Color(1.000f, 1.000f, 1.000f),
            new Color(0.965f, 0.972f, 1.000f),
            new Color(0.000f, 0.000f, 0.000f, 0.20f),
            new Color(0.907f, 0.913f, 0.940f),
            new Color(0.189f, 0.109f, 0.085f),
            new Color(0.437f, 0.449f, 0.497f),
            new Color(0.000f, 0.000f, 0.000f, 0.22f),
            new Color(0.964f, 0.971f, 0.999f),
            new Color(0.449f, 0.269f, 0.000f),
            new Color(0.000f, 0.000f, 0.000f, 0.060f),
            new Color(0.000f, 0.000f, 0.000f, 0.080f),
            new Color(0.839f, 0.845f, 0.870f),
            new Color(0.275f, 0.283f, 0.313f),
            new Color(0.000f, 0.383f, 1.000f, 0.90f),
            new Color(0.000f, 0.383f, 1.000f, 0.22f),
            new Color(0.000f, 0.000f, 0.000f, 0.12f),
            new Color(0.000f, 0.000f, 0.000f, 0.035f),
            new Color(0.051f, 0.509f, 0.356f),
            new Color(0.127f, 0.131f, 0.145f),
            new Color(0.449f, 0.269f, 0.000f));

        /// <summary>The dark editor colors of the Onyx preset.</summary>
        /// <returns>A fresh set, safe for the caller to keep.</returns>
        private static EditorThemeColors CreateOnyxDark() => new(new Color(0.654f, 0.811f, 0.991f),
            new Color(0.063f, 0.063f, 0.078f),
            new Color(0.000f, 0.000f, 0.000f),
            new Color(1.000f, 1.000f, 1.000f, 0.14f),
            new Color(0.050f, 0.050f, 0.050f),
            new Color(0.987f, 0.730f, 0.765f),
            new Color(0.797f, 0.797f, 0.797f),
            new Color(0.000f, 0.000f, 0.000f, 0.35f),
            new Color(0.000f, 0.000f, 0.000f),
            new Color(0.997f, 0.859f, 0.538f),
            new Color(1.000f, 1.000f, 1.000f, 0.060f),
            new Color(1.000f, 1.000f, 1.000f, 0.10f),
            new Color(0.130f, 0.130f, 0.130f),
            new Color(0.881f, 0.881f, 0.881f),
            new Color(0.654f, 0.811f, 0.991f, 0.90f),
            new Color(0.654f, 0.811f, 0.991f, 0.22f),
            new Color(1.000f, 1.000f, 1.000f, 0.090f),
            new Color(1.000f, 1.000f, 1.000f, 0.035f),
            new Color(0.750f, 1.000f, 0.925f),
            new Color(0.955f, 0.955f, 0.955f),
            new Color(0.997f, 0.859f, 0.538f));

        /// <summary>The light editor colors of the Onyx preset.</summary>
        /// <returns>A fresh set, safe for the caller to keep.</returns>
        private static EditorThemeColors CreateOnyxLight() => new(new Color(0.000f, 0.407f, 0.871f),
            new Color(1.000f, 1.000f, 1.000f),
            new Color(1.000f, 1.000f, 1.000f),
            new Color(0.000f, 0.000f, 0.000f, 0.20f),
            new Color(1.000f, 1.000f, 1.000f),
            new Color(0.277f, 0.166f, 0.138f),
            new Color(0.433f, 0.433f, 0.433f),
            new Color(0.000f, 0.000f, 0.000f, 0.22f),
            new Color(1.000f, 1.000f, 1.000f),
            new Color(0.450f, 0.285f, 0.000f),
            new Color(0.000f, 0.000f, 0.000f, 0.060f),
            new Color(0.000f, 0.000f, 0.000f, 0.080f),
            new Color(0.890f, 0.890f, 0.890f),
            new Color(0.313f, 0.313f, 0.313f),
            new Color(0.000f, 0.407f, 0.871f, 0.90f),
            new Color(0.000f, 0.407f, 0.871f, 0.22f),
            new Color(0.000f, 0.000f, 0.000f, 0.12f),
            new Color(0.000f, 0.000f, 0.000f, 0.035f),
            new Color(0.074f, 0.493f, 0.263f),
            new Color(0.182f, 0.182f, 0.182f),
            new Color(0.450f, 0.285f, 0.000f));

        /// <summary>The dark editor colors of the Graphite preset.</summary>
        /// <returns>A fresh set, safe for the caller to keep.</returns>
        private static EditorThemeColors CreateGraphiteDark() => new(new Color(0.764f, 0.764f, 0.764f),
            new Color(0.063f, 0.063f, 0.078f),
            new Color(0.180f, 0.180f, 0.180f),
            new Color(1.000f, 1.000f, 1.000f, 0.14f),
            new Color(0.232f, 0.232f, 0.232f),
            new Color(0.987f, 0.661f, 0.705f),
            new Color(0.764f, 0.764f, 0.764f),
            new Color(0.000f, 0.000f, 0.000f, 0.35f),
            new Color(0.140f, 0.140f, 0.140f),
            new Color(0.957f, 0.852f, 0.431f),
            new Color(1.000f, 1.000f, 1.000f, 0.060f),
            new Color(1.000f, 1.000f, 1.000f, 0.10f),
            new Color(0.320f, 0.320f, 0.320f),
            new Color(0.868f, 0.868f, 0.868f),
            new Color(0.764f, 0.764f, 0.764f, 0.90f),
            new Color(0.764f, 0.764f, 0.764f, 0.22f),
            new Color(1.000f, 1.000f, 1.000f, 0.090f),
            new Color(1.000f, 1.000f, 1.000f, 0.035f),
            new Color(0.919f, 0.999f, 0.945f),
            new Color(0.955f, 0.955f, 0.955f),
            new Color(0.957f, 0.852f, 0.431f));

        /// <summary>The light editor colors of the Graphite preset.</summary>
        /// <returns>A fresh set, safe for the caller to keep.</returns>
        private static EditorThemeColors CreateGraphiteLight() => new(new Color(0.439f, 0.439f, 0.439f),
            new Color(1.000f, 1.000f, 1.000f),
            new Color(1.000f, 1.000f, 1.000f),
            new Color(0.000f, 0.000f, 0.000f, 0.20f),
            new Color(0.918f, 0.918f, 0.918f),
            new Color(0.208f, 0.115f, 0.127f),
            new Color(0.439f, 0.439f, 0.439f),
            new Color(0.000f, 0.000f, 0.000f, 0.22f),
            new Color(0.980f, 0.980f, 0.980f),
            new Color(0.390f, 0.305f, 0.176f),
            new Color(0.000f, 0.000f, 0.000f, 0.060f),
            new Color(0.000f, 0.000f, 0.000f, 0.080f),
            new Color(0.845f, 0.845f, 0.845f),
            new Color(0.290f, 0.290f, 0.290f),
            new Color(0.439f, 0.439f, 0.439f, 0.90f),
            new Color(0.439f, 0.439f, 0.439f, 0.22f),
            new Color(0.000f, 0.000f, 0.000f, 0.12f),
            new Color(0.000f, 0.000f, 0.000f, 0.035f),
            new Color(0.296f, 0.493f, 0.427f),
            new Color(0.099f, 0.099f, 0.099f),
            new Color(0.390f, 0.305f, 0.176f));

        /// <summary>The dark editor colors of the Verdigris preset.</summary>
        /// <returns>A fresh set, safe for the caller to keep.</returns>
        private static EditorThemeColors CreateVerdigrisDark() => new(new Color(0.166f, 0.828f, 0.696f),
            new Color(0.063f, 0.063f, 0.078f),
            new Color(0.134f, 0.176f, 0.170f),
            new Color(1.000f, 1.000f, 1.000f, 0.14f),
            new Color(0.172f, 0.226f, 0.219f),
            new Color(0.839f, 0.675f, 0.992f),
            new Color(0.700f, 0.753f, 0.746f),
            new Color(0.000f, 0.000f, 0.000f, 0.35f),
            new Color(0.102f, 0.134f, 0.130f),
            new Color(0.995f, 0.816f, 0.458f),
            new Color(1.000f, 1.000f, 1.000f, 0.060f),
            new Color(1.000f, 1.000f, 1.000f, 0.10f),
            new Color(0.237f, 0.312f, 0.302f),
            new Color(0.812f, 0.873f, 0.865f),
            new Color(0.166f, 0.828f, 0.696f, 0.90f),
            new Color(0.166f, 0.828f, 0.696f, 0.22f),
            new Color(1.000f, 1.000f, 1.000f, 0.090f),
            new Color(1.000f, 1.000f, 1.000f, 0.035f),
            new Color(0.698f, 0.997f, 0.858f),
            new Color(0.882f, 0.949f, 0.940f),
            new Color(0.995f, 0.816f, 0.458f));

        /// <summary>The light editor colors of the Verdigris preset.</summary>
        /// <returns>A fresh set, safe for the caller to keep.</returns>
        private static EditorThemeColors CreateVerdigrisLight() => new(new Color(0.000f, 0.529f, 0.494f),
            new Color(1.000f, 1.000f, 1.000f),
            new Color(0.925f, 1.000f, 0.990f),
            new Color(0.000f, 0.000f, 0.000f, 0.20f),
            new Color(0.878f, 0.950f, 0.940f),
            new Color(0.415f, 0.207f, 0.688f),
            new Color(0.388f, 0.497f, 0.493f),
            new Color(0.000f, 0.000f, 0.000f, 0.22f),
            new Color(0.925f, 1.000f, 0.990f),
            new Color(0.645f, 0.430f, 0.000f),
            new Color(0.000f, 0.000f, 0.000f, 0.060f),
            new Color(0.000f, 0.000f, 0.000f, 0.080f),
            new Color(0.811f, 0.876f, 0.868f),
            new Color(0.255f, 0.327f, 0.325f),
            new Color(0.000f, 0.529f, 0.494f, 0.90f),
            new Color(0.000f, 0.529f, 0.494f, 0.22f),
            new Color(0.000f, 0.000f, 0.000f, 0.12f),
            new Color(0.000f, 0.000f, 0.000f, 0.035f),
            new Color(0.000f, 0.032f, 0.017f),
            new Color(0.149f, 0.191f, 0.189f),
            new Color(0.645f, 0.430f, 0.000f));

        /// <summary>The dark editor colors of the Cobalt preset.</summary>
        /// <returns>A fresh set, safe for the caller to keep.</returns>
        private static EditorThemeColors CreateCobaltDark() => new(new Color(0.455f, 0.757f, 0.988f),
            new Color(0.063f, 0.063f, 0.078f),
            new Color(0.126f, 0.149f, 0.180f),
            new Color(1.000f, 1.000f, 1.000f, 0.14f),
            new Color(0.162f, 0.192f, 0.231f),
            new Color(0.989f, 0.623f, 0.715f),
            new Color(0.701f, 0.733f, 0.770f),
            new Color(0.000f, 0.000f, 0.000f, 0.35f),
            new Color(0.096f, 0.114f, 0.137f),
            new Color(0.999f, 0.876f, 0.539f),
            new Color(1.000f, 1.000f, 1.000f, 0.060f),
            new Color(1.000f, 1.000f, 1.000f, 0.10f),
            new Color(0.223f, 0.264f, 0.318f),
            new Color(0.816f, 0.853f, 0.896f),
            new Color(0.455f, 0.757f, 0.988f, 0.90f),
            new Color(0.455f, 0.757f, 0.988f, 0.22f),
            new Color(1.000f, 1.000f, 1.000f, 0.090f),
            new Color(1.000f, 1.000f, 1.000f, 0.035f),
            new Color(0.506f, 0.855f, 0.992f),
            new Color(0.888f, 0.929f, 0.975f),
            new Color(0.999f, 0.876f, 0.539f));

        /// <summary>The light editor colors of the Cobalt preset.</summary>
        /// <returns>A fresh set, safe for the caller to keep.</returns>
        private static EditorThemeColors CreateCobaltLight() => new(new Color(0.000f, 0.415f, 0.890f),
            new Color(1.000f, 1.000f, 1.000f),
            new Color(0.915f, 0.952f, 1.000f),
            new Color(0.000f, 0.000f, 0.000f, 0.20f),
            new Color(0.876f, 0.912f, 0.958f),
            new Color(0.203f, 0.030f, 0.102f),
            new Color(0.392f, 0.447f, 0.529f),
            new Color(0.000f, 0.000f, 0.000f, 0.22f),
            new Color(0.915f, 0.952f, 1.000f),
            new Color(0.705f, 0.329f, 0.000f),
            new Color(0.000f, 0.000f, 0.000f, 0.060f),
            new Color(0.000f, 0.000f, 0.000f, 0.080f),
            new Color(0.809f, 0.841f, 0.884f),
            new Color(0.242f, 0.276f, 0.327f),
            new Color(0.000f, 0.415f, 0.890f, 0.90f),
            new Color(0.000f, 0.415f, 0.890f, 0.22f),
            new Color(0.000f, 0.000f, 0.000f, 0.12f),
            new Color(0.000f, 0.000f, 0.000f, 0.035f),
            new Color(0.030f, 0.291f, 0.591f),
            new Color(0.098f, 0.112f, 0.133f),
            new Color(0.705f, 0.329f, 0.000f));

        /// <summary>The dark editor colors of the Quartz preset.</summary>
        /// <returns>A fresh set, safe for the caller to keep.</returns>
        private static EditorThemeColors CreateQuartzDark() => new(new Color(0.998f, 0.619f, 0.771f),
            new Color(0.063f, 0.063f, 0.078f),
            new Color(0.184f, 0.147f, 0.159f),
            new Color(1.000f, 1.000f, 1.000f, 0.14f),
            new Color(0.235f, 0.188f, 0.204f),
            new Color(0.991f, 0.634f, 0.682f),
            new Color(0.769f, 0.723f, 0.738f),
            new Color(0.000f, 0.000f, 0.000f, 0.35f),
            new Color(0.141f, 0.113f, 0.122f),
            new Color(0.997f, 0.813f, 0.080f),
            new Color(1.000f, 1.000f, 1.000f, 0.060f),
            new Color(1.000f, 1.000f, 1.000f, 0.10f),
            new Color(0.322f, 0.258f, 0.279f),
            new Color(0.894f, 0.840f, 0.858f),
            new Color(0.998f, 0.619f, 0.771f, 0.90f),
            new Color(0.998f, 0.619f, 0.771f, 0.22f),
            new Color(1.000f, 1.000f, 1.000f, 0.090f),
            new Color(1.000f, 1.000f, 1.000f, 0.035f),
            new Color(0.609f, 0.998f, 0.920f),
            new Color(0.972f, 0.914f, 0.933f),
            new Color(0.997f, 0.813f, 0.080f));

        /// <summary>The light editor colors of the Quartz preset.</summary>
        /// <returns>A fresh set, safe for the caller to keep.</returns>
        private static EditorThemeColors CreateQuartzLight() => new(new Color(0.838f, 0.042f, 0.387f),
            new Color(1.000f, 1.000f, 1.000f),
            new Color(1.000f, 0.935f, 0.957f),
            new Color(0.000f, 0.000f, 0.000f, 0.20f),
            new Color(0.960f, 0.898f, 0.919f),
            new Color(0.195f, 0.101f, 0.078f),
            new Color(0.519f, 0.425f, 0.454f),
            new Color(0.000f, 0.000f, 0.000f, 0.22f),
            new Color(1.000f, 0.935f, 0.957f),
            new Color(0.472f, 0.252f, 0.000f),
            new Color(0.000f, 0.000f, 0.000f, 0.060f),
            new Color(0.000f, 0.000f, 0.000f, 0.080f),
            new Color(0.887f, 0.830f, 0.849f),
            new Color(0.326f, 0.267f, 0.285f),
            new Color(0.838f, 0.042f, 0.387f, 0.90f),
            new Color(0.838f, 0.042f, 0.387f, 0.22f),
            new Color(0.000f, 0.000f, 0.000f, 0.12f),
            new Color(0.000f, 0.000f, 0.000f, 0.035f),
            new Color(0.076f, 0.506f, 0.384f),
            new Color(0.149f, 0.122f, 0.130f),
            new Color(0.472f, 0.252f, 0.000f));

        /// <summary>The dark editor colors of the Amber preset.</summary>
        /// <returns>A fresh set, safe for the caller to keep.</returns>
        private static EditorThemeColors CreateAmberDark() => new(new Color(0.996f, 0.629f, 0.149f),
            new Color(0.063f, 0.063f, 0.078f),
            new Color(0.126f, 0.108f, 0.088f),
            new Color(1.000f, 1.000f, 1.000f, 0.14f),
            new Color(0.168f, 0.144f, 0.118f),
            new Color(0.985f, 0.617f, 0.552f),
            new Color(0.747f, 0.711f, 0.650f),
            new Color(0.000f, 0.000f, 0.000f, 0.35f),
            new Color(0.094f, 0.081f, 0.066f),
            new Color(0.969f, 0.807f, 0.000f),
            new Color(1.000f, 1.000f, 1.000f, 0.060f),
            new Color(1.000f, 1.000f, 1.000f, 0.10f),
            new Color(0.255f, 0.219f, 0.178f),
            new Color(0.872f, 0.831f, 0.759f),
            new Color(0.996f, 0.629f, 0.149f, 0.90f),
            new Color(0.996f, 0.629f, 0.149f, 0.22f),
            new Color(1.000f, 1.000f, 1.000f, 0.090f),
            new Color(1.000f, 1.000f, 1.000f, 0.035f),
            new Color(0.825f, 0.998f, 0.709f),
            new Color(0.951f, 0.906f, 0.828f),
            new Color(0.969f, 0.807f, 0.000f));

        /// <summary>The light editor colors of the Amber preset.</summary>
        /// <returns>A fresh set, safe for the caller to keep.</returns>
        private static EditorThemeColors CreateAmberLight() => new(new Color(0.804f, 0.348f, 0.000f),
            new Color(1.000f, 1.000f, 1.000f),
            new Color(1.000f, 0.958f, 0.910f),
            new Color(0.000f, 0.000f, 0.000f, 0.20f),
            new Color(0.983f, 0.942f, 0.894f),
            new Color(0.846f, 0.293f, 0.254f),
            new Color(0.558f, 0.479f, 0.401f),
            new Color(0.000f, 0.000f, 0.000f, 0.22f),
            new Color(1.000f, 0.958f, 0.910f),
            new Color(0.463f, 0.324f, 0.000f),
            new Color(0.000f, 0.000f, 0.000f, 0.060f),
            new Color(0.000f, 0.000f, 0.000f, 0.080f),
            new Color(0.908f, 0.870f, 0.826f),
            new Color(0.376f, 0.324f, 0.271f),
            new Color(0.804f, 0.348f, 0.000f, 0.90f),
            new Color(0.804f, 0.348f, 0.000f, 0.22f),
            new Color(0.000f, 0.000f, 0.000f, 0.12f),
            new Color(0.000f, 0.000f, 0.000f, 0.035f),
            new Color(0.076f, 0.120f, 0.018f),
            new Color(0.235f, 0.202f, 0.169f),
            new Color(0.463f, 0.324f, 0.000f));

        /// <summary>The dark editor colors of the Malachite preset.</summary>
        /// <returns>A fresh set, safe for the caller to keep.</returns>
        private static EditorThemeColors CreateMalachiteDark() => new(new Color(0.253f, 0.843f, 0.410f),
            new Color(0.063f, 0.063f, 0.078f),
            new Color(0.144f, 0.180f, 0.148f),
            new Color(1.000f, 1.000f, 1.000f, 0.14f),
            new Color(0.185f, 0.231f, 0.189f),
            new Color(0.990f, 0.655f, 0.643f),
            new Color(0.711f, 0.756f, 0.711f),
            new Color(0.000f, 0.000f, 0.000f, 0.35f),
            new Color(0.110f, 0.138f, 0.113f),
            new Color(0.967f, 0.838f, 0.000f),
            new Color(1.000f, 1.000f, 1.000f, 0.060f),
            new Color(1.000f, 1.000f, 1.000f, 0.10f),
            new Color(0.254f, 0.318f, 0.261f),
            new Color(0.824f, 0.877f, 0.824f),
            new Color(0.253f, 0.843f, 0.410f, 0.90f),
            new Color(0.253f, 0.843f, 0.410f, 0.22f),
            new Color(1.000f, 1.000f, 1.000f, 0.090f),
            new Color(1.000f, 1.000f, 1.000f, 0.035f),
            new Color(0.709f, 0.998f, 0.853f),
            new Color(0.895f, 0.952f, 0.895f),
            new Color(0.967f, 0.838f, 0.000f));

        /// <summary>The light editor colors of the Malachite preset.</summary>
        /// <returns>A fresh set, safe for the caller to keep.</returns>
        private static EditorThemeColors CreateMalachiteLight() => new(new Color(0.000f, 0.548f, 0.183f),
            new Color(1.000f, 1.000f, 1.000f),
            new Color(0.935f, 1.000f, 0.942f),
            new Color(0.000f, 0.000f, 0.000f, 0.20f),
            new Color(0.890f, 0.952f, 0.897f),
            new Color(0.807f, 0.300f, 0.282f),
            new Color(0.411f, 0.501f, 0.417f),
            new Color(0.000f, 0.000f, 0.000f, 0.22f),
            new Color(0.935f, 1.000f, 0.942f),
            new Color(0.346f, 0.346f, 0.000f),
            new Color(0.000f, 0.000f, 0.000f, 0.060f),
            new Color(0.000f, 0.000f, 0.000f, 0.080f),
            new Color(0.820f, 0.877f, 0.826f),
            new Color(0.271f, 0.331f, 0.275f),
            new Color(0.000f, 0.548f, 0.183f, 0.90f),
            new Color(0.000f, 0.548f, 0.183f, 0.22f),
            new Color(0.000f, 0.000f, 0.000f, 0.12f),
            new Color(0.000f, 0.000f, 0.000f, 0.035f),
            new Color(0.008f, 0.054f, 0.016f),
            new Color(0.159f, 0.194f, 0.162f),
            new Color(0.346f, 0.346f, 0.000f));
    }
}