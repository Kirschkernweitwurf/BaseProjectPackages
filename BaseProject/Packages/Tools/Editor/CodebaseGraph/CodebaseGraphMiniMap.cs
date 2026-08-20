using System.Collections.Generic;
using Base.ToolPackage.Editor.CodebaseGraph.Model;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Base.ToolPackage.Editor.CodebaseGraph
{
    /// <summary>
    /// An overview of the whole graph with a box showing what is currently on screen. Written rather
    /// than taken from the graph library, because that one clamps anything outside its bounds to the
    /// border instead of leaving it out. So a node that scrolls off the edge appears to slide across
    /// the map and pile up on top of its neighbors. Here a node is drawn where it is or not at all,
    /// and the view box is the only thing allowed to be clipped.
    /// <br/><br/>
    /// The view box follows a poll rather than an event. Dragging the canvas writes the transform
    /// straight onto the container without raising anything, so a map that waits to be told only ever
    /// catches up once the drag has finished.
    /// </summary>
    internal sealed class CodebaseGraphMiniMap : VisualElement
    {
        private const float ContentPadding = 24f;
        private const string DotClass = "minimap-dot";
        private const string FindingDotClass = "has-finding";
        private const long FollowIntervalMilliseconds = 16;
        private const float MapPadding = 8f;
        private const string MiniMapClass = "minimap";
        private const float MinimumDotSize = 2f;
        private const string SelectedDotClass = "is-selected";
        private const string ViewBoxClass = "minimap-view";

        private readonly Dictionary<string, Rect> _placements = new();
        private readonly VisualElement _dots;
        private readonly VisualElement _viewBox;
        private readonly GraphView _graphView;

        private readonly HashSet<string> _selected = new();

        private Rect _content;
        private float _scale = 1f;
        private Vector2 _offset;
        private Vector3 _lastPosition;
        private Vector3 _lastScale;

        /// <summary>Builds the map for a graph.</summary>
        /// <param name="graphView">Graph whose contents and viewport are mirrored.</param>
        public CodebaseGraphMiniMap(GraphView graphView)
        {
            _graphView = graphView;

            AddToClassList(MiniMapClass);

            _dots = new VisualElement();
            _dots.StretchToParentSize();
            _dots.pickingMode = PickingMode.Ignore;
            Add(_dots);

            _viewBox = new VisualElement();
            _viewBox.AddToClassList(ViewBoxClass);
            _viewBox.pickingMode = PickingMode.Ignore;
            Add(_viewBox);

            RegisterCallback<GeometryChangedEvent>(_ => Redraw());
            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<MouseMoveEvent>(OnMouseMove);

            schedule.Execute(FollowTransform).Every(FollowIntervalMilliseconds);
        }

        /// <summary>Replaces what the map shows.</summary>
        /// <param name="entries">Entries currently drawn in the graph.</param>
        /// <param name="placements">Where each of them sits, keyed by entry id.</param>
        public void SetContent(IReadOnlyList<GraphEntry> entries, Dictionary<string, Rect> placements)
        {
            _dots.Clear();
            _placements.Clear();
            _content = Rect.zero;

            bool hasAny = false;

            foreach (GraphEntry entry in entries)
            {
                if (!placements.TryGetValue(entry.Id, out Rect placement))
                    continue;

                _content = hasAny
                    ? Union(_content, placement)
                    : placement;

                hasAny = true;
                _placements[entry.Id] = placement;
                _dots.Add(BuildDot(entry));
            }

            if (hasAny)
                _content = Grow(_content, ContentPadding);

            ApplySelection();
            Redraw();
        }

        /// <summary>Redraws the view box after the graph has been panned or zoomed.</summary>
        public void Refresh() => PlaceViewBox();

        /// <summary>Marks which entries are selected, so the map shows where the selection sits.</summary>
        /// <param name="ids">Ids of the selected entries.</param>
        public void SetSelection(IEnumerable<string> ids)
        {
            _selected.Clear();

            foreach (string id in ids)
                _selected.Add(id);

            ApplySelection();
        }

        private static Rect Union(Rect first, Rect second)
        {
            float minX = Mathf.Min(first.xMin, second.xMin);
            float minY = Mathf.Min(first.yMin, second.yMin);
            float maxX = Mathf.Max(first.xMax, second.xMax);
            float maxY = Mathf.Max(first.yMax, second.yMax);

            return Rect.MinMaxRect(minX, minY, maxX, maxY);
        }

        private static Rect Grow(Rect rect, float amount) => new(rect.x - amount, rect.y - amount,
            rect.width + amount * 2f, rect.height + amount * 2f);

        private static VisualElement BuildDot(GraphEntry entry)
        {
            VisualElement dot = new();
            dot.AddToClassList(DotClass);
            dot.EnableInClassList(FindingDotClass, entry.HasOpenFindings);
            dot.style.backgroundColor = GraphColorPalette.GetColor(entry.ColorSeed);
            dot.userData = entry.Id;

            return dot;
        }

        private void ApplySelection()
        {
            foreach (VisualElement dot in _dots.Children())
            {
                if (dot.userData is string id)
                    dot.EnableInClassList(SelectedDotClass, _selected.Contains(id));
            }
        }

        private void FollowTransform()
        {
            ReadViewTransform(out Vector3 position, out Vector3 zoom);

            if (position == _lastPosition && zoom == _lastScale)
                return;

            _lastPosition = position;
            _lastScale = zoom;
            PlaceViewBox();
        }

        private void Redraw()
        {
            if (_content.width <= 0f || _content.height <= 0f || layout.width <= 0f)
                return;

            float usableWidth = Mathf.Max(1f, layout.width - MapPadding * 2f);
            float usableHeight = Mathf.Max(1f, layout.height - MapPadding * 2f);

            _scale = Mathf.Min(usableWidth / _content.width, usableHeight / _content.height);
            _offset = new Vector2(MapPadding + (usableWidth - _content.width * _scale) * 0.5f,
                MapPadding + (usableHeight - _content.height * _scale) * 0.5f);

            PlaceDots();
            PlaceViewBox();
        }

        private void PlaceDots()
        {
            foreach (VisualElement dot in _dots.Children())
            {
                if (dot.userData is not string id)
                    continue;

                if (!_placements.TryGetValue(id, out Rect placement))
                    continue;

                Vector2 position = ToLocal(placement.position);

                dot.style.left = position.x;
                dot.style.top = position.y;
                dot.style.width = Mathf.Max(MinimumDotSize, placement.width * _scale);
                dot.style.height = Mathf.Max(MinimumDotSize, placement.height * _scale);
            }
        }

        private void PlaceViewBox()
        {
            if (_scale <= 0f)
                return;

            ReadViewTransform(out Vector3 position, out Vector3 zoom);

            if (Mathf.Approximately(zoom.x, 0f) || Mathf.Approximately(zoom.y, 0f))
                return;

            Rect rect = new(-position.x / zoom.x,
                -position.y / zoom.y,
                _graphView.layout.width / zoom.x,
                _graphView.layout.height / zoom.y);

            Vector2 corner = ToLocal(rect.position);

            _viewBox.style.left = corner.x;
            _viewBox.style.top = corner.y;
            _viewBox.style.width = rect.width * _scale;
            _viewBox.style.height = rect.height * _scale;
        }

        /// <summary>
        /// Reads where the canvas currently sits. The graph exposes this through a transform the
        /// element layer has since deprecated. The write side of the same pair is not deprecated
        /// and takes the same types, so the read is kept here in one place rather than mixing two
        /// coordinate APIs across five call sites.
        /// </summary>
        private void ReadViewTransform(out Vector3 position, out Vector3 zoom)
        {
#pragma warning disable 618
            position = _graphView.viewTransform.position;
            zoom = _graphView.viewTransform.scale;
#pragma warning restore 618
        }

        private Vector2 ToLocal(Vector2 world) => new((world.x - _content.x) * _scale + _offset.x,
            (world.y - _content.y) * _scale + _offset.y);

        private Vector2 ToWorld(Vector2 local) => new((local.x - _offset.x) / _scale + _content.x,
            (local.y - _offset.y) / _scale + _content.y);

        private void OnMouseDown(MouseDownEvent evt)
        {
            CenterOn(evt.localMousePosition);
            evt.StopPropagation();
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (evt.pressedButtons == 0)
                return;

            CenterOn(evt.localMousePosition);
            evt.StopPropagation();
        }

        private void CenterOn(Vector2 local)
        {
            if (_scale <= 0f)
                return;

            Vector2 world = ToWorld(local);
            ReadViewTransform(out Vector3 _, out Vector3 zoom);

            Vector3 position = new(-world.x * zoom.x + _graphView.layout.width * 0.5f,
                -world.y * zoom.y + _graphView.layout.height * 0.5f,
                0f);

            _graphView.UpdateViewTransform(position, zoom);
            PlaceViewBox();
        }
    }
}