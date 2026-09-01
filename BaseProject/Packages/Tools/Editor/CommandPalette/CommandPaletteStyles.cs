using Base.EditorUiPackage;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.CommandPalette
{
    /// <summary>
    /// Every size, color and style the palette draws with. Styles are built on first use because
    /// <see cref="EditorStyles"/> is only valid inside a GUI call, and they are dropped again when
    /// the editor theme changes so the palette never keeps a dark color on a light background.
    /// <para>
    /// The shared editor look lives in <see cref="EditorPalette"/>; what stays here are the sizes
    /// and colors that only mean something in a palette, such as the chip of a result kind.
    /// </para>
    /// </summary>
    internal static class CommandPaletteStyles
    {
        /// <summary>Thickness of the outline around the search box.</summary>
        internal const float BorderWidth = 1f;

        /// <summary>Height of the small colored chip in front of a result.</summary>
        internal const float ChipHeight = 16f;

        /// <summary>Width of the small colored chip in front of a result.</summary>
        internal const float ChipWidth = 40f;

        /// <summary>Corner radius of the boxes.</summary>
        internal const float CornerRadius = 6f;

        /// <summary>Closing tag of a dimmed run.</summary>
        internal const string DimClose = "</color>";

        /// <summary>Height of the hint bar at the bottom of the window.</summary>
        internal const float FooterHeight = 20f;

        /// <summary>Space between two neighboring blocks.</summary>
        internal const float Gap = 8f;

        /// <summary>Closing tag of a matched run.</summary>
        internal const string MatchClose = "</color></b>";

        private const int PathFontSize = 12;

        /// <summary>Height of a pill shaped button.</summary>
        internal const float PillHeight = 22f;

        /// <summary>Horizontal padding inside a pill.</summary>
        internal const float PillPadding = 8f;

        /// <summary>Corner radius of the pills.</summary>
        internal const float PillRadius = 8f;

        /// <summary>Height of a single result row.</summary>
        internal const float RowHeight = 42f;

        /// <summary>Horizontal padding inside a result row.</summary>
        internal const float RowInset = 12f;

        /// <summary>Width Unity reserves for a vertical scrollbar.</summary>
        internal const float ScrollbarWidth = 15f;

        private const int SearchFontSize = 15;

        /// <summary>Height of the search box.</summary>
        internal const float SearchHeight = 34f;

        /// <summary>Edge length of the magnifier drawn inside the search box.</summary>
        internal const float SearchIconSize = 14f;

        /// <summary>Space above and below every hairline.</summary>
        internal const float SeparatorGap = 5f;

        /// <summary>Padding between the window edge and its content.</summary>
        internal const float WindowPadding = 10f;

        private static GUIStyle _badgeLabel;
        private static GUIStyle _chipLabel;
        private static GUIStyle _countLabel;
        private static GUIStyle _detailLabel;
        private static GUIStyle _emptyLabel;
        private static GUIStyle _hintLabel;
        private static GUIStyle _keyCapLabel;
        private static GUIStyle _pathLabel;
        private static GUIStyle _pillLabel;
        private static GUIStyle _pinLabel;
        private static GUIStyle _placeholder;
        private static GUIStyle _prefixLabel;
        private static GUIStyle _searchField;
        private static GUIStyle _tagLabel;

        private static readonly EditorStyleWatch Watch = new();

        /// <summary>Thickness of a hairline.</summary>
        internal static float SeparatorThickness => EditorMetrics.SeparatorThickness;

        /// <summary>Opening tag of a dimmed run.</summary>
        internal static string DimOpen => EditorThemeProvider.IsDarkMode
            ? "<color=#7E7E86>"
            : "<color=#87878F>";

        /// <summary>Opening tag of a matched run.</summary>
        internal static string MatchOpen => EditorThemeProvider.IsDarkMode
            ? "<b><color=#7FC0FF>"
            : "<b><color=#0E4FA8>";

        /// <summary>Text the query is typed into.</summary>
        internal static GUIStyle SearchField => _searchField ??= Pin(new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = SearchFontSize
        }, TextColor());

        /// <summary>Dimmed hint drawn inside an empty search box.</summary>
        internal static GUIStyle Placeholder => _placeholder ??= Pin(new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = SearchFontSize
        }, DimColor());

        /// <summary>Label in front of the field while tags are edited.</summary>
        internal static GUIStyle PrefixLabel => _prefixLabel ??= Pin(new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = SearchFontSize,
            fontStyle = FontStyle.Bold
        }, PinColor());

        /// <summary>Rich text label of the menu path.</summary>
        internal static GUIStyle PathLabel => _pathLabel ??= Pin(new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = PathFontSize,
            richText = true
        }, TextColor());

        /// <summary>Dimmed label of the declaring type.</summary>
        internal static GUIStyle DetailLabel => _detailLabel ??= Pin(new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleLeft
        }, DimColor());

        /// <summary>Centered white label inside a colored chip.</summary>
        internal static GUIStyle ChipLabel => _chipLabel ??= Pin(new GUIStyle(EditorStyles.miniBoldLabel)
        {
            alignment = TextAnchor.MiddleCenter
        }, Color.white);

        /// <summary>Centered label inside a tag pill.</summary>
        internal static GUIStyle TagLabel => _tagLabel ??= Pin(new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter
        }, DimColor());

        /// <summary>Centered label inside a pill button.</summary>
        internal static GUIStyle PillLabel => _pillLabel ??= Pin(new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter
        }, TextColor());

        /// <summary>Centered label inside a keyboard cap.</summary>
        internal static GUIStyle KeyCapLabel => _keyCapLabel ??= Pin(new GUIStyle(EditorStyles.miniBoldLabel)
        {
            alignment = TextAnchor.MiddleCenter
        }, TextColor());

        /// <summary>Centered notice shown when nothing matches.</summary>
        internal static GUIStyle EmptyLabel => _emptyLabel ??= Pin(new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = SearchFontSize
        }, DimColor());

        /// <summary>Dimmed label of the footer hints.</summary>
        internal static GUIStyle HintLabel => _hintLabel ??= Pin(new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleLeft
        }, DimColor());

        /// <summary>Right aligned label of the result count.</summary>
        internal static GUIStyle CountLabel => _countLabel ??= Pin(new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleRight
        }, DimColor());

        /// <summary>Right aligned label of the origin badge.</summary>
        internal static GUIStyle BadgeLabel => _badgeLabel ??= Pin(new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleRight
        }, DimColor());

        /// <summary>Centered label of the pin marker.</summary>
        internal static GUIStyle PinLabel => _pinLabel ??= Pin(new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter
        }, PinColor());

        /// <summary>Drops every cached style after either theme changes. Call once per GUI pass.</summary>
        internal static void EnsureFresh()
        {
            if (!Watch.IsStale)
                return;

            _badgeLabel = null;
            _chipLabel = null;
            _countLabel = null;
            _detailLabel = null;
            _emptyLabel = null;
            _hintLabel = null;
            _keyCapLabel = null;
            _pathLabel = null;
            _pillLabel = null;
            _pinLabel = null;
            _placeholder = null;
            _prefixLabel = null;
            _searchField = null;
            _tagLabel = null;

            Watch.MarkFresh();
        }

        /// <summary>Blue accent used for the selection and the active pill.</summary>
        internal static Color AccentColor() => EditorPalette.Accent;

        /// <summary>Fill behind the whole window.</summary>
        internal static Color BackgroundColor() => EditorPalette.Background;

        /// <summary>Border of the search box.</summary>
        internal static Color BorderColor() => EditorPalette.Border;

        /// <summary>Dimmed text used for secondary information.</summary>
        private static Color DimColor() => EditorPalette.DimText;

        /// <summary>Fill of the search box.</summary>
        internal static Color FieldColor() => EditorPalette.Field;

        /// <summary>Fill of a keyboard cap in the footer.</summary>
        internal static Color KeyCapColor() => EditorPalette.KeyCap;

        /// <summary>Chip color of an asset creation entry.</summary>
        internal static Color NewChipColor()
            => EditorPalette.Pick(new Color(0.27f, 0.58f, 0.41f), new Color(0.20f, 0.52f, 0.36f));

        /// <summary>Fill of a pill button in its current state.</summary>
        /// <param name="active">Whether the button is switched on.</param>
        /// <param name="hover">Whether the mouse sits on it.</param>
        /// <param name="pressed">Whether it is being held down.</param>
        /// <returns>The fill color to draw.</returns>
        internal static Color PillColor(bool active, bool hover, bool pressed)
        {
            if (active)
                return Shade(AccentColor(), hover, pressed);

            if (pressed)
                return EditorPalette.Tint(0.20f, 0.17f);

            return hover
                ? EditorPalette.Tint(0.14f, 0.12f)
                : EditorPalette.Tint(0.08f, 0.07f);
        }

        /// <summary>Amber used for pins and for the tag editor.</summary>
        internal static Color PinColor() => EditorPalette.Focus;

        /// <summary>Fill of the row the mouse hovers.</summary>
        internal static Color RowHoverColor() => EditorPalette.Hover;

        /// <summary>Fill of the row the keyboard selection sits on.</summary>
        internal static Color RowSelectedColor() => EditorPalette.SelectionFill;

        /// <summary>Chip color of a menu item entry.</summary>
        internal static Color RunChipColor()
            => EditorPalette.Pick(new Color(0.28f, 0.50f, 0.78f), new Color(0.24f, 0.46f, 0.76f));

        /// <summary>Hairline between the blocks of the window.</summary>
        internal static Color SeparatorColor() => EditorPalette.Separator;

        /// <summary>Chip color of a settings page entry.</summary>
        internal static Color SettingsChipColor()
            => EditorPalette.Pick(new Color(0.55f, 0.36f, 0.70f), new Color(0.49f, 0.31f, 0.66f));

        /// <summary>Fill of a tag pill.</summary>
        internal static Color TagPillColor() => EditorPalette.Tint(0.09f, 0.07f);

        /// <summary>Primary text color.</summary>
        private static Color TextColor() => EditorPalette.Text;

        private static Color Shade(Color color, bool hover, bool pressed)
            => EditorStyleUtility.Shade(color, hover, pressed);

        private static GUIStyle Pin(GUIStyle style, Color color)
            => EditorStyleUtility.PinTextColor(style, color);
    }
}