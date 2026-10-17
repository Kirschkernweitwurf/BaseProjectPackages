using Base.EditorUIPackage.Editor;
using UnityEditor;
using UnityEngine;

namespace Base.DataHubPackage.Editor
{
    /// <summary>
    /// The small hand-drawn controls every part of the Data Hub shares: icon buttons, the segmented
    /// control of the tabs and filters, rounded pills and the count stepper of the list tabs.
    /// </summary>
    internal static class DataHubGui
    {
        private const string MinusTooltip = "Take one copy of each selected asset out.";
        private const string ParentSeparator = " / ";
        private const string PlusTooltip = "Add one copy of each selected asset.";
        private const float SegmentInset = 2f;
        private const float SubtitleHeight = 16f;
        private const float TitleHeight = 22f;
        private const float TitleTop = 7f;

        /// <summary>Fills a rectangle on the repaint pass only.</summary>
        /// <param name="area">The area to fill.</param>
        /// <param name="color">The fill color.</param>
        internal static void Fill(Rect area, Color color)
        {
            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(area, color);
        }

        /// <summary>
        /// Draws an icon that fits the area. Built-in icons stay pixel sharp at their own size, a large
        /// custom script icon is scaled down to fit instead of drawn at its full size.
        /// </summary>
        /// <param name="area">The area to fit the icon in.</param>
        /// <param name="icon">The icon. Nothing is drawn when this is null.</param>
        internal static void Icon(Rect area, Texture icon)
        {
            if (Event.current.type != EventType.Repaint
                || icon == null)
                return;

            float points = Mathf.Max(1f, EditorGUIUtility.pixelsPerPoint);

            if (icon.width / points <= area.width
                && icon.height / points <= area.height)
            {
                EditorIcons.Draw(area, icon);
                return;
            }

            GUI.DrawTexture(area, icon, ScaleMode.ScaleToFit);
        }

        /// <summary>Draws a style as a plain background on the repaint pass only.</summary>
        /// <param name="area">The area to fill.</param>
        /// <param name="style">The background style.</param>
        internal static void Background(Rect area, GUIStyle style)
        {
            if (Event.current.type == EventType.Repaint)
                style.Draw(area, false, false, false, false);
        }

        /// <summary>Draws a rounded highlight in any color on the repaint pass only.</summary>
        /// <param name="area">The area to fill.</param>
        /// <param name="color">The fill color.</param>
        /// <param name="styles">The built window styles.</param>
        internal static void Highlight(Rect area, Color color, DataHubStyles styles)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            Color previous = GUI.color;
            GUI.color = color;
            styles.RoundedFill.Draw(area, false, false, false, false);
            GUI.color = previous;
        }

        /// <summary>
        /// Draws the two lines at the top of a pane: an icon, a title with an optional dimmed parent path
        /// before it, and a dimmed line under it.
        /// </summary>
        /// <param name="area">The header area. Its height is <see cref="DataHubStyles.PaneHeaderHeight"/>.</param>
        /// <param name="icon">The icon before the title, or null.</param>
        /// <param name="parent">The dimmed path before the title, or null.</param>
        /// <param name="title">The title.</param>
        /// <param name="subtitle">The dimmed line under the title, or null.</param>
        /// <param name="styles">The built window styles.</param>
        /// <param name="tooltip">What the title stands for, shown over both lines, or null.</param>
        internal static void PaneTitle(Rect area, Texture icon, string parent, string title, string subtitle,
            DataHubStyles styles, string tooltip = null)
        {
            if (!string.IsNullOrEmpty(tooltip))
                GUI.Label(area, new GUIContent(string.Empty, tooltip));

            Rect line = new(area.x + DataHubStyles.PanelPadding, area.y + TitleTop,
                Mathf.Max(0f, area.width - DataHubStyles.PanelPadding * 2f), TitleHeight);

            if (icon != null)
            {
                Icon(new Rect(line.x, line.y + (line.height - EditorTableStyles.IconSize) * 0.5f,
                    EditorTableStyles.IconSize, EditorTableStyles.IconSize), icon);

                float shift = EditorTableStyles.IconSize + EditorTableStyles.IconGap;
                line.x += shift;
                line.width = Mathf.Max(0f, line.width - shift);
            }

            Rect subtitleLine = new(line.x, line.yMax, line.width, SubtitleHeight);

            if (!string.IsNullOrEmpty(parent))
            {
                GUIContent prefix = new(parent + ParentSeparator);
                float width = Mathf.Min(line.width, styles.TitleDim.CalcSize(prefix).x);

                GUI.Label(new Rect(line.x, line.y, width, line.height), prefix, styles.TitleDim);

                line.x += width;
                line.width = Mathf.Max(0f, line.width - width);
            }

            GUI.Label(line, title, styles.Title);

            if (!string.IsNullOrEmpty(subtitle))
                GUI.Label(subtitleLine, subtitle, styles.Summary);
        }

        /// <summary>Draws a square button that only shows an icon.</summary>
        /// <param name="area">The button area.</param>
        /// <param name="icon">The icon.</param>
        /// <param name="tooltip">What the button does.</param>
        /// <param name="styles">The built window styles.</param>
        /// <returns>True when it was clicked.</returns>
        internal static bool IconButton(Rect area, Texture icon, string tooltip, DataHubStyles styles)
            => GUI.Button(area, new GUIContent(icon, tooltip), styles.IconButton);

        /// <summary>Draws a rounded pill with a centered text.</summary>
        /// <param name="area">The pill area.</param>
        /// <param name="content">The text and its tooltip.</param>
        /// <param name="fill">The pill color.</param>
        /// <param name="styles">The built window styles.</param>
        internal static void Pill(Rect area, GUIContent content, Color fill, DataHubStyles styles)
        {
            if (Event.current.type == EventType.Repaint)
            {
                Color previous = GUI.color;

                // An opaque base first, so a see-through badge color keeps its hue on a selected or hovered row.
                GUI.color = EditorPalette.Background;
                styles.PillBackground.Draw(area, false, false, false, false);

                GUI.color = fill;
                styles.PillBackground.Draw(area, false, false, false, false);
                GUI.color = previous;
            }

            GUI.Label(area, content, styles.Badge);
        }

        /// <summary>The width a pill needs for a text.</summary>
        /// <param name="text">The pill text.</param>
        /// <param name="styles">The built window styles.</param>
        /// <returns>The width.</returns>
        internal static float PillWidth(string text, DataHubStyles styles)
            => EditorRows.MeasureBadge(text, styles.Badge, EditorMetrics.BadgeHeight);

        /// <summary>
        /// Draws a row of segments on one track, the active one raised, like the tabs of a panel.
        /// </summary>
        /// <param name="area">The control area.</param>
        /// <param name="selected">The index of the active segment.</param>
        /// <param name="segments">The label of each segment.</param>
        /// <param name="styles">The built window styles.</param>
        /// <returns>The index of the active segment after this pass.</returns>
        internal static int Segmented(Rect area, int selected, GUIContent[] segments, DataHubStyles styles)
        {
            Background(area, styles.SegmentTrack);

            float width = area.width / segments.Length;

            for (int i = 0; i < segments.Length; i++)
            {
                Rect segment = new(area.x + i * width, area.y, width, area.height);
                bool isActive = i == selected;

                if (isActive)
                {
                    Background(new Rect(segment.x + SegmentInset, segment.y + SegmentInset,
                        segment.width - SegmentInset * 2f, segment.height - SegmentInset * 2f), styles.SegmentThumb);
                }

                GUIStyle label = isActive
                    ? styles.SegmentOn
                    : styles.Segment;

                if (GUI.Button(segment, segments[i], label))
                    selected = i;
            }

            return selected;
        }

        /// <summary>
        /// Draws minus, a typed number and plus on one track, for how often an asset is in a list.
        /// </summary>
        /// <param name="area">The stepper area.</param>
        /// <param name="count">The current count.</param>
        /// <param name="isMixed">True when the selected assets have different counts.</param>
        /// <param name="styles">The built window styles.</param>
        /// <param name="isStep">
        /// True when minus or plus was pressed rather than a number typed. With mixed counts a step means
        /// one more or less of each, a typed number means exactly that many of each.
        /// </param>
        /// <returns>The count after this pass. The same value when nothing was pressed or typed.</returns>
        internal static int Stepper(Rect area, int count, bool isMixed, DataHubStyles styles, out bool isStep)
        {
            isStep = true;

            Background(area, styles.SegmentTrack);

            float button = area.height;
            Rect minus = new(area.x, area.y, button, area.height);
            Rect plus = new(area.xMax - button, area.y, button, area.height);
            Rect field = new(minus.xMax, area.y, plus.x - minus.xMax, area.height);

            using (new EditorGUI.DisabledScope(count <= 0 && !isMixed))
            {
                if (IconButton(minus, DataHubIcons.Minus, MinusTooltip, styles))
                    return count - 1;
            }

            if (IconButton(plus, DataHubIcons.New, PlusTooltip, styles))
                return count + 1;

            isStep = false;

            EditorGUI.showMixedValue = isMixed;
            int typed = EditorGUI.DelayedIntField(field, count, styles.StepperNumber);
            EditorGUI.showMixedValue = false;

            return typed;
        }
    }
}