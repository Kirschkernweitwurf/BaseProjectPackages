using System.Collections.Generic;
using Base.EditorUIPackage.Editor;
using UnityEditor;
using UnityEngine;

namespace Base.ToolsPackage.Editor.BaseToolsOverview
{
    /// <summary>
    /// Fills the Base Tools node of the project settings, which Unity otherwise leaves blank
    /// because nothing is registered at the path itself, with an overview of everything below it:
    /// one striped row per page with its description and a button that jumps there.
    /// <para>
    /// The list comes from <see cref="BaseToolsPageCatalog"/> and is rebuilt the first time the
    /// page draws, so a new tool appears on its own. The only thing worth adding by hand is the
    /// sentence under a name, through <see cref="BaseToolsPageAttribute"/>.
    /// </para>
    /// </summary>
    internal static class BaseToolsOverviewProvider
    {
        private const float ButtonGap = 4f;
        private const string EmptyMessage = "Nothing is registered under this path yet. A page shows up here "
            + "as soon as a settings provider is created for it.";
        private const string Intro = "Every settings page of the Base tools. Pick one to configure it.";
        private const int LeftMouseButton = 0;
        private const int NoRow = -1;
        private const float OpenButtonHeight = 20f;
        private const float OpenButtonWidth = 72f;
        private const string OpenLabel = "Open";
        private const string PageLabel = "Base Tools";
        private const float RowHeight = RowPadding * 2f
            + TitleLineHeight
            + SummaryLineHeight
            + ButtonGap
            + OpenButtonHeight;
        private const float RowPadding = 4f;
        private const float SummaryLineHeight = 16f;
        private const float TitleLineHeight = 18f;

        private static readonly GUILayoutOption ExpandWidth = GUILayout.ExpandWidth(true);
        private static readonly BaseToolsOverviewStyles Styles = new();

        // Reused rather than allocated per row, because the tooltip carries the full sentence the
        // column may have to clip.
        private static readonly GUIContent SummaryContent = new();

        private static BaseToolsPage[] _pages;
        private static int _hoveredRow = NoRow;

        [SettingsProvider]
        private static SettingsProvider Create() => new(BaseToolsPageCatalog.RootPath, SettingsScope.Project)
        {
            label = PageLabel,
            keywords = new HashSet<string>
            {
                "base",
                "tools",
                "packages",
                "overview"
            },

            // Dropped rather than collected here, so the list is rebuilt on the first draw. The
            // walk builds every settings provider in the project, which is not something a domain
            // reload should be paying for, and this way a reload with the page open refills it too.
            activateHandler = (_, _) => _pages = null,
            deactivateHandler = () =>
            {
                _pages = null;
                _hoveredRow = NoRow;

                Styles.Dispose();
            },
            guiHandler = _ => DrawGui()
        };

        private static void DrawGui()
        {
            Styles.EnsureBuilt();

            _pages ??= BaseToolsPageCatalog.Collect();

            GUILayout.Label(Intro, Styles.Intro);
            EditorGUILayout.Space(EditorMetrics.SectionGap);

            if (_pages.Length == 0)
            {
                EditorGUILayout.HelpBox(EmptyMessage, MessageType.Info);
                return;
            }

            int hovered = NoRow;

            for (int i = 0; i < _pages.Length; i++)
            {
                if (DrawPage(_pages[i], i))
                    hovered = i;
            }

            UpdateHover(hovered);
        }

        // The settings window only repaints while something happens in it, so the row the mouse
        // sits on has to ask for the frame that draws its tint.
        private static void UpdateHover(int hovered)
        {
            if (hovered == _hoveredRow)
                return;

            _hoveredRow = hovered;

            EditorWindow window = EditorWindow.mouseOverWindow;

            if (window == null)
                return;

            window.Repaint();
        }

        // Laid out by explicit rectangles rather than nested layout groups: the settings page is as
        // wide as the window, and a layout group hands that width to whatever can stretch, which is
        // what pushed the pieces of a row to opposite ends of the screen.
        private static bool DrawPage(BaseToolsPage page, int index)
        {
            Rect row = GUILayoutUtility.GetRect(0f, RowHeight, ExpandWidth);
            bool isHovered = row.Contains(Event.current.mousePosition);

            DrawRowBackground(row, index, isHovered);

            Rect content = new(row.x + EditorMetrics.RowInset, row.y + RowPadding,
                row.width - EditorMetrics.RowInset * 2f, row.height - RowPadding * 2f);

            Rect title = new(content.x, content.y, content.width, TitleLineHeight);

            GUI.Label(title, page.Label, Styles.Name);

            Rect summary = new(content.x, title.yMax, content.width, SummaryLineHeight);

            DrawSummary(summary, page.Summary);
            DrawOpenButton(new Rect(content.x, summary.yMax + ButtonGap, OpenButtonWidth, OpenButtonHeight),
                page.Path);

            HandleRowClick(row, page.Path);

            return isHovered;
        }

        // Striping first, then the accent wash on top, so the row under the mouse reads as one
        // target for the click the whole row accepts.
        private static void DrawRowBackground(Rect row, int index, bool isHovered)
        {
            EditorRows.DrawRowBackground(row, index);

            if (!isHovered || Event.current.type != EventType.Repaint)
                return;

            EditorGUI.DrawRect(row, EditorPalette.SelectionFill);
        }

        // The column can be narrower than the sentence, so the full text is repeated as a tooltip
        // rather than being lost to the ellipsis.
        private static void DrawSummary(Rect cell, string summary)
        {
            if (string.IsNullOrEmpty(summary))
                return;

            SummaryContent.text = summary;
            SummaryContent.tooltip = summary;

            GUI.Label(cell, SummaryContent, Styles.Summary);
        }

        private static void DrawOpenButton(Rect button, string path)
        {
            if (GUI.Button(button, OpenLabel, Styles.OpenButton))
                Open(path);
        }

        // The whole row reacts, not just the button: a row that reads as one thing and only
        // responds in one spot reads as broken. The button consumes its own click, so it never
        // arrives here twice.
        private static void HandleRowClick(Rect row, string path)
        {
            EditorGUIUtility.AddCursorRect(row, MouseCursor.Link);

            Event current = Event.current;

            if (current.type != EventType.MouseDown
                || current.button != LeftMouseButton
                || !row.Contains(current.mousePosition))
                return;

            Open(path);
            current.Use();
        }

        // Switching pages while one is drawing leaves the settings window mid layout, so the jump
        // waits for the frame to end.
        private static void Open(string path)
            => EditorApplication.delayCall += () => SettingsService.OpenProjectSettings(path);
    }
}