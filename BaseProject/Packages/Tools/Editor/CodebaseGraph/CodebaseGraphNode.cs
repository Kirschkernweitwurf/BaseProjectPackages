using System;
using Base.ToolPackage.Editor.CodebaseGraph.Analysis;
using Base.ToolPackage.Editor.CodebaseGraph.Model;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Base.ToolPackage.Editor.CodebaseGraph
{
    /// <summary>
    /// A graph node showing one entry. Each level is drawn with its own width, corner radius and glyph.
    /// So a namespace, a class and a field are told apart by shape rather than by reading them, and a
    /// type lists its members the way a class does rather than only counting them.
    /// <br/><br/>
    /// Actions live in the right click menu and on double click rather than on buttons.
    /// Because a button inside a GraphView node competes with the node's own drag
    /// and selection manipulators and swallows about half of the clicks aimed at it.
    /// </summary>
    internal sealed class CodebaseGraphNode : Node
    {
        private const string AccentClass = "node-accent";
        private const string BadgeClass = "finding-badge";
        private const string BadgeDismissedClass = "is-dismissed";
        private const string BadgeRowClass = "finding-row";
        private const string ContractClass = "is-contract";
        private const string DismissCommand = "Dismiss findings here";
        private const string DismissedBadgeText = "Dismissed, findings silenced";
        private const string DismissedInsideFormat = "{0} dismissed inside";
        private const string DismissedNodeClass = "has-dismissals";
        private const string DismissedRowTooltip = "This member has a finding that was dismissed.";

        private const string DismissedTooltip = "Findings here were reviewed and dismissed, so they are "
            + "silenced everywhere including the report. The Dismissed button in the toolbar lists them "
            + "and brings them back.";

        private const string DismissTreeCommand = "Dismiss findings here and inside";
        private const string DrillCommand = "Open contents";
        private const string FindingsClass = "has-findings";
        private const string FocusCommand = "Focus on this";
        private const string FocusedClass = "is-focused";
        private const string GlyphClass = "node-glyph";
        private const string MemberLevelClass = "level-member";
        private const string MetaClass = "node-meta";
        private const string NamespaceLevelClass = "level-namespace";
        private const string NestedFormat = "{0} more inside";
        private const string NodeClass = "codebase-node";
        private const string OpenCommand = "Open script";
        private const string OverflowFormat = "and {0} more members";
        private const string RowClass = "member-row";
        private const string RowDismissedClass = "is-dismissed";
        private const string RowFindingClass = "has-finding";
        private const string RowGlyphClass = "member-glyph";
        private const string RowLabelClass = "member-label";
        private const string RowListClass = "member-list";
        private const string SubtitleClass = "node-subtitle";
        private const string TitleLabelName = "title-label";
        private const string TypeLevelClass = "level-type";

        /// <summary>The entry this node stands for.</summary>
        public GraphEntry Entry { get; }

        /// <summary>Port other nodes connect into.</summary>
        public Port InputPort { get; }

        /// <summary>Port this node connects out of.</summary>
        public Port OutputPort { get; }

        private static readonly Color BodyColor = new(0.20f, 0.20f, 0.22f, 1f);

        private readonly Action<GraphEntry> _onFocus;
        private readonly Action<GraphEntry> _onOpen;
        private readonly Action<GraphEntry> _onDrillDown;
        private readonly Action<GraphEntry> _onSelect;
        private readonly Action<GraphEntry, bool> _onDismiss;

        /// <summary>Builds the node for one entry.</summary>
        /// <param name="entry">Entry to show.</param>
        /// <param name="isFocused">Whether this node is the current focus.</param>
        /// <param name="onSelect">Raised on a single click, to show the entry in the detail pane.</param>
        /// <param name="onFocus">Raised when the view should center on this entry.</param>
        /// <param name="onDrillDown">Raised when the next level down should open.</param>
        /// <param name="onOpen">Raised when the script should be opened.</param>
        /// <param name="onDismiss">Raised when the findings here should be dismissed.</param>
        public CodebaseGraphNode(GraphEntry entry,
            bool isFocused,
            Action<GraphEntry> onSelect,
            Action<GraphEntry> onFocus,
            Action<GraphEntry> onDrillDown,
            Action<GraphEntry> onOpen,
            Action<GraphEntry, bool> onDismiss)
        {
            Entry = entry;
            _onSelect = onSelect;
            _onFocus = onFocus;
            _onDrillDown = onDrillDown;
            _onOpen = onOpen;
            _onDismiss = onDismiss;

            title = entry.Title;
            tooltip = BuildTooltip(entry);

            AddToClassList(NodeClass);
            AddToClassList(ResolveLevelClass(entry.Level));
            style.width = CodebaseGraphLayout.MeasureWidth(entry);

            if (entry.IsContract)
                AddToClassList(ContractClass);

            if (entry.HasOpenFindings)
                AddToClassList(FindingsClass);

            if (entry.HasDismissals)
                AddToClassList(DismissedNodeClass);

            if (isFocused)
                AddToClassList(FocusedClass);

            ApplyColors();
            BuildGlyph();

            InputPort = CreatePort(Direction.Input);
            inputContainer.Add(InputPort);

            OutputPort = CreatePort(Direction.Output);
            outputContainer.Add(OutputPort);

            BuildBody();
            RegisterCallback<MouseDownEvent>(OnMouseDown);

            RefreshExpandedState();
            RefreshPorts();
        }

        /// <inheritdoc/>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction(FocusCommand, action: _ => _onFocus?.Invoke(Entry));

            if (Entry.CanDrillDown)
                evt.menu.AppendAction(DrillCommand, action: _ => _onDrillDown?.Invoke(Entry));

            if (Entry.Type != null)
                evt.menu.AppendAction(OpenCommand, action: _ => _onOpen?.Invoke(Entry));

            if (!Entry.HasOpenFindings)
                return;

            evt.menu.AppendAction(DismissCommand, action: _ => _onDismiss?.Invoke(Entry, false));

            if (Entry.CanDrillDown)
                evt.menu.AppendAction(DismissTreeCommand, action: _ => _onDismiss?.Invoke(Entry, true));
        }

        private static string BuildTooltip(GraphEntry entry) => entry.HasDismissals
            ? $"{entry.Subtitle}\n\n{DismissedTooltip}"
            : entry.Subtitle;

        private static string ResolveLevelClass(EGraphScope level)
        {
            switch (level)
            {
                case EGraphScope.Namespace:
                    return NamespaceLevelClass;

                case EGraphScope.Member:
                    return MemberLevelClass;

                default:
                    return TypeLevelClass;
            }
        }

        private static VisualElement BuildRow(GraphMemberRow row)
        {
            VisualElement element = new();
            element.AddToClassList(RowClass);
            element.EnableInClassList(RowFindingClass, row.HasFinding);
            element.EnableInClassList(RowDismissedClass, row.IsDismissed);

            if (row.IsDismissed)
                element.tooltip = DismissedRowTooltip;

            Label glyph = GraphLabel.Build(row.Glyph, RowGlyphClass);
            glyph.style.color = GraphSymbols.GetColor(row.Access);
            element.Add(glyph);

            Label label = GraphLabel.Build(row.Label, RowLabelClass);
            label.style.color = GraphSymbols.GetColor(row.Access);
            element.Add(label);

            return element;
        }

        private static Label BuildDismissedBadge(string text)
        {
            Label badge = GraphLabel.Build(text, BadgeClass);
            badge.AddToClassList(BadgeDismissedClass);
            badge.tooltip = DismissedTooltip;

            return badge;
        }

        private Port CreatePort(Direction direction)
        {
            Port port = InstantiatePort(Orientation.Horizontal, direction, Port.Capacity.Multi, typeof(bool));
            port.portName = string.Empty;
            return port;
        }

        private void ApplyColors()
        {
            mainContainer.style.backgroundColor = BodyColor;
            extensionContainer.style.backgroundColor = BodyColor;
            titleContainer.style.backgroundColor = GraphColorPalette.GetColor(Entry.ColorSeed);
            titleContainer.Q<Label>(TitleLabelName).style.color = GraphColorPalette.TitleTextColor;

            VisualElement accent = new();
            accent.AddToClassList(AccentClass);
            accent.style.backgroundColor = GraphSymbols.GetColor(Entry.Access);
            mainContainer.Insert(0, accent);
        }

        private void BuildGlyph()
        {
            Label glyph = GraphLabel.Build(Entry.Glyph, GlyphClass);
            glyph.style.color = GraphSymbols.GetColor(Entry.Access);
            titleContainer.Insert(0, glyph);
        }

        private void BuildBody()
        {
            string meta = $"Used by {Entry.FanIn}   \u00b7   Uses {Entry.FanOut}";

            extensionContainer.Add(GraphLabel.Build(Entry.Subtitle, SubtitleClass));
            extensionContainer.Add(GraphLabel.Build(meta, MetaClass));

            BuildRows();
            BuildBadges();
        }

        private void BuildRows()
        {
            if (Entry.Rows.Count == 0)
                return;

            VisualElement list = new();
            list.AddToClassList(RowListClass);

            foreach (GraphMemberRow row in Entry.Rows)
                list.Add(BuildRow(row));

            if (Entry.HiddenRowCount > 0)
                list.Add(GraphLabel.Build(string.Format(OverflowFormat, Entry.HiddenRowCount), SubtitleClass));

            extensionContainer.Add(list);
        }

        private void BuildBadges()
        {
            if (Entry.BadgeCount == 0)
                return;

            VisualElement badges = new();
            badges.AddToClassList(BadgeRowClass);

            foreach (EFinding finding in Entry.Findings)
                badges.Add(GraphLabel.Build(FindingCatalog.Describe(finding).Title, BadgeClass));

            if (Entry.NestedFindingCount > 0)
                badges.Add(GraphLabel.Build(string.Format(NestedFormat, Entry.NestedFindingCount), BadgeClass));

            if (Entry.IsDismissed)
                badges.Add(BuildDismissedBadge(DismissedBadgeText));

            if (Entry.DismissedNestedCount > 0)
                badges.Add(BuildDismissedBadge(string.Format(DismissedInsideFormat,
                    Entry.DismissedNestedCount)));

            extensionContainer.Add(badges);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.button != 0)
                return;

            if (evt.clickCount >= 2)
            {
                Activate();
                evt.StopPropagation();
                return;
            }

            _onSelect?.Invoke(Entry);
        }

        private void Activate()
        {
            if (Entry.CanDrillDown)
            {
                _onDrillDown?.Invoke(Entry);
                return;
            }

            _onOpen?.Invoke(Entry);
        }
    }
}