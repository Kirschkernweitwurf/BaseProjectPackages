using System;
using System.Collections.Generic;
using Base.ToolsPackage.Editor.CodebaseGraph.Analysis;
using Base.ToolsPackage.Editor.CodebaseGraph.Model;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Base.ToolsPackage.Editor.CodebaseGraph
{
    /// <summary>
    /// Lists everything that was dismissed during triage. Dismissing is easy to do in passing, which is
    /// exactly why it needs somewhere to be looked at afterward: without this the only record is a JSON
    /// file, and a decision you cannot review is a decision you stop trusting.
    /// </summary>
    internal sealed class CodebaseGraphDismissalsPane : VisualElement
    {
        private const string AllFindingsText = "everything reported here";
        private const string CopyLabel = "Copy as instructions";

        private const string CopyMessage = "The instruction block is on the clipboard. Paste it back "
            + "through Update dismissals in the graph window, or hand it to an agent to edit.";

        private const string CopyTitle = "Dismissals";
        private const string CountFormat = "{0} dismissed";
        private const string CountWithStaleFormat = "{0} dismissed, {1} no longer match anything";

        private const string EmptyText = "Nothing is dismissed. Anything you dismiss in the graph window "
            + "shows up here, and can be brought back one entry at a time.";

        private const string MissingGroupFormat = "The thing it pointed at is gone ({0})";

        private const string MissingGroupText = "An id embeds the signature it was written for, so these "
            + "stopped matching when something was renamed, retyped or deleted. That is dead "
            + "configuration and can go, once you are satisfied it was a rename rather than a mistake.";

        private const string RemoveAllStaleLabel = "Remove all stale";
        private const string RemoveLabel = "Remove";
        private const string ResolvedGroupFormat = "The finding no longer fires ({0})";

        private const string ResolvedGroupText = "These still point at real code, but the thing they were "
            + "silencing is no longer being reported. Usually that means you fixed it. Occasionally it "
            + "means a rule stopped catching something it used to catch, and this is the only place that "
            + "would ever show.";

        private const string RestoreAllLabel = "Restore all";
        private const string RestoreAllTitle = "Restore every dismissal";
        private const string RestoreLabel = "Restore";
        private const string RestoreTreeLabel = "Restore with contents";

        private const string RestoreTreeTooltip = "Also brings back everything dismissed inside this one, "
            + "which is the exact reverse of dismissing with contents.";

        private const string ScanFirstText = "Nothing has been scanned yet, so it cannot be told which of "
            + "these still match something. Scan in the graph window to find out.";

        private const string ScopeAlone = "this entry only";
        private const string ScopeWithContents = "with everything inside";
        private const string SearchPlaceholder = "Search";
        private const string SuggestionFormat = "looks like it became {0}";
        private const string UpdateLabel = "Update";

        private const string UpdateTooltip = "Points the dismissal at the member that most likely replaced "
            + "it, keeping the decision rather than starting again.";

        private readonly List<DismissalEntry> _entries = new();

        private readonly ScrollView _list;
        private readonly Label _countLabel;
        private Button _restoreAllButton;
        private int _rowCount;
        private string _search = string.Empty;

        /// <summary>Builds the pane, its toolbar and its list.</summary>
        public CodebaseGraphDismissalsPane()
        {
            AddToClassList(CodebaseGraphStyle.PaneClass);
            Add(BuildToolbar());

            _countLabel = new Label(string.Empty);
            _countLabel.AddToClassList(CodebaseGraphStyle.PaneHeadingClass);
            Add(_countLabel);

            _list = new ScrollView
            {
                style =
                {
                    flexGrow = 1f
                }
            };

            Add(_list);

            DismissalStore.Changed += Rebuild;
            RegisterCallback<DetachFromPanelEvent>(_ => DismissalStore.Changed -= Rebuild);

            Rebuild();
        }

        /// <summary>Rereads the dismissals, after a scan has changed what counts as stale.</summary>
        internal void Refresh() => Rebuild();

        private static string BuildScopeText(DismissalEntry entry)
        {
            string scope = entry.IncludesContents
                ? ScopeWithContents
                : ScopeAlone;

            string finding = entry.Finding == EFinding.None
                ? AllFindingsText
                : FindingCatalog.Describe(entry.Finding).Title;

            string text = $"{finding}   \u00b7   {scope}";

            return string.IsNullOrEmpty(entry.SuggestedId)
                ? text
                : $"{text}   {string.Format(SuggestionFormat, entry.SuggestedId)}";
        }

        private static void CopyInstructions()
        {
            EditorGUIUtility.systemCopyBuffer = DismissalTextFormat.Write();
            EditorUtility.DisplayDialog(CopyTitle, CopyMessage, "OK");
        }

        private VisualElement BuildToolbar()
        {
            Toolbar toolbar = new();
            toolbar.AddToClassList(CodebaseGraphStyle.TopBarClass);

            _restoreAllButton = new ToolbarButton(RestoreAll)
            {
                text = RestoreAllLabel
            };

            toolbar.Add(_restoreAllButton);

            toolbar.Add(new ToolbarButton(CopyInstructions)
            {
                text = CopyLabel
            });

            ToolbarSearchField search = new()
            {
                tooltip = SearchPlaceholder
            };

            search.RegisterValueChangedCallback(OnSearchChanged);
            toolbar.Add(search);

            return toolbar;
        }

        private void Rebuild()
        {
            CodebaseGraphData graph = CodebaseGraphCache.Get();

            _entries.Clear();
            _entries.AddRange(DismissalStore.Collect());
            DismissalAudit.Apply(graph, _entries);

            int stale = DismissalAudit.CountStale(_entries);
            _countLabel.text = stale == 0
                ? string.Format(CountFormat, _entries.Count)
                : string.Format(CountWithStaleFormat, _entries.Count, stale);

            _restoreAllButton.SetEnabled(_entries.Count > 0);

            _rowCount = 0;
            _list.Clear();

            if (_entries.Count == 0)
            {
                _list.Add(GraphLabel.Build(EmptyText, CodebaseGraphStyle.PanePlaceholderClass));
                return;
            }

            if (graph == null)
                _list.Add(GraphLabel.Build(ScanFirstText, CodebaseGraphStyle.PanePlaceholderClass));

            AppendStale(EStaleReason.Missing, MissingGroupFormat, MissingGroupText);
            AppendStale(EStaleReason.Resolved, ResolvedGroupFormat, ResolvedGroupText);
            AppendLive();
        }

        private void AppendStale(EStaleReason reason, string headingFormat, string explanation)
        {
            int count = DismissalAudit.Count(_entries, reason);
            if (count == 0)
                return;

            _list.Add(GraphLabel.Build(string.Format(headingFormat, count), CodebaseGraphStyle.SectionTitleClass));
            _list.Add(GraphLabel.Build(explanation, CodebaseGraphStyle.PanePlaceholderClass));
            _list.Add(new Button(() => RemoveStale(reason))
            {
                text = RemoveAllStaleLabel
            });

            foreach (DismissalEntry entry in _entries)
            {
                if (entry.StaleReason == reason && IsMatch(entry))
                    _list.Add(BuildRow(entry));
            }
        }

        private void AppendLive()
        {
            EDismissalKind? lastKind = null;

            foreach (DismissalEntry entry in _entries)
            {
                if (entry.IsStale || !IsMatch(entry))
                    continue;

                if (lastKind != entry.Kind)
                {
                    _list.Add(GraphLabel.Build(entry.Kind.ToString(), CodebaseGraphStyle.SectionTitleClass));
                    lastKind = entry.Kind;
                }

                _list.Add(BuildRow(entry));
            }
        }

        private void RemoveStale(EStaleReason reason)
        {
            List<string> doomed = new();

            foreach (DismissalEntry entry in _entries)
            {
                if (entry.StaleReason == reason)
                    doomed.Add(entry.Id);
            }

            // Collected first, because restoring raises the change event, which rebuilds this very list.
            DismissalStore.RestoreMany(doomed);
            Rebuild();
        }

        private void UpdateToSuggestion(DismissalEntry entry)
        {
            DismissalStore.Restore(entry.Id);
            DismissalStore.Dismiss(entry.SuggestedId, entry.IncludesContents);
            Rebuild();
        }

        private bool IsMatch(DismissalEntry entry) => string.IsNullOrEmpty(_search)
            || entry.DisplayName.IndexOf(_search, StringComparison.OrdinalIgnoreCase) >= 0;

        private VisualElement BuildRow(DismissalEntry entry)
        {
            VisualElement row = new();
            row.AddToClassList(CodebaseGraphStyle.DismissalRowClass);

            // Counted rather than taken from an index, because headings and explanations sit between
            // the rows and would otherwise flip the stripe halfway down a group.
            row.EnableInClassList(CodebaseGraphStyle.IsOddClass, _rowCount % 2 == 1);
            _rowCount++;
            row.EnableInClassList(CodebaseGraphStyle.DismissalStaleClass, entry.IsStale);

            VisualElement text = new();
            text.AddToClassList(CodebaseGraphStyle.DismissalTextClass);

            text.Add(GraphLabel.Build(entry.DisplayName, CodebaseGraphStyle.DismissalNameClass));

            text.Add(GraphLabel.Build(BuildScopeText(entry), CodebaseGraphStyle.DismissalScopeClass));
            row.Add(text);

            if (entry.IsStale)
            {
                AppendStaleButtons(row, entry);
                return row;
            }

            row.Add(new Button(() => Restore(entry))
            {
                text = RestoreLabel
            });

            if (entry.CanHoldContents)
                row.Add(new Button(() => RestoreTree(entry))
                {
                    text = RestoreTreeLabel,
                    tooltip = RestoreTreeTooltip
                });

            return row;
        }

        private void AppendStaleButtons(VisualElement row, DismissalEntry entry)
        {
            if (!string.IsNullOrEmpty(entry.SuggestedId))
                row.Add(new Button(() => UpdateToSuggestion(entry))
                {
                    text = UpdateLabel,
                    tooltip = UpdateTooltip
                });

            row.Add(new Button(() => Restore(entry))
            {
                text = RemoveLabel
            });
        }

        private void Restore(DismissalEntry entry)
        {
            DismissalStore.Restore(entry.Id);
            Rebuild();
        }

        private void RestoreTree(DismissalEntry entry)
        {
            DismissalStore.RestoreWithContents(entry.Id);
            Rebuild();
        }

        private void RestoreAll()
        {
            bool confirmed = EditorUtility.DisplayDialog(RestoreAllTitle,
                $"Bring back all {_entries.Count} dismissed entries?",
                "Restore",
                "Cancel");

            if (!confirmed)
                return;

            DismissalStore.RestoreAll();
            Rebuild();
        }

        private void OnSearchChanged(ChangeEvent<string> evt)
        {
            _search = evt.newValue ?? string.Empty;
            Rebuild();
        }
    }
}