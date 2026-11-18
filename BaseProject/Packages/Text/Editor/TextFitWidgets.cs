using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Base.EditorUIPackage.Editor;
using UnityEditor;
using UnityEngine;

namespace Base.TextPackage.Editor
{
    /// <summary>
    /// The small pieces the Text Fit window is drawn from: badges, pill buttons, filter chips and one-line texts.
    /// Laid out with <see cref="GUILayout"/>, each one line high, so they line up in a row. Other check windows
    /// can share the row pieces, so the checks look alike.
    /// </summary>
    public static class TextFitWidgets
    {
        private const string BoxLabel = "Show Box";
        private const string BoxTooltip = "Opens the prefab or scene and selects the box, so it can be resized.";
        private const string Breadcrumb = " › ";
        private const float ChipPadding = 14f;
        private const string Ellipsis = "…";
        private const string LeadFormat = "{0}  ·  {1}";
        private const string LineBreak = "  ↵  ";

        /// <summary>The width of a language badge, so the texts next to them line up.</summary>
        internal const float LocaleWidth = 58f;

        // The asset and the box name, which the path never gives up.
        private const int MinCrumbs = 2;

        private const float PillPadding = 16f;
        private const float SelectedChipAlpha = 0.55f;
        private const float StripeAlpha = 0.07f;

        private static readonly Regex Tags = new("<[^>]+>");

        /// <summary>
        /// <paramref name="text"/> without rich text tags and on one line, so a row shows what a player reads.
        /// </summary>
        internal static string Plain(string text) => Tags.Replace(text ?? string.Empty, string.Empty)
            .Replace("\r", string.Empty)
            .Replace("\n", LineBreak);

        /// <summary>A filled, rounded badge in a line-high slot, like a language code or a size.</summary>
        public static void Badge(EditorWindowStyles styles, string text, Color fill, float minimumWidth,
            string tooltip = null)
        {
            float width = EditorRows.MeasureBadge(text, styles.Badge, minimumWidth);
            Rect slot = GUILayoutUtility.GetRect(width, EditorGUIUtility.singleLineHeight, GUILayout.Width(width));
            Rect pill = new(slot.x, slot.center.y - EditorMetrics.BadgeHeight * 0.5f, width, EditorMetrics.BadgeHeight);

            // Rounded like the chips and buttons, so the window reads as one family of shapes.
            DrawPill(styles, pill, fill);
            GUI.Label(pill, new GUIContent(text, tooltip), styles.Badge);
        }

        /// <summary>The width a filter chip with <paramref name="label"/> takes, for wrapping a row of them.</summary>
        public static float ChipWidth(EditorWindowStyles styles, string label)
            => styles.Badge.CalcSize(new GUIContent(label)).x + ChipPadding;

        /// <summary>
        /// A rounded chip that filters the list, filled in <paramref name="fill"/> and in the accent color while it
        /// is picked. Returns true when clicked.
        /// </summary>
        public static bool Chip(EditorWindowStyles styles, string label, string tooltip, Color fill,
            bool isSelected)
        {
            float width = ChipWidth(styles, label);
            Rect slot = GUILayoutUtility.GetRect(width, EditorMetrics.PillHeight, GUILayout.Width(width));
            Color shown = isSelected
                ? EditorPalette.WithAlpha(EditorPalette.Accent, SelectedChipAlpha)
                : fill;

            DrawPill(styles, slot, EditorStyleUtility.Shade(shown, slot.Contains(Event.current.mousePosition),
                false));
            EditorGUIUtility.AddCursorRect(slot, MouseCursor.Link);

            return GUI.Button(slot, new GUIContent(label, tooltip), styles.Badge);
        }

        /// <summary>
        /// A small text button with a soft fill that lights up under the mouse, for an action on a row. Returns true
        /// when clicked.
        /// </summary>
        public static bool PillButton(EditorWindowStyles styles, string label, string tooltip)
        {
            GUIContent content = new(label, tooltip);
            float width = styles.PingHot.CalcSize(content).x + PillPadding;
            Rect slot = GUILayoutUtility.GetRect(width, EditorGUIUtility.singleLineHeight, GUILayout.Width(width));
            Rect pill = new(slot.x, slot.center.y - EditorMetrics.PillHeight * 0.5f, width, EditorMetrics.PillHeight);
            bool isHovered = pill.Contains(Event.current.mousePosition);

            DrawPill(styles, pill, isHovered
                ? EditorTableStyles.PingHoverColor
                : EditorTableStyles.PingRestColor);
            EditorGUIUtility.AddCursorRect(pill, MouseCursor.Link);

            return GUI.Button(pill, content, isHovered
                ? styles.PingHot
                : styles.Ping);
        }

        /// <summary>
        /// A button that opens the prefab or scene of <paramref name="location"/> and selects the box there.
        /// </summary>
        internal static void ShowBoxButton(EditorWindowStyles styles, TextBoxLocation location)
        {
            if (!PillButton(styles, BoxLabel, BoxTooltip))
                return;

            location.Show();

            // Opening a prefab or a scene rebuilds the GUI, so the rest of this pass is skipped.
            GUIUtility.ExitGUI();
        }

        /// <summary>A built-in icon in a line-high square, drawn crisp.</summary>
        public static void Icon(Texture icon)
        {
            float size = EditorGUIUtility.singleLineHeight;
            EditorIcons.Draw(GUILayoutUtility.GetRect(size, size, GUILayout.Width(size)), icon);
        }

        /// <summary>
        /// A label that fills the rest of its row and ends in '…' when the window is too narrow, instead of pushing
        /// the row wider.
        /// </summary>
        public static void Label(string text, GUIStyle style, string tooltip = null)
        {
            GUIContent content = new(text, tooltip ?? text);

            // It takes the rest of the row, so what follows sits at the right edge.
            GUILayout.Label(content, style, GUILayout.MinWidth(0f), GUILayout.Height(SizeOf(content, style).y),
                GUILayout.ExpandWidth(true));
        }

        /// <summary>
        /// The background of one row in a list: every second row on a clear stripe and a hairline above every row
        /// but the first, so the eye never slips into the neighbor.
        /// </summary>
        public static void RowBackground(Rect row, int index)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            if (index % 2 != 0)
                EditorGUI.DrawRect(row, EditorPalette.Tint(StripeAlpha));

            if (index > 0)
                EditorRows.DrawSeparator(row);
        }

        /// <summary>
        /// The path of <paramref name="location"/> on one line, after <paramref name="lead"/> like "UI". When the
        /// window is too narrow, the objects in the middle give way to '…' first, so the asset and the box name stay
        /// readable.
        /// </summary>
        /// <param name="location">The box whose path to show.</param>
        /// <param name="lead">A word in front, like the area or null.</param>
        /// <param name="style">The text style.</param>
        /// <param name="end">
        /// Where the line has to stop, like the left edge of the buttons above it or null for the full width.
        /// </param>
        internal static void PathLabel(TextBoxLocation location, string lead, GUIStyle style, float? end = null)
        {
            List<string> crumbs = location.Crumbs.ToList();
            GUIContent content = new(Join(lead, crumbs, false), location.Path);
            Rect slot = GUILayoutUtility.GetRect(0f, SizeOf(content, style).y, GUILayout.MinWidth(0f),
                GUILayout.ExpandWidth(true));

            // Only the repaint knows the real width and only the repaint shows the text.
            if (Event.current.type != EventType.Repaint)
                return;

            if (end.HasValue)
                slot.xMax = Mathf.Min(slot.xMax, end.Value);

            while (crumbs.Count > MinCrumbs
                && SizeOf(content, style).x > slot.width)
            {
                crumbs.RemoveAt(1);
                content.text = Join(lead, crumbs, true);
            }

            style.Draw(slot, content, false, false, false, false);
        }

        // A style that ends in '…' measures as nothing, since it could shrink to the dots, so it is measured with
        // plain clipping.
        private static Vector2 SizeOf(GUIContent content, GUIStyle style)
        {
            TextClipping clipping = style.clipping;
            style.clipping = TextClipping.Clip;
            Vector2 size = style.CalcSize(content);
            style.clipping = clipping;

            return size;
        }

        // The gap sits right after the asset, where the removed objects were.
        private static string Join(string lead, List<string> crumbs, bool isShortened)
        {
            List<string> shown = new(crumbs);

            if (isShortened)
                shown.Insert(1, Ellipsis);

            string path = string.Join(Breadcrumb, shown);

            return string.IsNullOrEmpty(lead)
                ? path
                : string.Format(LeadFormat, lead, path);
        }

        private static void DrawPill(EditorWindowStyles styles, Rect area, Color fill)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            Color previous = GUI.color;
            GUI.color = fill;
            styles.PillBackground.Draw(area, false, false, false, false);
            GUI.color = previous;
        }
    }
}