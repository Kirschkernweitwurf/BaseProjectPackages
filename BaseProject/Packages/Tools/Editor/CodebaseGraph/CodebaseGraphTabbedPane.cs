using System;
using Base.ToolPackage.Editor.CodebaseGraph.Analysis;
using Base.ToolPackage.Editor.CodebaseGraph.Model;
using UnityEngine.UIElements;

namespace Base.ToolPackage.Editor.CodebaseGraph
{
    /// <summary>
    /// The three ways of looking at what a scan produced, sharing one column. They used to be a pane
    /// and two floating windows, which meant the detail panel sat unused while you worked through a
    /// list. The list had no way to tell you anything its own row did not already say.
    /// <br/><br/>
    /// Only one is built into the tree at a time. All three exist from the start, because keeping their
    /// scroll position and their filters across a switch is the whole reason a tab beats a button.
    /// </summary>
    internal sealed class CodebaseGraphTabbedPane : VisualElement
    {
        private const string DismissedFormat = "Dismissed ({0})";
        private const string DismissedTooltip = "Everything you set aside, and anything that stopped "
            + "matching the code.";

        private const string EntriesLabel = "Entries";
        private const string EntriesTooltip = "What is on screen in the graph.";
        private const string IssuesFormat = "Issues ({0})";
        private const string IssuesTooltip = "Every finding as a plain list, worst first.";

        private readonly CodebaseGraphListPane _entriesPane;
        private readonly CodebaseGraphIssuesPane _issuesPane;
        private readonly CodebaseGraphDismissalsPane _dismissalsPane;
        private readonly VisualElement _host;
        private readonly Button _entriesTab;
        private readonly Button _issuesTab;
        private readonly Button _dismissedTab;

        private VisualElement _shown;

        /// <summary>Builds the column and its three views.</summary>
        /// <param name="entriesPane">The list of what the graph is showing.</param>
        public CodebaseGraphTabbedPane(CodebaseGraphListPane entriesPane)
        {
            _entriesPane = entriesPane;
            _issuesPane = new CodebaseGraphIssuesPane();
            _dismissalsPane = new CodebaseGraphDismissalsPane();

            AddToClassList(CodebaseGraphStyle.PaneClass);

            VisualElement tabs = new();
            tabs.AddToClassList(CodebaseGraphStyle.PaneTabRowClass);

            _entriesTab = BuildTab(EntriesLabel, EntriesTooltip, action: () => Show(_entriesPane));
            _issuesTab = BuildTab(string.Format(IssuesFormat, 0), IssuesTooltip, action: () => Show(_issuesPane));

            _dismissedTab = BuildTab(string.Format(DismissedFormat, 0),
                DismissedTooltip,
                action: () => Show(_dismissalsPane));

            tabs.Add(_entriesTab);
            tabs.Add(_issuesTab);
            tabs.Add(_dismissedTab);
            Add(tabs);

            _host = new VisualElement
            {
                style =
                {
                    flexGrow = 1f
                }
            };

            Add(_host);
            Show(_entriesPane);
        }

        /// <summary>Hands a freshly built graph to the views that read one.</summary>
        /// <param name="graph">Graph to read, or null when nothing has been scanned.</param>
        internal void SetGraph(CodebaseGraphData graph)
        {
            _issuesPane.SetGraph(graph);
            _dismissalsPane.Refresh();
            SetCounts();
        }

        /// <summary>Writes the counts onto the tabs, so the column says what is waiting on each.</summary>
        internal void SetCounts()
        {
            _issuesTab.text = string.Format(IssuesFormat, _issuesPane.VisibleCount);
            _dismissedTab.text = string.Format(DismissedFormat, DismissalStore.Count);
        }

        private static Button BuildTab(string label, string tooltip, Action action)
        {
            Button tab = new(action)
            {
                text = label,
                tooltip = tooltip
            };

            tab.AddToClassList(CodebaseGraphStyle.PaneTabClass);
            return tab;
        }

        private void Show(VisualElement pane)
        {
            if (_shown == pane)
                return;

            _host.Clear();
            _host.Add(pane);
            _shown = pane;

            _entriesTab.EnableInClassList(CodebaseGraphStyle.IsActiveClass, pane == _entriesPane);
            _issuesTab.EnableInClassList(CodebaseGraphStyle.IsActiveClass, pane == _issuesPane);
            _dismissedTab.EnableInClassList(CodebaseGraphStyle.IsActiveClass, pane == _dismissalsPane);
        }
    }
}