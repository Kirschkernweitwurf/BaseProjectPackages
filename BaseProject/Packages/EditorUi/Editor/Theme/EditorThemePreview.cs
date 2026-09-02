using UnityEditor;
using UnityEngine;

namespace Base.EditorUIPackage.Editor
{
    /// <summary>
    /// A miniature Base list window, drawn from the same styles the real ones use, so a change to a
    /// color or a corner radius can be judged next to the field that made it rather than by opening
    /// a window and comparing from memory.
    /// </summary>
    /// <remarks>
    /// Every piece here is drawn with <see cref="GUI.Label"/> rather than a control, because a
    /// preview that reacts to clicks invites the user to try to use it.
    /// </remarks>
    public static class EditorThemePreview
    {
        private const string BadgeText = "Alive";
        private const string ClearLabel = "Clear";
        private const string DetailText = "Assets/Scripts/Gameplay";
        private const string HeaderDetail = "Path";
        private const string HeaderName = "Name";
        private const string HeaderState = "State";

        private const int HoveredRow = 2;
        private const string HoverRowName = "Row under the mouse";
        private const float NameColumnShare = 0.34f;
        private const string PingLabel = "Ping";
        private const string PlainRowName = "A plain row";
        private const string PrimaryLabel = "Refresh";
        private const int RowCount = 4;
        private const float SearchFieldHeight = 18f;
        private const string SearchText = "Search";
        private const int SelectedRow = 3;
        private const string SelectedRowName = "The selected row";
        private const string StripedRowName = "Every second row";
        private const string SummaryPillText = "All good";
        private const string SummaryText = "4 of 4 shown";
        private const float ToolbarHeight = 20f;

        /// <summary>
        /// The height <see cref="Draw"/> needs, so a caller can reserve the rectangle itself.
        /// </summary>
        /// <returns>The preview height in pixels.</returns>
        public static float MeasureHeight() => EditorTableStyles.OuterMargin * 2f
            + ToolbarHeight
            + EditorMetrics.ItemGap
            + EditorTableStyles.SummaryHeight
            + EditorMetrics.TightGap
            + CardHeight();

        /// <summary>
        /// Draws the preview into the given area.
        /// </summary>
        /// <param name="area">The rectangle to fill, usually one reserved at <see cref="MeasureHeight"/>.</param>
        /// <param name="styles">The built list window styles the preview borrows.</param>
        public static void Draw(Rect area, EditorTableStyles styles)
        {
            if (styles == null)
                return;

            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(area, EditorPalette.Background);

            Rect content = Inset(area, EditorTableStyles.OuterMargin);

            Rect toolbar = new(content.x, content.y, content.width, ToolbarHeight);
            DrawToolbar(toolbar, styles);

            Rect summary = new(content.x, toolbar.yMax + EditorMetrics.ItemGap, content.width,
                EditorTableStyles.SummaryHeight);

            DrawSummary(summary, styles);

            Rect card = new(content.x, summary.yMax + EditorMetrics.TightGap, content.width, CardHeight());
            DrawCard(card, styles);
        }

        private static float CardHeight() => EditorTableStyles.CardPadding * 2f
            + EditorMetrics.HeaderHeight
            + EditorMetrics.SeparatorThickness
            + RowCount * EditorTableStyles.RowHeight;

        private static Rect Inset(Rect area, float amount) => new(area.x + amount, area.y + amount,
            area.width - amount * 2f, area.height - amount * 2f);

        private static void DrawToolbar(Rect area, EditorTableStyles styles)
        {
            Rect primary = new(area.x, area.y, EditorTableStyles.ToolbarButtonWidth, area.height);
            GUI.Label(primary, PrimaryLabel, styles.PrimaryButton);

            Rect secondary = new(primary.xMax + EditorMetrics.TightGap, area.y,
                EditorTableStyles.ToolbarButtonWidth, area.height);

            GUI.Label(secondary, ClearLabel, styles.SecondaryButton);

            float searchWidth = Mathf.Min(EditorTableStyles.SearchWidth,
                Mathf.Max(0f, area.xMax - secondary.xMax - EditorMetrics.ItemGap));

            Rect search = new(area.xMax - searchWidth, area.y + (area.height - SearchFieldHeight) * 0.5f,
                searchWidth, SearchFieldHeight);

            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(search, EditorPalette.Field);
                DrawOutline(search, EditorPalette.Border);
            }

            GUI.Label(Inset(search, EditorMetrics.TightGap), SearchText, styles.Detail);
        }

        private static void DrawSummary(Rect area, EditorTableStyles styles)
        {
            GUI.Label(area, SummaryText, styles.Summary);

            float width = EditorRows.MeasureBadge(SummaryPillText, styles.Badge);
            Rect pill = new(area.xMax - width, area.y + (area.height - EditorMetrics.PillHeight) * 0.5f,
                width, EditorMetrics.PillHeight);

            DrawPill(pill, SummaryPillText, EditorTableStyles.SummaryOkColor, styles);
        }

        private static void DrawCard(Rect area, EditorTableStyles styles)
        {
            GUI.Label(area, GUIContent.none, styles.Card);

            Rect body = new(area.x, area.y + EditorTableStyles.CardPadding, area.width,
                area.height - EditorTableStyles.CardPadding * 2f);

            Rect header = new(body.x, body.y, body.width, EditorMetrics.HeaderHeight);

            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(header, EditorTableStyles.HeaderColor);

            DrawHeaderTitles(header, styles);

            Rect separator = new(body.x, header.yMax, body.width, EditorMetrics.SeparatorThickness);
            EditorRows.DrawSeparator(separator);

            for (int index = 0; index < RowCount; index++)
            {
                Rect row = new(body.x, separator.yMax + index * EditorTableStyles.RowHeight, body.width,
                    EditorTableStyles.RowHeight);

                DrawRow(row, index, styles);
            }

            DrawDivider(body, header.yMax);
        }

        private static void DrawHeaderTitles(Rect header, EditorTableStyles styles)
        {
            Rect name = NameCell(header);

            GUI.Label(name, HeaderName, styles.Header);

            Rect arrow = new(name.x
                + styles.Header.CalcSize(new GUIContent(HeaderName)).x
                + EditorTableStyles.HeaderArrowGap, header.y, EditorMetrics.SortArrowWidth, header.height);

            EditorRows.DrawSortArrow(arrow, ESortOrder.Ascending, EditorPalette.Accent);

            GUI.Label(DetailCell(header), HeaderDetail, styles.Header);
            GUI.Label(BadgeCell(header), HeaderState, styles.Header);
        }

        // The divider is drawn by hand rather than through EditorColumnDividers, because the preview
        // has no columns to resize and that class would claim the mouse events of the settings page.
        private static void DrawDivider(Rect body, float top)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            float x = DetailCell(body).x - EditorTableStyles.RowInset * 0.5f;

            EditorGUI.DrawRect(new Rect(x, top, EditorMetrics.DividerThickness, body.yMax - top),
                EditorPalette.Divider);
        }

        private static void DrawRow(Rect row, int index, EditorTableStyles styles)
        {
            EditorRows.DrawRowBackground(row, index, index == HoveredRow, index == SelectedRow);

            GUI.Label(NameCell(row), RowName(index), styles.NameBold);
            GUI.Label(DetailCell(row), DetailText, styles.Detail);

            DrawPill(BadgePill(row), BadgeText, BadgeColor(index), styles);

            Rect ping = PingCell(row);

            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(ping, EditorTableStyles.PingRestColor);

            GUI.Label(ping, PingLabel, styles.Ping);
        }

        private static string RowName(int index)
        {
            if (index == SelectedRow)
                return SelectedRowName;

            if (index == HoveredRow)
                return HoverRowName;

            return index % 2 != 0
                ? StripedRowName
                : PlainRowName;
        }

        private static Color BadgeColor(int index) => index switch
        {
            0 => EditorTableStyles.OkBadgeColor,
            1 => EditorTableStyles.WarningBadgeColor,
            2 => EditorTableStyles.DangerBadgeColor,
            _ => EditorTableStyles.NeutralBadgeColor
        };

        private static void DrawPill(Rect area, string text, Color fill, EditorTableStyles styles)
        {
            Color previous = GUI.color;

            GUI.color = fill;
            GUI.Label(area, GUIContent.none, styles.PillBackground);
            GUI.color = previous;

            GUI.Label(area, text, styles.Badge);
        }

        private static void DrawOutline(Rect area, Color color)
        {
            float thickness = EditorMetrics.SeparatorThickness;

            EditorGUI.DrawRect(new Rect(area.x, area.y, area.width, thickness), color);
            EditorGUI.DrawRect(new Rect(area.x, area.yMax - thickness, area.width, thickness), color);
            EditorGUI.DrawRect(new Rect(area.x, area.y, thickness, area.height), color);
            EditorGUI.DrawRect(new Rect(area.xMax - thickness, area.y, thickness, area.height), color);
        }

        private static Rect NameCell(Rect row) => new(row.x + EditorTableStyles.RowInset, row.y,
            NameWidth(row), row.height);

        private static Rect DetailCell(Rect row)
        {
            Rect name = NameCell(row);
            float right = BadgeCell(row).x - EditorMetrics.ItemGap;

            return new Rect(name.xMax, row.y, Mathf.Max(0f, right - name.xMax), row.height);
        }

        private static Rect BadgeCell(Rect row) => new(PingCell(row).x
            - EditorTableStyles.BadgeGap
            - EditorTableStyles.MinBadgeWidth, row.y, EditorTableStyles.MinBadgeWidth, row.height);

        private static Rect PingCell(Rect row)
        {
            float height = Mathf.Min(EditorMetrics.BadgeHeight, row.height);

            return new Rect(row.xMax - EditorTableStyles.RowInset - EditorTableStyles.PingButtonWidth,
                row.y + (row.height - height) * 0.5f, EditorTableStyles.PingButtonWidth, height);
        }

        private static Rect BadgePill(Rect row)
        {
            Rect cell = BadgeCell(row);

            return new Rect(cell.x, cell.y + (cell.height - EditorMetrics.PillHeight) * 0.5f, cell.width,
                EditorMetrics.PillHeight);
        }

        private static float NameWidth(Rect row) => Mathf.Max(0f,
            (row.width - EditorTableStyles.RowInset * 2f) * NameColumnShare);
    }
}