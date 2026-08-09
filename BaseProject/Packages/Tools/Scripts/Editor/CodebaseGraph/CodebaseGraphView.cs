using System;
using System.Collections.Generic;
using System.Linq;
using Base.ToolPackage.Editor.CodebaseGraph.Model;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Base.ToolPackage.Editor.CodebaseGraph
{
    /// <summary>Renders the current entries as draggable nodes with usage edges.</summary>
    public sealed class CodebaseGraphView : GraphView
    {
        private const long FrameDelayMilliseconds = 60;
        private const float MinimumZoom = 0.08f;

        private static readonly Color FindingEdgeColor = new(0.88f, 0.36f, 0.36f);

        private readonly Action<GraphEntry> _onSelect;
        private readonly Action<GraphEntry> _onFocus;
        private readonly Action<GraphEntry> _onDrillDown;
        private readonly Action<GraphEntry> _onOpen;
        private readonly Action<GraphEntry, bool> _onDismiss;

        private CodebaseGraphNode _focusedNode;

        /// <summary>Builds the view and wires the actions its nodes raise.</summary>
        /// <param name="onSelect">Raised on a single click on a node.</param>
        /// <param name="onFocus">Raised when the view should center on a node.</param>
        /// <param name="onDrillDown">Raised when the next level down should open.</param>
        /// <param name="onOpen">Raised when a script should be opened.</param>
        /// <param name="onDismiss">Raised when the findings on a node should be set aside.</param>
        public CodebaseGraphView(Action<GraphEntry> onSelect,
            Action<GraphEntry> onFocus,
            Action<GraphEntry> onDrillDown,
            Action<GraphEntry> onOpen,
            Action<GraphEntry, bool> onDismiss)
        {
            _onSelect = onSelect;
            _onFocus = onFocus;
            _onDrillDown = onDrillDown;
            _onOpen = onOpen;
            _onDismiss = onDismiss;

            style.flexGrow = 1f;
            SetupZoom(MinimumZoom, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            GridBackground grid = new();
            Insert(0, grid);
            grid.StretchToParentSize();
        }

        /// <summary>Clears and rebuilds the graph from the given entries.</summary>
        /// <param name="entries">Entries to draw.</param>
        /// <param name="focusedId">Id of the focused entry, or null.</param>
        public void Rebuild(IReadOnlyList<GraphEntry> entries, string focusedId)
        {
            DeleteElements(graphElements.ToList());
            _focusedNode = null;

            if (entries.Count == 0)
                return;

            Dictionary<string, Rect> placements = CodebaseGraphLayout.Calculate(entries);
            Dictionary<string, CodebaseGraphNode> byId = new();

            foreach (GraphEntry entry in entries)
            {
                if (!placements.TryGetValue(entry.Id, out Rect placement))
                    continue;

                CodebaseGraphNode node = new(entry,
                    entry.Id == focusedId,
                    _onSelect,
                    _onFocus,
                    _onDrillDown,
                    _onOpen,
                    _onDismiss);

                node.SetPosition(placement);
                AddElement(node);
                byId[entry.Id] = node;

                if (entry.Id == focusedId)
                    _focusedNode = node;
            }

            foreach (GraphEntry entry in entries)
                Connect(entry, byId);

            // Node positions only become real once the panel has laid out, so framing waits a beat.
            schedule.Execute(FrameContent).ExecuteLater(FrameDelayMilliseconds);
        }

        /// <summary>Moves the viewport onto the content, centered on the focused node when there is one.</summary>
        private void FrameContent()
        {
            if (_focusedNode == null)
            {
                FrameAll();
                return;
            }

            ClearSelection();
            AddToSelection(_focusedNode);
            FrameSelection();
        }

        private void Connect(GraphEntry entry, Dictionary<string, CodebaseGraphNode> byId)
        {
            if (!byId.TryGetValue(entry.Id, out CodebaseGraphNode source))
                return;

            HashSet<string> drawn = new();

            foreach (string targetId in entry.TargetIds)
            {
                if (targetId == entry.Id || !drawn.Add(targetId))
                    continue;

                if (!byId.TryGetValue(targetId, out CodebaseGraphNode target))
                    continue;

                Edge edge = source.OutputPort.ConnectTo(target.InputPort);

                if (entry.HasFindings)
                {
                    edge.edgeControl.inputColor = FindingEdgeColor;
                    edge.edgeControl.outputColor = FindingEdgeColor;
                }

                AddElement(edge);
            }
        }
    }
}
