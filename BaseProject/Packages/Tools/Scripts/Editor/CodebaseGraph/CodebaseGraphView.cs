using System;
using System.Collections.Generic;
using Base.ToolPackage.Editor.CodebaseGraph.Model;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Base.ToolPackage.Editor.CodebaseGraph
{
    /// <summary>
    /// Renders the current entries as draggable nodes with usage edges. A dense graph is unreadable if
    /// every line is drawn the same weight at the same time. So lines are as heavy as the traffic they
    /// carry and selecting a node pushes everything it does not touch into the background.
    /// </summary>
    internal sealed class CodebaseGraphView : GraphView
    {
        private const float DimmedAlpha = 0.10f;
        private const float FadedAlpha = 0.07f;
        private const long FrameDelayMilliseconds = 60;
        private const float FullOpacity = 1f;
        private const int HeavyWeight = 12;
        private const int MediumWeight = 4;
        private const int MediumWidth = 2;
        private const float MinimapBottomOffset = 48f;
        private const float MinimapHeight = 150f;
        private const float MinimapMargin = 14f;
        private const float MinimapWidth = 220f;
        private const float MinimumZoom = 0.08f;
        private const float RestingAlpha = 0.55f;
        private const int ThickWidth = 4;
        private const int ThinWidth = 1;
        private const int WideWidth = 5;

        private static readonly Color IncomingColor = new(0.42f, 0.62f, 0.84f);
        private static readonly Color OutgoingColor = new(0.84f, 0.70f, 0.42f);
        private static readonly Color RestingColor = new(0.62f, 0.62f, 0.66f);
        private static readonly Color SelectedColor = Color.white;

        private readonly Action<GraphEntry> _onSelect;
        private readonly Action<GraphEntry> _onFocus;
        private readonly Action<GraphEntry> _onDrillDown;
        private readonly Action<GraphEntry> _onOpen;
        private readonly Action<GraphEntry, bool> _onDismiss;
        private readonly List<CodebaseGraphEdge> _edges = new();

        private CodebaseGraphNode _focusedNode;
        private CodebaseGraphMiniMap _miniMap;
        private EEdgeMode _edgeMode = EEdgeMode.Muted;
        private ELayoutMode _layoutMode = ELayoutMode.Dependencies;

        /// <summary>Builds the view and wires the actions its nodes raise.</summary>
        /// <param name="onSelect">Raised on a single click on a node.</param>
        /// <param name="onFocus">Raised when the view should center on a node.</param>
        /// <param name="onDrillDown">Raised when the next level down should open.</param>
        /// <param name="onOpen">Raised when a script should be opened.</param>
        /// <param name="onDismiss">Raised when the findings on a node should be dismissed.</param>
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

            BuildMiniMap();
        }

        /// <inheritdoc/>
        public override void AddToSelection(ISelectable selectable)
        {
            base.AddToSelection(selectable);
            RefreshEdges();
        }

        /// <inheritdoc/>
        public override void RemoveFromSelection(ISelectable selectable)
        {
            base.RemoveFromSelection(selectable);
            RefreshEdges();
        }

        /// <inheritdoc/>
        public override void ClearSelection()
        {
            base.ClearSelection();
            RefreshEdges();
        }

        /// <summary>Sets how the graph arranges what it draws.</summary>
        /// <param name="mode">The arrangement to use.</param>
        public void SetLayoutMode(ELayoutMode mode) => _layoutMode = mode;

        /// <summary>Sets how many relation lines are drawn at once.</summary>
        /// <param name="mode">The mode to draw in.</param>
        public void SetEdgeMode(EEdgeMode mode)
        {
            _edgeMode = mode;
            RefreshEdges();
        }

        /// <summary>Clears and rebuilds the graph from the given entries.</summary>
        /// <param name="entries">Entries to draw.</param>
        /// <param name="focusedId">ID of the focused entry, or null.</param>
        public void Rebuild(IReadOnlyList<GraphEntry> entries, string focusedId)
        {
            DeleteElements(graphElements.ToList());
            _focusedNode = null;
            _edges.Clear();

            if (entries.Count == 0)
            {
                _miniMap.SetContent(entries, new Dictionary<string, Rect>());
                return;
            }

            Dictionary<string, Rect> placements = CodebaseGraphLayout.Calculate(entries, _layoutMode);
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

            RefreshEdges();
            _miniMap.SetContent(entries, placements);

            // Node positions only become real once the panel has laid out, so framing waits a beat.
            schedule.Execute(FrameContent).ExecuteLater(FrameDelayMilliseconds);
        }

        /// <summary>Shows or hides the minimap.</summary>
        /// <param name="isVisible">Whether the minimap should be drawn.</param>
        public void SetMiniMapVisible(bool isVisible) => _miniMap.style.display = isVisible
            ? DisplayStyle.Flex
            : DisplayStyle.None;

        private static Color Fade(Color color, float alpha) => new(color.r, color.g, color.b, alpha);

        private static int ResolveWidth(int weight, bool isTouched)
        {
            if (isTouched)
                return WideWidth;

            if (weight >= HeavyWeight)
                return ThickWidth;

            return weight >= MediumWeight
                ? MediumWidth
                : ThinWidth;
        }

        private void BuildMiniMap()
        {
            _miniMap = new CodebaseGraphMiniMap(this)
            {
                style =
                {
                    width = MinimapWidth,
                    height = MinimapHeight
                }
            };

            Add(_miniMap);
            RegisterCallback<GeometryChangedEvent>(_ => PlaceMiniMap());
            PlaceMiniMap();

            viewTransformChanged += _ => _miniMap.Refresh();
        }

        private void PlaceMiniMap()
        {
            _miniMap.style.left = Mathf.Max(MinimapMargin, layout.width - MinimapWidth - MinimapMargin);
            _miniMap.style.top = Mathf.Max(MinimapMargin,
                layout.height - MinimapHeight - MinimapBottomOffset);

            _miniMap.Refresh();
        }

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

            foreach (GraphEdgeInfo target in entry.Targets)
            {
                if (target.TargetId == entry.Id || !drawn.Add(target.TargetId))
                    continue;

                if (!byId.TryGetValue(target.TargetId, out CodebaseGraphNode node))
                    continue;

                CodebaseGraphEdge edge = source.OutputPort.ConnectTo<CodebaseGraphEdge>(node.InputPort);
                edge.SourceId = entry.Id;
                edge.TargetId = target.TargetId;
                edge.Weight = target.Weight;

                AddElement(edge);
                _edges.Add(edge);
            }
        }

        /// <summary>
        /// Restyles every line for the current selection. Without this a graph of any size is a wall of
        /// identical curves, and the one relation being looked at is indistinguishable from the rest.
        /// </summary>
        private void RefreshEdges()
        {
            HashSet<string> selected = CollectSelectedIds();
            _miniMap.SetSelection(selected);

            foreach (CodebaseGraphEdge edge in _edges)
                ApplyEdgeStyle(edge, selected);
        }

        private HashSet<string> CollectSelectedIds()
        {
            HashSet<string> ids = new();

            foreach (ISelectable selectable in selection)
            {
                if (selectable is CodebaseGraphNode node)
                    ids.Add(node.Entry.Id);
            }

            return ids;
        }

        private void ApplyEdgeStyle(CodebaseGraphEdge edge, HashSet<string> selected)
        {
            bool isOutgoing = selected.Contains(edge.SourceId);
            bool isIncoming = selected.Contains(edge.TargetId);
            bool isTouched = isOutgoing || isIncoming;

            edge.style.display = IsVisible(isTouched, selected.Count)
                ? DisplayStyle.Flex
                : DisplayStyle.None;

            edge.Restyle(ResolveColor(isOutgoing, isIncoming, selected.Count),
                ResolveWidth(edge.Weight, isTouched),
                ResolveOpacity(isTouched, selected.Count));
        }

        private Color ResolveColor(bool isOutgoing, bool isIncoming, int selectedCount)
        {
            bool isTouched = isOutgoing || isIncoming;

            // Muted draws every line at the edge of visibility and lifts only what is selected to white.
            // It is the default because it answers the one question a line exists to answer, what does
            // this connect to, without every other line competing to answer it at the same time.
            if (_edgeMode == EEdgeMode.Muted)
                return isTouched
                    ? SelectedColor
                    : Fade(RestingColor, FadedAlpha);

            if (selectedCount == 0)
                return Fade(RestingColor, RestingAlpha);

            if (isOutgoing)
                return OutgoingColor;

            return isIncoming
                ? IncomingColor
                : Fade(RestingColor, DimmedAlpha);
        }

        /// <summary>
        /// How visible a line is. This is what actually carries the emphasis, because opacity is a plain
        /// element style rather than something the graph recomputes for itself on the next redraw.
        /// </summary>
        private float ResolveOpacity(bool isTouched, int selectedCount)
        {
            if (_edgeMode == EEdgeMode.Muted)
                return isTouched
                    ? FullOpacity
                    : FadedAlpha;

            if (selectedCount == 0)
                return RestingAlpha;

            return isTouched
                ? FullOpacity
                : DimmedAlpha;
        }

        private bool IsVisible(bool isTouched, int selectedCount)
        {
            switch (_edgeMode)
            {
                case EEdgeMode.None:
                    return false;

                case EEdgeMode.SelectedOnly:
                    return selectedCount > 0 && isTouched;

                default:
                    return true;
            }
        }
    }
}