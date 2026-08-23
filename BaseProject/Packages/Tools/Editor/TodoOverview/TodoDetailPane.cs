using Base.ToolPackage.Editor.TodoOverview.Model;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.TodoOverview
{
    /// <summary>
    /// The panel under the list that shows the selected item in full. A row can only ever show the
    /// first line of an item, so this is where a comment that runs over several lines becomes
    /// readable, together with the file it sits in.
    /// </summary>
    internal static class TodoDetailPane
    {
        private const float ButtonWidth = 58f;
        private const string CopyLabel = "Copy";
        private const string CopyTooltip = "Copy the full text of this item";
        private const string LineBreak = "\n";
        private const string LocationFormat = "{0}:{1}";
        private const string OpenLabel = "Open";
        private const string OpenTooltip = "Open the file at this line";
        private const float Padding = 8f;
        private const string PingLabel = "Ping";
        private const string PingTooltip = "Highlight the file in the project window";
        private const string UnassignedLabel = "Unassigned";

        private static readonly GUIContent CopyContent = new(CopyLabel, CopyTooltip);
        private static readonly GUIContent OpenContent = new(OpenLabel, OpenTooltip);
        private static readonly GUIContent PingContent = new(PingLabel, PingTooltip);

        /// <summary>Draws the panel for one item.</summary>
        /// <param name="area">The area the panel fills.</param>
        /// <param name="entry">The item to show.</param>
        /// <param name="accent">The color of the item's keyword.</param>
        internal static void Draw(Rect area, TodoEntry entry, Color accent)
        {
            TodoChrome.DrawFill(area, TodoStyles.PanelColor(), TodoStyles.DetailRadius);
            TodoChrome.DrawFill(new Rect(area.x, area.y, TodoStyles.BandWidth, area.height), accent,
                TodoStyles.DetailRadius);

            Rect content = new(area.x + TodoStyles.BandWidth + Padding, area.y + Padding,
                area.width - TodoStyles.BandWidth - Padding * 2f, area.height - Padding * 2f);

            Rect header = new(content.x, content.y, content.width, TodoStyles.ButtonHeight);
            DrawHeader(header, entry, accent);

            Rect path = new(content.x, content.yMax - EditorGUIUtility.singleLineHeight, content.width,
                EditorGUIUtility.singleLineHeight);

            float bodyTop = header.yMax + TodoStyles.Gap;
            Rect body = new(content.x, bodyTop, content.width, Mathf.Max(0f, path.y - bodyTop));

            GUI.Label(body, BuildText(entry), TodoStyles.DetailBody);
            GUI.Label(path, string.Format(LocationFormat, entry.AssetPath, entry.Line), TodoStyles.Path);
        }

        private static string BuildText(TodoEntry entry) => entry.Details.Length == 0
            ? entry.Message
            : entry.Message + LineBreak + entry.Details;

        private static void DrawHeader(Rect header, TodoEntry entry, Color accent)
        {
            Rect keyword = new(header.x, header.center.y - TodoStyles.ChipHeight * 0.5f, TodoStyles.ChipWidth,
                TodoStyles.ChipHeight);

            TodoChrome.DrawPill(keyword, new GUIContent(entry.Keyword), accent, TodoStyles.Chip);

            float left = DrawButtons(header, entry);

            Rect meta = new(keyword.xMax + TodoStyles.Gap, header.y, Mathf.Max(0f, left - keyword.xMax
                - TodoStyles.Gap * 2f), header.height);

            GUI.Label(meta, Owner(entry), TodoStyles.Path);
        }

        // Right to left, so the primary action ends up furthest from the edge the eye leaves at.
        private static float DrawButtons(Rect header, TodoEntry entry)
        {
            Rect button = new(header.xMax - ButtonWidth, header.y, ButtonWidth, TodoStyles.ButtonHeight);

            if (TodoChrome.DrawButton(button, PingContent, TodoStyles.ControlColor(), TodoStyles.Button,
                    TodoStyles.ButtonRadius))
                TodoNavigator.Ping(entry);

            button.x -= ButtonWidth + TodoStyles.TightGap;

            if (TodoChrome.DrawButton(button, CopyContent, TodoStyles.ControlColor(), TodoStyles.Button,
                    TodoStyles.ButtonRadius))
                EditorGUIUtility.systemCopyBuffer = BuildText(entry);

            button.x -= ButtonWidth + TodoStyles.TightGap;

            if (TodoChrome.DrawButton(button, OpenContent, TodoStyles.AccentColor(), TodoStyles.Chip,
                    TodoStyles.ButtonRadius))
                TodoNavigator.Open(entry);

            return button.x;
        }

        private static string Owner(TodoEntry entry) => entry.Owner.Length == 0
            ? UnassignedLabel
            : entry.Owner;
    }
}