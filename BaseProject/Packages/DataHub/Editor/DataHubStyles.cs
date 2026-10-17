using System;
using Base.EditorUIPackage.Editor;
using UnityEditor;
using UnityEngine;

namespace Base.DataHubPackage.Editor
{
    /// <summary>
    /// What the Data Hub needs on top of the shared Base window look: its pane sizes, the flat table cells,
    /// the sidebar, the segmented tabs and the small icon buttons. Colors, rows, badges and the card all
    /// come from <see cref="EditorWindowStyles"/>.
    /// </summary>
    internal sealed class DataHubStyles : EditorWindowStyles
    {
        /// <summary>Height of a button in a pane header.</summary>
        internal const float ButtonHeight = 22f;

        /// <summary>Space between a cell's edge and the control or text in it.</summary>
        internal const float CellPadding = 4f;

        /// <summary>Width the inspector panel starts at before the user drags it.</summary>
        internal const float DefaultInspectorWidth = 360f;

        /// <summary>Width the type tree starts at before the user drags it.</summary>
        internal const float DefaultTreeWidth = 210f;

        /// <summary>Height of the bar with the search field and the filter under a pane header.</summary>
        internal const float FilterBarHeight = 34f;

        /// <summary>Height of the header at the top of every pane, so their lines meet across the window.</summary>
        internal const float PaneHeaderHeight = 52f;

        /// <summary>Width of a square icon button.</summary>
        internal const float IconButtonWidth = 26f;

        /// <summary>Smallest width of any of the three panes.</summary>
        internal const float MinPaneWidth = 160f;

        /// <summary>Smallest height of the window.</summary>
        internal const float MinWindowHeight = 360f;

        /// <summary>Smallest width of the window, enough for three panes.</summary>
        internal const float MinWindowWidth = 820f;

        /// <summary>Space between the edge of a pane and its content.</summary>
        internal const float PanelPadding = 10f;

        /// <summary>Width of the red edge on a row with problems.</summary>
        internal const float ProblemEdgeWidth = 3f;

        /// <summary>Gap between a rounded row highlight and the pane edge.</summary>
        internal const float RowHighlightInset = 6f;

        /// <summary>Height of the line under the table with counts and the check result.</summary>
        internal const float StatusBarHeight = 24f;

        /// <summary>Width of a count stepper: minus, the number and plus.</summary>
        internal const float StepperWidth = 92f;

        private const string CellStyleName = "DataHubCell.";
        private const float DarkSidebarAlpha = 0.16f;
        private const float DropHighlightAlpha = 0.16f;
        private const float GridAlpha = 0.45f;
        private const int HeaderButtonPadding = 8;
        private const int IconButtonPadding = 4;
        private const float LightSidebarAlpha = 0.04f;
        private const int SegmentCornerRadius = 5;
        private const float TreeSelectionAlpha = 0.28f;

        /// <summary>Fill of the sidebar and the side panel, a shade off the table so the panes read apart.</summary>
        internal static Color SidebarColor => EditorPalette.Pick(new Color(0f, 0f, 0f, DarkSidebarAlpha),
            new Color(0f, 0f, 0f, LightSidebarAlpha));

        /// <summary>The faint vertical line between two table columns.</summary>
        internal static Color GridColor => EditorPalette.WithAlpha(EditorPalette.Separator,
            EditorPalette.Separator.a * GridAlpha);

        /// <summary>Fill of the selected line in the type tree.</summary>
        internal static Color TreeSelectionColor => EditorPalette.WithAlpha(EditorPalette.Accent, TreeSelectionAlpha);

        /// <summary>The asset count next to a table in the tree: small, dimmed and right-aligned.</summary>
        internal GUIStyle Count { get; private set; }

        /// <summary>The asset count next to the selected table in the tree, as bright as its label.</summary>
        internal GUIStyle CountSelected { get; private set; }

        /// <summary>A text cell: plain text at rest, a field under the mouse and while typing.</summary>
        internal GUIStyle CellText { get; private set; }

        /// <summary>A number cell, right-aligned so digits line up.</summary>
        internal GUIStyle CellNumber { get; private set; }

        /// <summary>A read-only number cell, dim like other details and right aligned like editable numbers.</summary>
        internal GUIStyle DetailNumber { get; private set; }

        /// <summary>An enum cell: plain text at rest, a field under the mouse. The arrow is drawn by hand.</summary>
        internal GUIStyle CellPopup { get; private set; }

        /// <summary>Rounded accent fill over a list while assets are dragged onto it.</summary>
        internal GUIStyle DropHighlight { get; private set; }

        /// <summary>Title of a column group, centered over its columns.</summary>
        internal GUIStyle GroupHeader { get; private set; }

        /// <summary>A button in a pane header with an icon and a label.</summary>
        internal GUIStyle HeaderButton { get; private set; }

        /// <summary>Column title for a centered column.</summary>
        internal GUIStyle HeaderCenter { get; private set; }

        /// <summary>The filled accent button in a pane header, for the main action.</summary>
        internal GUIStyle HeaderPrimaryButton { get; private set; }

        /// <summary>Column title for a right-aligned column.</summary>
        internal GUIStyle HeaderRight { get; private set; }

        /// <summary>A square button that only shows an icon, with a fill under the mouse.</summary>
        internal GUIStyle IconButton { get; private set; }

        /// <summary>A name that can be clicked to find its asset, lit in the accent under the mouse.</summary>
        internal GUIStyle LinkName { get; private set; }

        /// <summary>A white rounded fill, drawn tinted for a rounded highlight with no style of its own.</summary>
        internal GUIStyle RoundedFill { get; private set; }

        /// <summary>A small section label, like "In use" over a block of lists.</summary>
        internal GUIStyle SectionLabel { get; private set; }

        /// <summary>The label of a segment that is not active.</summary>
        internal GUIStyle Segment { get; private set; }

        /// <summary>The label of the active segment.</summary>
        internal GUIStyle SegmentOn { get; private set; }

        /// <summary>The raised fill of the active segment.</summary>
        internal GUIStyle SegmentThumb { get; private set; }

        /// <summary>The track behind the segments of a segmented control.</summary>
        internal GUIStyle SegmentTrack { get; private set; }

        /// <summary>The number in the middle of a count stepper.</summary>
        internal GUIStyle StepperNumber { get; private set; }

        /// <summary>The parent part of a breadcrumb title, dimmed so the page name stands out.</summary>
        internal GUIStyle TitleDim { get; private set; }

        /// <inheritdoc/>
        protected override void Build()
        {
            base.Build();

            Count = EditorStyleUtility.PinTextColor(new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleRight
            }, EditorPalette.DimText);

            CountSelected = EditorStyleUtility.PinTextColor(new GUIStyle(Count), EditorPalette.Text);

            CellText = BuildCellField(EditorStyles.textField, TextAnchor.MiddleLeft);
            CellNumber = BuildCellField(EditorStyles.textField, TextAnchor.MiddleRight);
            DetailNumber = new GUIStyle(Detail)
            {
                alignment = TextAnchor.MiddleRight
            };
            CellPopup = BuildCellField(EditorStyles.popup, TextAnchor.MiddleLeft);
            StepperNumber = BuildCellField(EditorStyles.textField, TextAnchor.MiddleCenter);

            DropHighlight = BuildFill(EditorPalette.WithAlpha(EditorPalette.Accent, DropHighlightAlpha),
                EditorMetrics.CardCornerRadius);

            GroupHeader = AlignedHeader(TextAnchor.MiddleCenter);
            HeaderCenter = AlignedHeader(TextAnchor.MiddleCenter);
            HeaderRight = AlignedHeader(TextAnchor.MiddleRight);

            HeaderButton = BuildHeaderButton(SecondaryButton);
            HeaderPrimaryButton = BuildHeaderButton(PrimaryButton);

            IconButton = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                imagePosition = ImagePosition.ImageOnly,
                padding = EditorStyleUtility.UniformPadding(IconButtonPadding),
                border = EditorStyleUtility.UniformPadding(EditorMetrics.CardCornerRadius),
                hover = { background = Textures.Rounded(EditorPalette.Hover, EditorMetrics.CardCornerRadius) },
                active =
                {
                    background = Textures.Rounded(EditorPalette.SelectionFill, EditorMetrics.CardCornerRadius)
                }
            };

            LinkName = EditorStyleUtility.PinTextColor(new GUIStyle(Name), EditorPalette.Text);
            LinkName.hover.textColor = EditorPalette.Accent;

            RoundedFill = BuildFill(Color.white, EditorMetrics.CardCornerRadius);

            SectionLabel = EditorStyleUtility.PinTextColor(new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleLeft
            }, EditorPalette.DimText);

            Segment = EditorStyleUtility.PinTextColor(new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Ellipsis
            }, EditorPalette.DimText);

            SegmentOn = EditorStyleUtility.PinTextColor(new GUIStyle(Segment)
            {
                fontStyle = FontStyle.Bold
            }, EditorPalette.SecondaryText);

            SegmentThumb = BuildFill(EditorPalette.Secondary, SegmentCornerRadius);
            SegmentTrack = BuildFill(EditorPalette.Field, SegmentCornerRadius);

            TitleDim = EditorStyleUtility.PinTextColor(new GUIStyle(Title)
            {
                fontStyle = FontStyle.Normal
            }, EditorPalette.DimText);
        }

        private static GUIStyle BuildHeaderButton(GUIStyle source) => new(source)
        {
            imagePosition = ImagePosition.ImageLeft,
            padding = new RectOffset(HeaderButtonPadding, HeaderButtonPadding, 0, 0),
            fixedHeight = ButtonHeight
        };

        private GUIStyle AlignedHeader(TextAnchor alignment) => EditorStyleUtility.PinTextColor(new GUIStyle(Header)
        {
            alignment = alignment
        }, EditorPalette.DimText);

        private GUIStyle BuildFill(Color color, int radius) => new()
        {
            border = EditorStyleUtility.UniformPadding(radius),
            normal = { background = Textures.Rounded(color, radius) }
        };

        // The editor's field look on hover and focus only. A table full of boxed fields reads as a form
        // rather than as data, and the values are what the eye should find.
        private GUIStyle BuildCellField(GUIStyle source, TextAnchor alignment)
        {
            int radius = Mathf.Max(1, source.border.left);

            // Renamed, because Unity 6 draws the editor's own field styles from its style sheet by name and
            // would put the box back whatever the background says.
            GUIStyle style = new(source)
            {
                name = CellStyleName + source.name,
                alignment = alignment,
                clipping = TextClipping.Clip,
                margin = new RectOffset()
            };

            style.normal.background = null;
            style.normal.scaledBackgrounds = Array.Empty<Texture2D>();
            style.hover.background = Textures.Rounded(EditorPalette.Field, radius);
            style.hover.scaledBackgrounds = Array.Empty<Texture2D>();

            return EditorStyleUtility.PinTextColor(style, EditorPalette.Text);
        }
    }
}