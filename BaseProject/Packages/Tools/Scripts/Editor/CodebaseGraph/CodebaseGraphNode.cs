using System;
using Base.ToolPackage.Editor.CodebaseGraph.Analysis;
using Base.ToolPackage.Editor.CodebaseGraph.Model;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Base.ToolPackage.Editor.CodebaseGraph
{
    /// <summary>
    /// A graph node showing one entry. Actions live in the right click menu and on double click rather
    /// than on buttons, because a button inside a GraphView node competes with the node's own drag and
    /// selection manipulators and swallows about half of the clicks aimed at it.
    /// </summary>
    public sealed class CodebaseGraphNode : Node
    {
        private const string BadgeClass = "finding-badge";
        private const string BadgeRowClass = "finding-row";
        private const string DismissCommand = "Dismiss findings here";
        private const string DismissTreeCommand = "Dismiss findings here and inside";
        private const string DrillCommand = "Open contents";
        private const string FindingsClass = "has-findings";
        private const string FocusCommand = "Focus on this";
        private const string FocusedClass = "is-focused";
        private const string MetaClass = "node-meta";
        private const string NestedFormat = "{0} more inside";
        private const string NodeClass = "codebase-node";
        private const string OpenCommand = "Open script";
        private const string SubtitleClass = "node-subtitle";

        private static readonly Color BodyColor = new(0.20f, 0.20f, 0.22f, 1f);

        /// <summary>The entry this node stands for.</summary>
        public GraphEntry Entry { get; }

        /// <summary>Port other nodes connect into.</summary>
        public Port InputPort { get; }

        /// <summary>Port this node connects out of.</summary>
        public Port OutputPort { get; }

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
        /// <param name="onDismiss">Raised when the findings here should be set aside.</param>
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
            tooltip = entry.Subtitle;
            AddToClassList(NodeClass);
            style.width = CodebaseGraphLayout.NodeWidth;

            if (entry.BadgeCount > 0)
                AddToClassList(FindingsClass);

            if (isFocused)
                AddToClassList(FocusedClass);

            ApplyColors();

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
            evt.menu.AppendAction(FocusCommand, _ => _onFocus?.Invoke(Entry));

            if (Entry.CanDrillDown)
                evt.menu.AppendAction(DrillCommand, _ => _onDrillDown?.Invoke(Entry));

            if (Entry.Type != null)
                evt.menu.AppendAction(OpenCommand, _ => _onOpen?.Invoke(Entry));

            if (Entry.BadgeCount == 0)
                return;

            evt.menu.AppendAction(DismissCommand, _ => _onDismiss?.Invoke(Entry, false));

            if (Entry.CanDrillDown)
                evt.menu.AppendAction(DismissTreeCommand, _ => _onDismiss?.Invoke(Entry, true));
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
        }

        private void BuildBody()
        {
            Label subtitle = new(Entry.Subtitle);
            subtitle.AddToClassList(SubtitleClass);
            extensionContainer.Add(subtitle);

            Label meta = new($"used by {Entry.FanIn}   \u00b7   uses {Entry.FanOut}");
            meta.AddToClassList(MetaClass);
            extensionContainer.Add(meta);

            if (Entry.BadgeCount == 0)
                return;

            VisualElement badges = new();
            badges.AddToClassList(BadgeRowClass);

            foreach (EFinding finding in Entry.Findings)
                badges.Add(BuildBadge(FindingCatalog.Describe(finding).Title));

            if (Entry.NestedFindingCount > 0)
                badges.Add(BuildBadge(string.Format(NestedFormat, Entry.NestedFindingCount)));

            extensionContainer.Add(badges);
        }

        private Label BuildBadge(string text)
        {
            Label badge = new(text);
            badge.AddToClassList(BadgeClass);
            return badge;
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
