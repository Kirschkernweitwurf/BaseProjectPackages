using System.Collections.Generic;
using Base.DataHubPackage.Editor.Bookmarks;
using Base.EditorUIPackage.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor
{
    /// <summary>
    /// The left pane of the Data Hub: the brand mark, the Overview, the designer's pinned assets, the tables and
    /// the assets edited last, in one scrolling column like the sidebar of a modern app.
    /// </summary>
    internal sealed class DataHubSidebar
    {
        private const string BrandTooltip = "One place for every data asset of the project. Pick a table below.";
        private const string OverviewLabel = "Overview";
        private const string OverviewTooltip = "Every table at a glance, and everything that needs fixing.";
        private const string PinnedLabel = "PINNED";
        private const string PinTooltip = "A pinned asset. Click to open it, unpin it with the star in the side panel.";
        private const string RecentLabel = "RECENT EDITS";
        private const string RecentTooltip = "Changed a moment ago. Click to go back to it.";
        private const float RowGap = 1f;
        private const float SectionLabelHeight = 24f;
        private const string TablesLabel = "TABLES";
        private const float TopPadding = 6f;

        private Vector2 _scroll;

        /// <summary>The asset clicked in the pinned or recent list this pass, or null.</summary>
        internal Object Revealed { get; private set; }

        /// <summary>True when the Overview line was clicked this pass.</summary>
        internal bool IsOverviewClicked { get; private set; }

        /// <summary>Draws the sidebar and records what was clicked.</summary>
        /// <param name="area">The area to fill.</param>
        /// <param name="tree">The table tree.</param>
        /// <param name="isOverviewShown">True while the Overview is the open page.</param>
        /// <param name="pins">The pinned assets.</param>
        /// <param name="recent">The assets edited last.</param>
        /// <param name="styles">The built window styles.</param>
        /// <param name="headerRight">Where the header's buttons start, so the brand stops before them.</param>
        /// <returns>True when the tree selection changed.</returns>
        internal bool Draw(Rect area, DataHubTree tree, bool isOverviewShown, DataPins pins, DataRecentEdits recent,
            DataHubStyles styles, float headerRight)
        {
            Revealed = null;
            IsOverviewClicked = false;

            DataHubGui.Fill(area, DataHubStyles.SidebarColor);

            Rect header = new(area.x, area.y, headerRight - area.x, DataHubStyles.PaneHeaderHeight);
            DataHubGui.Brand(header, DataHubWindow.WindowTitle, PlayerSettings.productName, styles, BrandTooltip);

            IReadOnlyList<Object> pinned = pins.Assets;
            IReadOnlyList<Object> edited = recent.Assets;
            float rowStep = EditorTableStyles.RowHeight + RowGap;

            float height = TopPadding
                + rowStep
                + SectionHeight(pinned.Count, rowStep)
                + SectionLabelHeight
                + tree.Height
                + SectionHeight(edited.Count, rowStep);

            Rect body = new(area.x, area.y + DataHubStyles.PaneHeaderHeight, area.width,
                Mathf.Max(0f, area.height - DataHubStyles.PaneHeaderHeight));

            float width = height > body.height
                ? body.width - GUI.skin.verticalScrollbar.fixedWidth
                : body.width;

            _scroll = GUI.BeginScrollView(body, _scroll, new Rect(0f, 0f, width, height), false, false);

            float y = TopPadding;

            if (DrawLink(new Rect(0f, y, width, EditorTableStyles.RowHeight), DataHubIcons.Overview, OverviewLabel,
                    OverviewTooltip, isOverviewShown, styles))
                IsOverviewClicked = true;

            y += rowStep;
            y = DrawAssets(y, width, PinnedLabel, pinned, PinTooltip, styles);

            SectionLabel(new Rect(0f, y, width, SectionLabelHeight), TablesLabel, styles);
            y += SectionLabelHeight;

            bool isChanged = tree.Draw(new Rect(0f, y, width, tree.Height), styles);
            y += tree.Height;

            DrawAssets(y, width, RecentLabel, edited, RecentTooltip, styles);

            GUI.EndScrollView();

            return isChanged;
        }

        private static float SectionHeight(int count, float rowStep) => count == 0
            ? 0f
            : SectionLabelHeight + count * rowStep;

        private static void SectionLabel(Rect line, string text, DataHubStyles styles)
            => GUI.Label(new Rect(line.x + DataHubStyles.RowHighlightInset + EditorTableStyles.RowInset * 0.5f,
                line.y + TopPadding, line.width, line.height - TopPadding), text, styles.SectionLabel);

        private static bool DrawLink(Rect row, Texture icon, string label, string tooltip, bool isSelected,
            DataHubStyles styles)
        {
            Rect highlight = new(row.x + DataHubStyles.RowHighlightInset, row.y,
                row.width - DataHubStyles.RowHighlightInset * 2f, row.height);

            if (isSelected)
                DataHubGui.Highlight(highlight, DataHubStyles.TreeSelectionColor, styles);
            else if (row.Contains(Event.current.mousePosition))
                DataHubGui.Highlight(highlight, EditorPalette.Hover, styles);

            Rect iconRect = new(highlight.x + EditorTableStyles.RowInset * 0.5f,
                row.y + (row.height - EditorTableStyles.IconSize) * 0.5f, EditorTableStyles.IconSize,
                EditorTableStyles.IconSize);

            DataHubGui.Icon(iconRect, icon);

            Rect text = new(iconRect.xMax + EditorTableStyles.IconGap, row.y, 0f, row.height);
            text.width = Mathf.Max(0f, highlight.xMax - text.x);

            GUIStyle style = isSelected
                ? styles.NameBold
                : styles.Name;

            GUI.Label(text, new GUIContent(label, tooltip), style);

            Event current = Event.current;

            if (current.type != EventType.MouseDown
                || current.button != 0
                || !row.Contains(current.mousePosition))
                return false;

            current.Use();

            return true;
        }

        private float DrawAssets(float y, float width, string label, IReadOnlyList<Object> assets, string tooltip,
            DataHubStyles styles)
        {
            if (assets.Count == 0)
                return y;

            SectionLabel(new Rect(0f, y, width, SectionLabelHeight), label, styles);
            y += SectionLabelHeight;

            foreach (Object asset in assets)
            {
                // Deleted since the list was read, which the next project change takes care of.
                if (asset == null)
                    continue;

                Rect row = new(0f, y, width, EditorTableStyles.RowHeight);

                if (DrawLink(row, DataHubIcons.Of(asset), asset.name, tooltip, false, styles))
                    Revealed = asset;

                y += EditorTableStyles.RowHeight + RowGap;
            }

            return y;
        }
    }
}