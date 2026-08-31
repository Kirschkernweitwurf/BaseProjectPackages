using Base.EditorUiPackage;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeExplorer
{
    /// <summary>
    /// Styles, metrics and colors for the attribute window.
    /// </summary>
    /// <remarks>
    /// Built once rather than per repaint, and rebuilt when the editor theme changes, since every color
    /// here is picked for one editor theme and the light and dark versions are not interchangeable. The shared
    /// editor look comes from <see cref="EditorPalette"/>; what stays here are the tints only a two
    /// pane browser with cards and tabs needs.
    /// </remarks>
    internal sealed class AttributeExplorerStyles
    {
        /// <summary>The eyebrow above a category name on its own page.</summary>
        internal const string CategoryEyebrow = "Category";
        /// <summary>Height of a category header in the list.</summary>
        internal const float CategoryHeight = 22f;

        /// <summary>Gap between the two panes.</summary>
        internal const float ColumnGap = 8f;

        /// <summary>Shown when nothing is selected.</summary>
        internal const string EmptyMessage = "Pick an attribute on the left.";

        /// <summary>Height of one attribute row in the list.</summary>
        internal const float EntryHeight = 20f;

        /// <summary>Width of the list beside the content.</summary>
        internal const float ListWidth = 250f;

        /// <summary>Padding inside the content pane.</summary>
        internal const float Padding = 14f;

        /// <summary>Height of a category name on its own page.</summary>
        internal const float PageHeadingHeight = 30f;

        /// <summary>Space the vertical scrollbar of the content pane takes.</summary>
        internal const float ScrollBarWidth = 15f;

        /// <summary>Gap between two blocks in the content pane.</summary>
        internal const float SectionGap = 14f;

        /// <summary>Gap between the accent bar and whatever the row draws first.</summary>
        internal const float SelectionBarGap = 5f;

        /// <summary>Width of the accent bar marking the selected row.</summary>
        internal const float SelectionBarWidth = 2f;

        /// <summary>Gap between two rows of the same block.</summary>
        internal const float TightGap = 4f;
        private const float CardBorderStrength = 0.14f;

        private const int CardCornerPadding = 12;
        private const float CardFillAlternateStrength = 0.15f;
        private const float CardFillStrength = 0.04f;
        private const float CardFocusedStrength = 0.20f;
        private const float CardHoverStrength = 0.22f;
        private const float CategoryBandStrength = 0.04f;
        private const int CategoryIndent = 18;
        private const int DescriptionFontSize = 11;
        private const int EntryIndent = 22;
        private const int HeadingFontSize = 18;
        private const float LightTabStripFactor = 0.5f;
        private const int PageHeadingFontSize = 22;
        private const int SectionFontSize = 12;
        private const float TabActiveStrength = 0.05f;
        private const float TabStripStrength = 0.10f;

        /// <summary>A block in the content pane.</summary>
        internal GUIStyle Card { get; private set; }

        /// <summary>The name of an attribute in the category overview.</summary>
        internal GUIStyle CardTitle { get; private set; }

        /// <summary>One bullet of the variations list.</summary>
        internal GUIStyle Bullet { get; private set; }

        /// <summary>Category header in the list.</summary>
        internal GUIStyle Category { get; private set; }

        /// <summary>Padding around everything in the content pane.</summary>
        internal GUIStyle ContentPane { get; private set; }

        /// <summary>The line under the name explaining what the attribute does.</summary>
        internal GUIStyle Description { get; private set; }

        /// <summary>An unselected list row.</summary>
        internal GUIStyle Entry { get; private set; }

        /// <summary>The category the selected attribute belongs to, above its name.</summary>
        internal GUIStyle Eyebrow { get; private set; }

        /// <summary>The attribute count under the list.</summary>
        internal GUIStyle Footer { get; private set; }

        /// <summary>Large name of the selected attribute.</summary>
        internal GUIStyle Heading { get; private set; }

        /// <summary>Background of the list column.</summary>
        internal GUIStyle ListBackground { get; private set; }

        /// <summary>Running text in the content pane, for requirements and variations.</summary>
        internal GUIStyle Body { get; private set; }

        /// <summary>Small heading above the preview and the source.</summary>
        internal GUIStyle Section { get; private set; }

        /// <summary>The name of a category on its own page.</summary>
        internal GUIStyle PageHeading { get; private set; }

        /// <summary>The selected list row.</summary>
        internal GUIStyle SelectedEntry { get; private set; }

        /// <summary>An unselected tab.</summary>
        internal GUIStyle Tab { get; private set; }

        /// <summary>The tab the window is on.</summary>
        internal GUIStyle TabSelected { get; private set; }

        /// <summary>Monospaced style for the source block.</summary>
        internal GUIStyle Source { get; private set; }

        /// <summary>Outline of a card in the category overview.</summary>
        internal Color CardBorder { get; private set; }

        /// <summary>Background of a card in the category overview.</summary>
        internal Color CardFill { get; private set; }

        /// <summary>Background of every other card, so a long category reads as rows.</summary>
        internal Color CardFillAlternate { get; private set; }

        /// <summary>Background of a card the pointer is over.</summary>
        internal Color CardHover { get; private set; }

        /// <summary>Background of the card the keyboard is on.</summary>
        internal Color CardFocused { get; private set; }

        /// <summary>Band behind a category header in the list.</summary>
        internal Color CategoryBand { get; private set; }

        /// <summary>Line between the list and the content.</summary>
        internal Color Divider { get; private set; }

        /// <summary>Tint of a row the pointer is over.</summary>
        internal Color Hover { get; private set; }

        /// <summary>Accent used for the selected row and its bar.</summary>
        internal Color Selection { get; private set; }

        /// <summary>Tint behind the selected row.</summary>
        internal Color SelectionFill { get; private set; }

        /// <summary>Tint of every other row, so a long category reads as rows.</summary>
        internal Color Stripe { get; private set; }

        /// <summary>Background of the tab the window is on.</summary>
        internal Color TabActive { get; private set; }

        /// <summary>Background of the strip the tabs sit in.</summary>
        internal Color TabStrip { get; private set; }

        private readonly EditorStyleWatch _watch = new();

        /// <summary>Builds the styles, and rebuilds them after the editor theme or the theme changed.</summary>
        internal void EnsureBuilt()
        {
            if (!_watch.IsStale)
                return;

            Build();

            _watch.MarkFresh();
        }

        /// <summary>Kept for symmetry with the window lifetime. There is nothing to free.</summary>
        internal void Dispose() => _watch.Invalidate();

        private static Color Tint(float strength) => EditorPalette.Tint(strength);

        private static Color Muted() => EditorStyleUtility.MutedTextColor();

        private static RectOffset Uniform(int value) => EditorStyleUtility.UniformPadding(value);

        private void Build()
        {
            Heading = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = HeadingFontSize,
                margin = Uniform(0)
            };

            Eyebrow = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                normal =
                {
                    textColor = Muted()
                }
            };

            Description = new GUIStyle(EditorStyles.label)
            {
                fontSize = DescriptionFontSize,
                wordWrap = true,
                normal =
                {
                    textColor = Muted()
                }
            };

            Section = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = SectionFontSize,
                padding = new RectOffset(0, 0, 0, 3)
            };

            Body = new GUIStyle(EditorStyles.label)
            {
                fontSize = DescriptionFontSize,
                wordWrap = true,
                richText = true
            };

            Category = new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(CategoryIndent, 4, 2, 2)
            };

            Entry = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(EntryIndent, 6, 0, 0)
            };

            SelectedEntry = new GUIStyle(Entry)
            {
                fontStyle = FontStyle.Bold
            };

            PageHeading = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = PageHeadingFontSize,
                margin = Uniform(0)
            };

            Tab = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                normal =
                {
                    textColor = Muted()
                },
                hover =
                {
                    textColor = EditorStyles.label.normal.textColor
                }
            };

            TabSelected = new GUIStyle(Tab)
            {
                fontStyle = FontStyle.Bold,
                normal =
                {
                    textColor = EditorStyles.label.normal.textColor
                }
            };

            ListBackground = new GUIStyle(EditorStyles.helpBox)
            {
                margin = Uniform(0),
                padding = new RectOffset(0, 0, 4, 4)
            };

            ContentPane = new GUIStyle
            {
                padding = Uniform((int)Padding)
            };

            CardTitle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontStyle = FontStyle.Bold,
                margin = Uniform(0),
                padding = Uniform(0)
            };

            Bullet = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = DescriptionFontSize,
                margin = Uniform(0),
                padding = Uniform(0)
            };

            Card = new GUIStyle(EditorStyles.helpBox)
            {
                margin = Uniform(0),
                padding = Uniform(CardCornerPadding)
            };

            Footer = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(6, 4, 0, 0),
                normal =
                {
                    textColor = Muted()
                }
            };

            CardBorder = Tint(CardBorderStrength);
            CardFill = Tint(CardFillStrength);
            CardFillAlternate = Tint(CardFillAlternateStrength);
            CardFocused = Tint(CardFocusedStrength);
            CardHover = Tint(CardHoverStrength);
            CategoryBand = Tint(CategoryBandStrength);
            Hover = EditorPalette.Hover;
            TabActive = Tint(TabActiveStrength);
            TabStrip = new Color(0f, 0f, 0f, EditorThemeProvider.IsDarkMode
                ? TabStripStrength
                : TabStripStrength * LightTabStripFactor);

            Stripe = EditorPalette.Stripe;
            SelectionFill = EditorPalette.SelectionFill;

            Selection = EditorPalette.Selection;
            Divider = EditorPalette.Divider;

            Source = new GUIStyle(EditorStyles.textArea)
            {
                font = EditorFonts.Monospaced(),
                wordWrap = false,
                richText = false,
                padding = Uniform(CardCornerPadding)
            };
        }
    }
}