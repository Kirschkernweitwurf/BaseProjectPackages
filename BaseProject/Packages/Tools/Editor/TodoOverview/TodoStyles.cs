using System;
using Base.EditorUIPackage.Editor;
using Base.ToolsPackage.Editor.TodoOverview.Model;
using UnityEditor;
using UnityEngine;

namespace Base.ToolsPackage.Editor.TodoOverview
{
    /// <summary>
    /// Every size, color and style the todo window draws with. Styles are built on first use because
    /// <see cref="EditorStyles"/> is only valid inside a GUI call, and they are rebuilt when the
    /// editor theme changes so a dark color never ends up on a light background.
    /// <para>
    /// All of them are built in one go rather than one by one on first access, because Unity swaps
    /// the editor styles out while a dropdown owns the GUI. Reading one in such a pass throws, so
    /// nothing here may touch <see cref="EditorStyles"/> outside <see cref="EnsureBuilt"/>.
    /// </para>
    /// <para>
    /// The shared editor look lives in <see cref="EditorPalette"/>; what stays here are the sizes of a
    /// row and the colors that only mean something on a task list, such as a date that has passed.
    /// </para>
    /// </summary>
    internal static class TodoStyles
    {
        /// <summary>Width of the colored band in front of a row.</summary>
        internal const float BandWidth = 3f;

        /// <summary>Height of a button in the toolbar and in the detail panel.</summary>
        internal const float ButtonHeight = 20f;

        /// <summary>Corner radius of a button.</summary>
        internal const float ButtonRadius = 4f;

        /// <summary>Height of the small triangle that marks a dropdown and the active sort order.</summary>
        internal const float CaretHeight = 4f;

        /// <summary>Width of the small triangle that marks a dropdown and the active sort order.</summary>
        internal const float CaretWidth = 7f;

        /// <summary>Height of a keyword or date pill.</summary>
        internal const float ChipHeight = 16f;

        /// <summary>Corner radius of a pill.</summary>
        internal const float ChipRadius = 4f;

        /// <summary>Default width of the keyword column.</summary>
        internal const float ChipWidth = 66f;

        /// <summary>Default width of the date column.</summary>
        internal const float DateWidth = 76f;

        /// <summary>Height of the panel that shows the selected item in full.</summary>
        internal const float DetailHeight = 104f;

        /// <summary>Corner radius of the boxes.</summary>
        internal const float DetailRadius = 6f;

        /// <summary>How wide a column divider can be grabbed, which is wider than the line itself.</summary>
        internal const float DividerHitWidth = 8f;

        /// <summary>Space between two neighboring blocks.</summary>
        internal const float Gap = 6f;

        /// <summary>Width of the count badge on a section header.</summary>
        internal const float HeaderBadgeWidth = 34f;

        /// <summary>Height of the row that carries the column titles.</summary>
        internal const float HeaderHeight = 20f;

        /// <summary>Default width of the file and line column.</summary>
        internal const float LocationWidth = 190f;

        /// <summary>Narrowest the date column can be dragged.</summary>
        internal const float MinDateWidth = 52f;

        /// <summary>Narrowest the keyword column can be dragged.</summary>
        internal const float MinKeywordWidth = 44f;

        /// <summary>Narrowest the file and line column can be dragged.</summary>
        internal const float MinLocationWidth = 80f;

        /// <summary>Width the message column keeps, whatever the other columns are dragged to.</summary>
        internal const float MinMessageWidth = 120f;

        /// <summary>Narrowest the owner column can be dragged.</summary>
        internal const float MinOwnerWidth = 44f;

        /// <summary>Default width of the owner column.</summary>
        internal const float OwnerWidth = 88f;

        /// <summary>Height of a single row, header rows included.</summary>
        internal const float RowHeight = 24f;

        /// <summary>Padding between the window edge and the content of a row.</summary>
        internal const float RowInset = 8f;

        /// <summary>Width of the search field in the toolbar.</summary>
        internal const float SearchWidth = 180f;

        /// <summary>Space between two things that belong together, such as a title and its mark.</summary>
        internal const float TightGap = 3f;

        /// <summary>Height of the bar at the top of the window.</summary>
        internal const float ToolbarHeight = 30f;

        private const int ChipTextPadding = 8;

        private const int DropdownTextPadding = 22;

        private const int MessageFontSize = 12;

        /// <summary>Thickness of a hairline.</summary>
        internal static float SeparatorThickness => EditorMetrics.SeparatorThickness;

        /// <summary>Label of a pill or button whose fill is bright, chosen by <see cref="ChipStyle"/>.</summary>
        internal static GUIStyle AccentLabel { get; private set; }

        /// <summary>Centered label of a button that is not the primary action.</summary>
        internal static GUIStyle Button { get; private set; }

        /// <summary>Label of a pill whose fill is dark, chosen by <see cref="ChipStyle"/>.</summary>
        internal static GUIStyle Chip { get; private set; }

        /// <summary>Centered label of a section's item count.</summary>
        internal static GUIStyle Count { get; private set; }

        /// <summary>Right aligned label that reports how many items the list shows.</summary>
        internal static GUIStyle Counter { get; private set; }

        /// <summary>Wrapping label that shows the full text of the selected item.</summary>
        internal static GUIStyle DetailBody { get; private set; }

        /// <summary>Left aligned label of a dropdown, kept clear of the caret on its right.</summary>
        internal static GUIStyle Dropdown { get; private set; }

        /// <summary>Centered notice shown when nothing matches.</summary>
        internal static GUIStyle Empty { get; private set; }

        /// <summary>Foldout a section header is opened and closed with.</summary>
        internal static GUIStyle Foldout { get; private set; }

        /// <summary>Title above a column.</summary>
        internal static GUIStyle Header { get; private set; }

        /// <summary>Right aligned file and line label at the end of a row.</summary>
        internal static GUIStyle Location { get; private set; }

        /// <summary>The text of an item, which is what the eye should land on first.</summary>
        internal static GUIStyle Message { get; private set; }

        /// <summary>Centered label inside a pill whose fill is too pale for white text.</summary>
        internal static GUIStyle MutedChip { get; private set; }

        /// <summary>Name of the responsible person on a row.</summary>
        internal static GUIStyle Owner { get; private set; }

        /// <summary>Dimmed path shown in the detail panel.</summary>
        internal static GUIStyle Path { get; private set; }

        /// <summary>The search field in the toolbar.</summary>
        internal static GUIStyle Search { get; private set; }

        /// <summary>The word shown in the search field while it is empty.</summary>
        internal static GUIStyle SearchHint { get; private set; }

        private static readonly EditorStyleWatch Watch = new();

        /// <summary>
        /// Builds the styles on the first pass and again after either theme changes. Call at the top of
        /// every GUI pass and skip the pass when it reports that the styles are not there yet.
        /// </summary>
        /// <returns><c>true</c> once every style is ready to draw with.</returns>
        internal static bool EnsureBuilt()
        {
            if (!Watch.IsStale)
                return true;

            // A pass that runs while a dropdown owns the GUI has no editor styles to copy from, and
            // a style built from a missing one would stay broken for the rest of the session.
            if (!TryBuild())
                return false;

            Watch.MarkFresh();

            return true;
        }

        /// <summary>Fill of the primary action and of the mark on the active sort order.</summary>
        /// <returns>The accent color.</returns>
        internal static Color AccentColor() => EditorPalette.Accent;

        /// <summary>Fill of a control that can be clicked but is not the primary action.</summary>
        /// <returns>The control fill color.</returns>
        internal static Color ControlColor() => EditorPalette.Secondary;

        /// <summary>Fill of the search box, which is an input rather than a button.</summary>
        /// <returns>The field fill color.</returns>
        internal static Color FieldColor() => EditorPalette.Field;

        /// <summary>Fill of a pill whose keyword is currently filtered out.</summary>
        /// <returns>The muted fill color.</returns>
        internal static Color MutedChipColor() => EditorPalette.KeyCap;

        /// <summary>Fill behind the toolbar, the detail panel and the section headers.</summary>
        /// <returns>The panel fill color.</returns>
        internal static Color PanelColor() => EditorPalette.Card;

        /// <summary>
        /// The label style that stays readable on a given fill.
        /// </summary>
        /// <remarks>
        /// A theme carries exactly one pair of text colors meant to sit on something: the primary one
        /// for a dark surface and the on-accent one for a bright surface, which swap round between the
        /// dark and light editor themes. Whichever of the two is further from the fill in brightness
        /// is the readable one, so a keyword color the user picked gets a legible label without the
        /// theme needing to know it exists.
        /// </remarks>
        /// <param name="fill">The color the label is drawn on top of.</param>
        /// <returns>The style to draw the label with.</returns>
        internal static GUIStyle ChipStyle(Color fill) => EditorPalette.TextOn(fill) == EditorPalette.AccentText
            ? AccentLabel
            : Chip;

        /// <summary>Color a date pill is drawn in.</summary>
        /// <param name="state">How loudly the date is asking to be looked at.</param>
        /// <returns>The fill color of the pill.</returns>
        internal static Color DateColor(ETodoDateState state) => state switch
        {
            ETodoDateState.Alert => EditorPalette.Danger,
            ETodoDateState.Warning => EditorPalette.Focus,
            _ => MutedChipColor()
        };

        /// <summary>Label style of a date pill, which has to stay readable on its fill.</summary>
        /// <param name="state">How loudly the date is asking to be looked at.</param>
        /// <returns>The style the date is drawn with.</returns>
        internal static GUIStyle DateStyle(ETodoDateState state) => state switch
        {
            ETodoDateState.Alert => ChipStyle(DateColor(state)),
            ETodoDateState.Warning => ChipStyle(DateColor(state)),
            _ => MutedChip
        };

        // Reading any editor style throws while the GUI belongs to a dropdown, so the whole set is
        // built inside one try and the pass is given up on rather than half built.
        private static bool TryBuild()
        {
            try
            {
                Build();
            }
            catch (NullReferenceException)
            {
                return false;
            }

            return true;
        }

        private static void Build()
        {
            AccentLabel = Pin(new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleCenter
            }, EditorPalette.AccentText);

            Button = Pin(new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter
            }, EditorPalette.SecondaryText);

            // Not white any more. A keyword color is picked by the user and can land anywhere on the
            // spectrum, so the label is chosen per fill by ChipStyle rather than fixed to one end of
            // it, and this is the end that reads on a dark fill.
            Chip = Pin(new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleCenter
            }, EditorPalette.Text);

            Count = Pin(new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter
            }, EditorPalette.DimText);

            DetailBody = Pin(new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.UpperLeft,
                wordWrap = true
            }, EditorPalette.Text);

            Dropdown = Pin(new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(ChipTextPadding, DropdownTextPadding, 0, 0)
            }, EditorPalette.SecondaryText);

            Empty = Pin(new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = MessageFontSize
            }, EditorPalette.DimText);

            Foldout = Pin(new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold
            }, EditorPalette.Text);

            Header = Pin(new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset()
            }, EditorPalette.DimText);

            Counter = Pin(new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleRight
            }, EditorPalette.DimText);

            Location = Pin(new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleLeft
            }, EditorPalette.DimText);

            Message = Pin(new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = MessageFontSize
            }, EditorPalette.Text);

            MutedChip = Pin(new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleCenter
            }, EditorPalette.DimText);

            Owner = Pin(new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleLeft
            }, EditorPalette.DimText);

            Path = Pin(new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleLeft
            }, EditorPalette.DimText);

            // Built from a plain label rather than from the toolbar search style, so the field
            // brings no background, no fixed height and no state of its own into the toolbar.
            Search = Pin(new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft,
                padding = EditorStyleUtility.HorizontalPadding(ChipTextPadding),
                wordWrap = false
            }, EditorPalette.Text);

            SearchHint = Pin(new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft,
                padding = EditorStyleUtility.HorizontalPadding(ChipTextPadding),
                wordWrap = false
            }, EditorPalette.DimText);
        }

        private static GUIStyle Pin(GUIStyle style, Color color) => EditorStyleUtility.PinTextColor(style, color);
    }
}