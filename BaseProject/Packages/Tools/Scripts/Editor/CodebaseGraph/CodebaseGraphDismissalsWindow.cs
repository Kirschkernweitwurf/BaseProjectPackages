using System;
using System.Collections.Generic;
using Base.ToolPackage.Editor.CodebaseGraph.Analysis;
using Base.ToolPackage.Editor.CodebaseGraph.Model;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Base.ToolPackage.Editor.CodebaseGraph
{
    /// <summary>
    /// Lists everything that was dismissed during triage. Dismissing is easy to do in passing, which is
    /// exactly why it needs somewhere to be looked at afterwards: without this the only record is a JSON
    /// file, and a decision you cannot review is a decision you stop trusting.
    /// </summary>
    public sealed class CodebaseGraphDismissalsWindow : EditorWindow
    {
        private const string AllFindingsText = "everything reported here";
        private const string CopyLabel = "Copy as instructions";

        private const string CopyMessage = "The instruction block is on the clipboard. Paste it back "
            + "through Update dismissals in the graph window, or hand it to an agent to edit.";

        private const string CountFormat = "{0} dismissed";
        private const string CountWithStaleFormat = "{0} dismissed, {1} no longer match anything";

        private const string EmptyText = "Nothing is dismissed. Anything you dismiss in the graph window "
            + "shows up here, and can be brought back one entry at a time.";

        private const string HeadingClass = "pane-heading";
        private const string KindTitleClass = "section-title";

        private const int MinimumWindowHeight = 320;
        private const int MinimumWindowWidth = 620;
        private const string MissingGroupFormat = "The thing it pointed at is gone ({0})";

        private const string MissingGroupText = "An id embeds the signature it was written for, so these "
            + "stopped matching when something was renamed, retyped or deleted. That is dead "
            + "configuration and can go, once you are satisfied it was a rename rather than a mistake.";

        private const string PaneClass = "pane";
        private const string PlaceholderClass = "pane-placeholder";
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

        private const string RootClass = "codebase-graph-root";
        private const string RowClass = "dismissal-row";
        private const string RowNameClass = "dismissal-name";
        private const string RowScopeClass = "dismissal-scope";

        private const string ScanFirstText = "Nothing has been scanned yet, so it cannot be told which of "
            + "these still match something. Scan in the graph window to find out.";

        private const string ScopeAlone = "this entry only";
        private const string ScopeWithContents = "with everything inside";
        private const string SearchPlaceholder = "Search";
        private const string StaleRowClass = "dismissal-stale";
        private const string SuggestionFormat = "looks like it became {0}";
        private const string ToolbarRowClass = "top-bar";
        private const string UpdateLabel = "Update";

        private const string UpdateTooltip = "Points the dismissal at the member that most likely replaced "
            + "it, keeping the decision rather than starting again.";

        private const string WindowTitle = "Dismissed findings";

        private readonly List<DismissalEntry> _entries = new();

        private ScrollView _list;
        private Label _countLabel;
        private Button _restoreAllButton;
        private string _search = string.Empty;

#region Unity Callbacks
        private void CreateGUI()
        {
            rootVisualElement.AddToClassList(RootClass);
            CodebaseGraphStyle.Apply(rootVisualElement);

            rootVisualElement.Add(BuildToolbar());

            _countLabel = new Label(string.Empty);
            _countLabel.AddToClassList(HeadingClass);
            rootVisualElement.Add(_countLabel);

            _list = new ScrollView();
            _list.AddToClassList(PaneClass);
            rootVisualElement.Add(_list);

            DismissalStore.Changed += Rebuild;
            Rebuild();
        }

        private void OnDisable() => DismissalStore.Changed -= Rebuild;
#endregion

        /// <summary>
        /// Closes the window if it is open. It shows the state of one graph window and offers actions
        /// that only make sense beside it, so leaving it behind would strand a panel with no owner.
        /// </summary>
        public static void CloseIfOpen()
        {
            foreach (CodebaseGraphDismissalsWindow window
                in Resources.FindObjectsOfTypeAll<CodebaseGraphDismissalsWindow>())
                window.Close();
        }

        /// <summary>Opens the window.</summary>
        public static void Open()
        {
            CodebaseGraphDismissalsWindow window = GetWindow<CodebaseGraphDismissalsWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(MinimumWindowWidth, MinimumWindowHeight);
        }

        private static Label BuildLabel(string text, string styleClass)
        {
            Label label = new(text);
            label.AddToClassList(styleClass);
            return label;
        }

        private VisualElement BuildToolbar()
        {
            Toolbar toolbar = new();
            toolbar.AddToClassList(ToolbarRowClass);

            _restoreAllButton = new ToolbarButton(RestoreAll) { text = RestoreAllLabel };
            toolbar.Add(_restoreAllButton);

            toolbar.Add(new ToolbarButton(CopyInstructions) { text = CopyLabel });

            ToolbarSearchField search = new();
            search.tooltip = SearchPlaceholder;
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
            _list.Clear();

            if (_entries.Count == 0)
            {
                _list.Add(BuildLabel(EmptyText, PlaceholderClass));
                return;
            }

            if (graph == null)
                _list.Add(BuildLabel(ScanFirstText, PlaceholderClass));

            AppendStale(EStaleReason.Missing, MissingGroupFormat, MissingGroupText);
            AppendStale(EStaleReason.Resolved, ResolvedGroupFormat, ResolvedGroupText);
            AppendLive();
        }

        private void AppendStale(EStaleReason reason, string headingFormat, string explanation)
        {
            int count = DismissalAudit.Count(_entries, reason);
            if (count == 0)
                return;

            _list.Add(BuildLabel(string.Format(headingFormat, count), KindTitleClass));
            _list.Add(BuildLabel(explanation, PlaceholderClass));
            _list.Add(new Button(() => RemoveStale(reason)) { text = RemoveAllStaleLabel });

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
                    _list.Add(BuildLabel(entry.Kind.ToString(), KindTitleClass));
                    lastKind = entry.Kind;
                }

                _list.Add(BuildRow(entry));
            }
        }

        private void RemoveStale(EStaleReason reason)
        {
            foreach (DismissalEntry entry in _entries)
            {
                if (entry.StaleReason == reason)
                    DismissalStore.Restore(entry.Id);
            }

            Rebuild();
        }

        private void UpdateToSuggestion(DismissalEntry entry)
        {
            DismissalStore.Restore(entry.Id);
            DismissalStore.Dismiss(entry.SuggestedId, entry.IncludesContents);
            Rebuild();
        }

        private bool IsMatch(DismissalEntry entry)
            => string.IsNullOrEmpty(_search)
                || entry.DisplayName.IndexOf(_search, StringComparison.OrdinalIgnoreCase) >= 0;

        private VisualElement BuildRow(DismissalEntry entry)
        {
            VisualElement row = new();
            row.AddToClassList(RowClass);
            row.EnableInClassList(StaleRowClass, entry.IsStale);

            VisualElement text = new();
            text.style.flexGrow = 1f;
            text.Add(BuildLabel(entry.DisplayName, RowNameClass));

            text.Add(BuildLabel(BuildScopeText(entry), RowScopeClass));
            row.Add(text);

            if (entry.IsStale)
            {
                AppendStaleButtons(row, entry);
                return row;
            }

            row.Add(new Button(() => Restore(entry)) { text = RestoreLabel });

            if (entry.CanHoldContents)
            {
                row.Add(new Button(() => RestoreTree(entry))
                {
                    text = RestoreTreeLabel,
                    tooltip = RestoreTreeTooltip
                });
            }

            return row;
        }

        private string BuildScopeText(DismissalEntry entry)
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

        private void AppendStaleButtons(VisualElement row, DismissalEntry entry)
        {
            if (!string.IsNullOrEmpty(entry.SuggestedId))
            {
                row.Add(new Button(() => UpdateToSuggestion(entry))
                {
                    text = UpdateLabel,
                    tooltip = UpdateTooltip
                });
            }

            row.Add(new Button(() => Restore(entry)) { text = RemoveLabel });
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

        private void CopyInstructions()
        {
            EditorGUIUtility.systemCopyBuffer = DismissalTextFormat.Write();
            EditorUtility.DisplayDialog(WindowTitle, CopyMessage, "OK");
        }

        private void OnSearchChanged(ChangeEvent<string> evt)
        {
            _search = evt.newValue ?? string.Empty;
            Rebuild();
        }

    }
}
