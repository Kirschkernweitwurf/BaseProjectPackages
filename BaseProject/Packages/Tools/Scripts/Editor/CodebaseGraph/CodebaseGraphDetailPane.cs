using System;
using System.Collections.Generic;
using Base.ToolPackage.Editor.CodebaseGraph.Analysis;
using Base.ToolPackage.Editor.CodebaseGraph.Model;
using UnityEngine.UIElements;

namespace Base.ToolPackage.Editor.CodebaseGraph
{
    /// <summary>
    /// Explains whatever is selected. A red badge on a node only says that something was found, so this
    /// pane says what the scan actually saw, what to do about it, and whether the window can do it.
    /// </summary>
    public sealed class CodebaseGraphDetailPane : VisualElement
    {
        private const string ActionClass = "finding-action";
        private const string ActionRowClass = "action-row";
        private const string ActionTitleClass = "finding-action-title";
        private const string ActionTitleText = "What to do";
        private const string CardClass = "finding-card";
        private const string CardTitleClass = "finding-card-title";
        private const string CutHintFormat = "Cheapest edge to cut: {0}";
        private const string CycleTitleText = "The others caught in this same loop";
        private const string DismissClass = "dismiss-button";
        private const string DismissedNoticeClass = "dismissed-notice";

        private const string DismissedNoticeText = "You dismissed this. It is hidden from the report. "
            + "The code has not changed.";

        private const string DismissFindingLabel = "Dismiss this";

        private const string DismissFindingTooltip = "Hides this one finding. Anything else found here "
            + "still shows.";

        private const string DismissLabel = "Dismiss everything here";

        private const string DismissTooltip = "Hides everything found here, now and in future scans. "
            + "Usually you want the button on the single finding instead.";

        private const string DismissTreeLabel = "Dismiss with contents";
        private const string DrillLabel = "Open contents";

        private const string EmptyText = "Click something in the list or the graph to see what it is and "
            + "what was found on it.";

        private const string EntryPointFormat = "Reached from outside the code: {0}. That is why it is not "
            + "reported as unused.";

        private const string ExplanationClass = "finding-explanation";
        private const string FixLabel = "Apply fix";
        private const string FocusLabel = "Show neighbors";

        private const string FocusTooltip = "Hides everything except this entry and what it connects to, "
            + "so you can read one dependency at a time. Use the Neighbors dropdown to widen the reach.";

        private const string HeadingClass = "pane-heading";
        private const int MaxRelations = 12;

        private const string MetricsFormat = "Abstractness {0:0.00}   \u00b7   Instability {1:0.00}   "
            + "\u00b7   Distance from the main sequence {2:0.00}";

        private const string MetricsTitleText = "Shape numbers";
        private const string MoreFormat = "and {0} more";
        private const string OpenLabel = "Open script";
        private const string PaneClass = "pane";
        private const string PartnerClass = "finding-partner";
        private const string PlaceholderClass = "pane-placeholder";
        private const string RelationClass = "relation-entry";
        private const string RepeatFormat = "  x{0}";
        private const string RestoreLabel = "Bring back";
        private const string SectionTitleClass = "section-title";
        private const string SubtitleClass = "pane-subtitle";
        private const string ToolbarRowClass = "action-row";
        private const string UsedByTitle = "Used by";
        private const string UsesTitle = "Uses";

        private readonly Action<GraphEntry> _onFocus;
        private readonly Action<GraphEntry> _onDrillDown;
        private readonly Action<GraphEntry> _onOpen;
        private readonly Action<GraphEntry, EFinding> _onQuickFix;
        private readonly Action<GraphEntry, bool> _onDismiss;
        private readonly Action<GraphEntry, EFinding> _onDismissFinding;
        private readonly Action<GraphEntry> _onRestore;
        private readonly ScrollView _content;

        /// <summary>Builds an empty detail pane.</summary>
        /// <param name="onFocus">Raised when the graph should center on the entry.</param>
        /// <param name="onDrillDown">Raised when the next level down should open.</param>
        /// <param name="onOpen">Raised when the script should be opened.</param>
        /// <param name="onQuickFix">Raised when a fixable finding should be applied.</param>
        /// <param name="onDismiss">Raised when every finding here should be dismissed.</param>
        /// <param name="onDismissFinding">Raised when one finding here should be dismissed.</param>
        /// <param name="onRestore">Raised when a dismissed entry should be brought back.</param>
        public CodebaseGraphDetailPane(Action<GraphEntry> onFocus,
            Action<GraphEntry> onDrillDown,
            Action<GraphEntry> onOpen,
            Action<GraphEntry, EFinding> onQuickFix,
            Action<GraphEntry, bool> onDismiss,
            Action<GraphEntry, EFinding> onDismissFinding,
            Action<GraphEntry> onRestore)
        {
            _onFocus = onFocus;
            _onDrillDown = onDrillDown;
            _onOpen = onOpen;
            _onQuickFix = onQuickFix;
            _onDismiss = onDismiss;
            _onDismissFinding = onDismissFinding;
            _onRestore = onRestore;

            AddToClassList(PaneClass);

            _content = new ScrollView();
            _content.style.flexGrow = 1f;
            Add(_content);

            ShowPlaceholder();
        }

        /// <summary>Shows everything known about one entry.</summary>
        /// <param name="entry">Entry to describe.</param>
        /// <param name="graph">Graph the entry came from, used to name its relations.</param>
        public void Show(GraphEntry entry, CodebaseGraphData graph)
        {
            _content.Clear();

            if (entry == null)
            {
                ShowPlaceholder();
                return;
            }

            _content.Add(BuildLabel(entry.Title, HeadingClass));
            _content.Add(BuildLabel(entry.Subtitle, SubtitleClass));
            AppendEntryPointNote(entry);

            if (entry.IsDismissed)
                _content.Add(BuildLabel(DismissedNoticeText, DismissedNoticeClass));

            _content.Add(BuildActionRow(entry));

            foreach (EFinding finding in entry.Findings)
                _content.Add(BuildFindingCard(entry, finding));

            AppendMetrics(entry);
            BuildRelations(entry, graph);
        }

        private static Label BuildLabel(string text, string styleClass)
        {
            Label label = new(text);
            label.AddToClassList(styleClass);
            return label;
        }

        private static void AppendRelations(VisualElement parent, string title, List<string> names)
        {
            parent.Add(BuildLabel($"{title} ({names.Count})", SectionTitleClass));

            int shown = names.Count < MaxRelations
                ? names.Count
                : MaxRelations;

            for (int index = 0; index < shown; index++)
                parent.Add(BuildLabel(names[index], RelationClass));

            if (names.Count > shown)
                parent.Add(BuildLabel(string.Format(MoreFormat, names.Count - shown), RelationClass));
        }

        private static List<string> CollectNamespaceRelations(IEnumerable<string> names)
        {
            List<string> result = new();
            foreach (string name in names)
                result.Add(name);

            result.Sort(StringComparer.OrdinalIgnoreCase);
            return result;
        }

        private static List<string> CollectTypeRelations(IEnumerable<TypeKey> keys, CodebaseGraphData graph)
        {
            List<string> result = new();

            foreach (TypeKey key in keys)
            {
                TypeNodeInfo type = graph.FindType(key);
                if (type != null)
                    result.Add(type.FullName);
            }

            result.Sort(StringComparer.OrdinalIgnoreCase);
            return result;
        }

        private static List<string> CollectMemberRelations(IEnumerable<UsageEdgeInfo> edges,
            CodebaseGraphData graph,
            bool useSource)
        {
            List<string> result = new();

            foreach (UsageEdgeInfo edge in edges)
            {
                MemberNodeInfo member = graph.FindMember(useSource
                    ? edge.SourceKey
                    : edge.TargetKey);

                if (member == null)
                    continue;

                TypeNodeInfo declaring = graph.FindType(member.DeclaringTypeKey);
                string owner = declaring == null
                    ? string.Empty
                    : $"{declaring.ShortName}.";

                string repeats = edge.Count > 1
                    ? string.Format(RepeatFormat, edge.Count)
                    : string.Empty;

                result.Add($"{owner}{member.Name}   ({edge.Kind}){repeats}");
            }

            result.Sort(StringComparer.OrdinalIgnoreCase);
            return result;
        }

        private void ShowPlaceholder() => _content.Add(BuildLabel(EmptyText, PlaceholderClass));

        private void AppendMetrics(GraphEntry entry)
        {
            if (entry.Member != null || entry.Type == null)
                return;

            _content.Add(BuildLabel(MetricsTitleText, SectionTitleClass));
            _content.Add(BuildLabel(string.Format(MetricsFormat,
                    entry.Type.Abstractness,
                    entry.Type.Instability,
                    entry.Type.MainSequenceDistance),
                RelationClass));
        }

        private void AppendEntryPointNote(GraphEntry entry)
        {
            string reason = ResolveEntryPointReason(entry);
            if (string.IsNullOrEmpty(reason))
                return;

            _content.Add(BuildLabel(string.Format(EntryPointFormat, reason), SubtitleClass));
        }

        private string ResolveEntryPointReason(GraphEntry entry)
        {
            if (entry.Member != null && entry.Member.IsEntryPoint)
                return entry.Member.EntryPointReason;

            return entry.Member == null && entry.Type != null && entry.Type.IsEntryPoint
                ? entry.Type.EntryPointReason
                : null;
        }

        private VisualElement BuildActionRow(GraphEntry entry)
        {
            VisualElement row = new();
            row.AddToClassList(ToolbarRowClass);

            Button focusButton = new(() => _onFocus?.Invoke(entry))
            {
                text = FocusLabel,
                tooltip = FocusTooltip
            };

            row.Add(focusButton);

            if (entry.CanDrillDown)
                row.Add(new Button(() => _onDrillDown?.Invoke(entry)) { text = DrillLabel });

            if (entry.Type != null)
                row.Add(new Button(() => _onOpen?.Invoke(entry)) { text = OpenLabel });

            AppendDismissButtons(row, entry);
            return row;
        }

        private void AppendDismissButtons(VisualElement row, GraphEntry entry)
        {
            if (entry.IsDismissed)
            {
                Button restore = new(() => _onRestore?.Invoke(entry)) { text = RestoreLabel };
                restore.AddToClassList(DismissClass);
                row.Add(restore);

                return;
            }

            if (!entry.HasOpenFindings)
                return;

            Button dismiss = new(() => _onDismiss?.Invoke(entry, false))
            {
                text = DismissLabel,
                tooltip = DismissTooltip
            };

            dismiss.AddToClassList(DismissClass);
            row.Add(dismiss);

            if (!entry.CanDrillDown)
                return;

            Button dismissTree = new(() => _onDismiss?.Invoke(entry, true))
            {
                text = DismissTreeLabel,
                tooltip = DismissTooltip
            };

            dismissTree.AddToClassList(DismissClass);
            row.Add(dismissTree);
        }

        private VisualElement BuildFindingCard(GraphEntry entry, EFinding finding)
        {
            FindingDescriptor descriptor = FindingCatalog.Describe(finding);

            VisualElement card = new();
            card.AddToClassList(CardClass);

            card.Add(BuildLabel(descriptor.Title, CardTitleClass));
            card.Add(BuildLabel(descriptor.Explanation, ExplanationClass));

            if (!string.IsNullOrEmpty(descriptor.Action))
            {
                card.Add(BuildLabel(ActionTitleText, ActionTitleClass));
                card.Add(BuildLabel(descriptor.Action, ActionClass));
            }

            AppendCyclePartners(card, entry, finding);

            VisualElement actions = new();
            actions.AddToClassList(ActionRowClass);

            if (descriptor.CanQuickFix && entry.Member != null)
                actions.Add(new Button(() => _onQuickFix?.Invoke(entry, finding)) { text = FixLabel });

            // One finding at a time. Dismissing the whole entry silences findings nobody has looked at,
            // including ones the next scan has not raised yet.
            Button dismiss = new(() => _onDismissFinding?.Invoke(entry, finding))
            {
                text = DismissFindingLabel,
                tooltip = DismissFindingTooltip
            };

            dismiss.AddToClassList(DismissClass);
            actions.Add(dismiss);
            card.Add(actions);

            return card;
        }

        private void AppendCyclePartners(VisualElement card, GraphEntry entry, EFinding finding)
        {
            List<string> partners = null;

            if (finding == EFinding.TypeCycle && entry.Type != null)
                partners = entry.Type.CyclePartners;

            if (finding == EFinding.NamespaceCycle && entry.Namespace != null)
                partners = entry.Namespace.CyclePartners;

            if (partners == null || partners.Count == 0)
                return;

            string cut = finding == EFinding.TypeCycle
                ? entry.Type?.CycleCutHint
                : entry.Namespace?.CycleCutHint;

            if (!string.IsNullOrEmpty(cut))
                card.Add(BuildLabel(string.Format(CutHintFormat, cut), ActionClass));

            card.Add(BuildLabel(CycleTitleText, ActionTitleClass));

            foreach (string partner in partners)
                card.Add(BuildLabel($"\u2022  {partner}", PartnerClass));
        }

        private void BuildRelations(GraphEntry entry, CodebaseGraphData graph)
        {
            if (entry.Member != null)
            {
                AppendRelations(_content, UsedByTitle, CollectMemberRelations(entry.Member.Incoming, graph, true));
                AppendRelations(_content, UsesTitle, CollectMemberRelations(entry.Member.Outgoing, graph, false));
                return;
            }

            if (entry.Namespace != null)
            {
                AppendRelations(_content, UsedByTitle, CollectNamespaceRelations(entry.Namespace.Incoming.Keys));
                AppendRelations(_content, UsesTitle, CollectNamespaceRelations(entry.Namespace.Outgoing.Keys));
                return;
            }

            if (entry.Type == null)
                return;

            AppendRelations(_content, UsedByTitle, CollectTypeRelations(entry.Type.Incoming.Keys, graph));
            AppendRelations(_content, UsesTitle, CollectTypeRelations(entry.Type.Outgoing.Keys, graph));
        }
    }
}
