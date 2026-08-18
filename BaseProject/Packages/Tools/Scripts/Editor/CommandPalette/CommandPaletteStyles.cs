using Base.EditorUiPackage;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.CommandPalette
{
    /// <summary>
    /// Every size, color and style the palette draws with. Styles are built on first use because
    /// <see cref="EditorStyles"/> is only valid inside a GUI call, and they are dropped again when
    /// the editor skin changes so the palette never keeps a dark color on a light background.
    /// <para>
    /// The shared editor look lives in <see cref="EditorPalette"/>; what stays here are the sizes
    /// and colors that only mean something in a palette, such as the chip of a result kind.
    /// </para>
    /// </summary>
    internal static class CommandPaletteStyles
    {
        /// <summary>Thickness of the outline around the search box.</summary>
        public const float BorderWidth = 1f;

        /// <summary>Height of the small colored chip in front of a result.</summary>
        public const float ChipHeight = 16f;

        /// <summary>Width of the small colored chip in front of a result.</summary>
        public const float ChipWidth = 40f;

        /// <summary>Corner radius of the boxes.</summary>
        public const float CornerRadius = 6f;

        /// <summary>Closing tag of a dimmed run.</summary>
        public const string DimClose = "</color>";

        /// <summary>Height of the hint bar at the bottom of the window.</summary>
        public const float FooterHeight = 20f;

        /// <summary>Space between two neighboring blocks.</summary>
        public const float Gap = 8f;

        /// <summary>Closing tag of a matched run.</summary>
        public const string MatchClose = "</color></b>";

        private const int PathFontSize = 12;

        /// <summary>Height of a pill shaped button.</summary>
        public const float PillHeight = 22f;

        /// <summary>Horizontal padding inside a pill.</summary>
        public const float PillPadding = 8f;

        /// <summary>Corner radius of the pills.</summary>
        public const float PillRadius = 8f;

        /// <summary>Height of a single result row.</summary>
        public const float RowHeight = 42f;

        /// <summary>Horizontal padding inside a result row.</summary>
        public const float RowInset = 12f;

        /// <summary>Width Unity reserves for a vertical scrollbar.</summary>
        public const float ScrollbarWidth = 15f;

        private const int SearchFontSize = 15;

        /// <summary>Height of the search box.</summary>
        public const float SearchHeight = 34f;

        /// <summary>Edge length of the magnifier drawn inside the search box.</summary>
        public const float SearchIconSize = 14f;

        /// <summary>Space above and below every hairline.</summary>
        public const float SeparatorGap = 5f;

        /// <summary>Thickness of a hairline.</summary>
        public const float SeparatorThickness = EditorMetrics.SeparatorThickness;

        /// <summary>Padding between the window edge and its content.</summary>
        public const float WindowPadding = 10f;

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

        private static bool _built;
        private static bool _builtForProSkin;

        /// <summary>Opening tag of a dimmed run.</summary>
        public static string DimOpen => EditorGUIUtility.isProSkin
            ? "<color=#7E7E86>"
            : "<color=#87878F>";

        /// <summary>Opening tag of a matched run.</summary>
        public static string MatchOpen => EditorGUIUtility.isProSkin
            ? "<b><color=#7FC0FF>"
            : "<b><color=#0E4FA8>";

        /// <summary>Text the query is typed into.</summary>
        public static GUIStyle SearchField => _searchField ??= Pin(new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = SearchFontSize
        }, TextColor());

        /// <summary>Dimmed hint drawn inside an empty search box.</summary>
        public static GUIStyle Placeholder => _placeholder ??= Pin(new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = SearchFontSize
        }, DimColor());

        /// <summary>Label in front of the field while tags are edited.</summary>
        public static GUIStyle PrefixLabel => _prefixLabel ??= Pin(new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = SearchFontSize,
            fontStyle = FontStyle.Bold
        }, PinColor());

        /// <summary>Rich text label of the menu path.</summary>
        public static GUIStyle PathLabel => _pathLabel ??= Pin(new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = PathFontSize,
            richText = true
        }, TextColor());

        /// <summary>Dimmed label of the declaring type.</summary>
        public static GUIStyle DetailLabel => _detailLabel ??= Pin(new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleLeft
        }, DimColor());

        /// <summary>Centered white label inside a colored chip.</summary>
        public static GUIStyle ChipLabel => _chipLabel ??= Pin(new GUIStyle(EditorStyles.miniBoldLabel)
        {
            alignment = TextAnchor.MiddleCenter
        }, Color.white);

        /// <summary>Centered label inside a tag pill.</summary>
        public static GUIStyle TagLabel => _tagLabel ??= Pin(new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter
        }, DimColor());

        /// <summary>Centered label inside a pill button.</summary>
        public static GUIStyle PillLabel => _pillLabel ??= Pin(new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter
        }, TextColor());

        /// <summary>Centered label inside a keyboard cap.</summary>
        public static GUIStyle KeyCapLabel => _keyCapLabel ??= Pin(new GUIStyle(EditorStyles.miniBoldLabel)
        {
            alignment = TextAnchor.MiddleCenter
        }, TextColor());

        /// <summary>Centered notice shown when nothing matches.</summary>
        public static GUIStyle EmptyLabel => _emptyLabel ??= Pin(new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = SearchFontSize
        }, DimColor());

        /// <summary>Dimmed label of the footer hints.</summary>
        public static GUIStyle HintLabel => _hintLabel ??= Pin(new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleLeft
        }, DimColor());

        /// <summary>Right aligned label of the result count.</summary>
        public static GUIStyle CountLabel => _countLabel ??= Pin(new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleRight
        }, DimColor());

        /// <summary>Right aligned label of the origin badge.</summary>
        public static GUIStyle BadgeLabel => _badgeLabel ??= Pin(new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleRight
        }, DimColor());

        /// <summary>Centered label of the pin marker.</summary>
        public static GUIStyle PinLabel => _pinLabel ??= Pin(new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter
        }, PinColor());

        /// <summary>Drops every cached style after a skin change. Call once per GUI pass.</summary>
        public static void EnsureFresh()
        {
            if (_built && _builtForProSkin == EditorGUIUtility.isProSkin)
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

            _built = true;
            _builtForProSkin = EditorGUIUtility.isProSkin;
        }

        /// <summary>Blue accent used for the selection and the active pill.</summary>
        public static Color AccentColor() => EditorPalette.Accent;

        /// <summary>Fill behind the whole window.</summary>
        public static Color BackgroundColor() => EditorPalette.Background;

        /// <summary>Border of the search box.</summary>
        public static Color BorderColor() => EditorPalette.Border;

        /// <summary>Dimmed text used for secondary information.</summary>
        public static Color DimColor() => EditorPalette.DimText;

        /// <summary>Fill of the search box.</summary>
        public static Color FieldColor() => EditorPalette.Field;

        /// <summary>Fill of a keyboard cap in the footer.</summary>
        public static Color KeyCapColor() => EditorPalette.KeyCap;

        /// <summary>Chip color of an asset creation entry.</summary>
        public static Color NewChipColor()
            => EditorPalette.Pick(new Color(0.27f, 0.58f, 0.41f), new Color(0.20f, 0.52f, 0.36f));

        /// <summary>Fill of a pill button in its current state.</summary>
        /// <param name="active">Whether the button is switched on.</param>
        /// <param name="hover">Whether the mouse sits on it.</param>
        /// <param name="pressed">Whether it is being held down.</param>
        /// <returns>The fill color to draw.</returns>
        public static Color PillColor(bool active, bool hover, bool pressed)
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
        public static Color PinColor() => EditorPalette.Focus;

        /// <summary>Fill of the row the mouse hovers.</summary>
        public static Color RowHoverColor() => EditorPalette.Hover;

        /// <summary>Fill of the row the keyboard selection sits on.</summary>
        public static Color RowSelectedColor() => EditorPalette.SelectionFill;

        /// <summary>Chip color of a menu item entry.</summary>
        public static Color RunChipColor()
            => EditorPalette.Pick(new Color(0.28f, 0.50f, 0.78f), new Color(0.24f, 0.46f, 0.76f));

        /// <summary>Hairline between the blocks of the window.</summary>
        public static Color SeparatorColor() => EditorPalette.Separator;

        /// <summary>Fill of a tag pill.</summary>
        public static Color TagPillColor() => EditorPalette.Tint(0.09f, 0.07f);

        /// <summary>Primary text color.</summary>
        public static Color TextColor() => EditorPalette.Text;

        private static Color Shade(Color color, bool hover, bool pressed)
            => EditorStyleUtility.Shade(color, hover, pressed);

        private static GUIStyle Pin(GUIStyle style, Color color)
            => EditorStyleUtility.PinTextColor(style, color);
    }
}