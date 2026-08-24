using System.Collections.Generic;
using Base.CorePackage.StateMachine;
using UnityEngine;
using UnityEngine.UIElements;

namespace Base.CorePackage.Editor.StateMachine
{
    /// <summary>
    /// Draws a machine: one box per state, one curve per transition, and a highlight on the state it is in
    /// right now.
    /// <para>
    /// Boxes and labels are ordinary elements styled from USS, and only the curves are painted, so the
    /// drawing needs no graph framework and nothing here can be edited. The canvas sizes itself to the
    /// layout and is meant to sit inside a scroll view.
    /// </para>
    /// </summary>
    internal sealed class StateMachineCanvas : VisualElement
    {
        private const float ArrowHalfWidth = 4.5f;
        private const float ArrowLength = 10f;
        private const float BackwardBulge = 46f;
        private const string EdgeKeyFormat = "{0}|{1}|{2}";
        private const float EdgeWidth = 1.6f;
        private const float LabelOffset = 9f;
        private const float MinimumReach = 46f;
        private const float PortInset = 12f;
        private const float SelfLoopHeight = 42f;
        private const float SelfLoopSpread = 30f;
        private const string AnyStateLabel = "Any State";

        private static readonly CustomStyleProperty<Color> EdgeColorProperty = new("--sm-edge");
        private static readonly CustomStyleProperty<Color> EdgeActiveColorProperty = new("--sm-edge-active");

        private readonly Dictionary<string, StateMachineNodeView> _nodes = new();
        private readonly Dictionary<string, Label> _edgeLabels = new();
        private readonly List<StateMachineCanvasEdge> _edges = new();

        private Color _edgeColor = Color.gray;
        private Color _edgeActiveColor = Color.green;
        private string _activeEdgeKey = string.Empty;

        /// <summary>Builds the canvas.</summary>
        internal StateMachineCanvas()
        {
            AddToClassList(StateMachineStyle.CanvasClass);

            style.position = Position.Relative;

            generateVisualContent += OnGenerateVisualContent;

            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        /// <summary>Throws the drawing away and builds it again from the given machine.</summary>
        /// <param name="machine">The machine to draw, or null to clear the canvas.</param>
        internal void Show(IStateMachineInfo machine)
        {
            Clear();

            _nodes.Clear();
            _edgeLabels.Clear();
            _edges.Clear();
            _activeEdgeKey = string.Empty;

            if (machine == null || machine.StateNames.Count == 0)
            {
                Resize(Vector2.zero);
                MarkDirtyRepaint();

                return;
            }

            bool hasAnyStateEdges = HasAnyStateEdges(machine);
            Dictionary<string, Vector2> placements = StateMachineLayout.Calculate(machine, hasAnyStateEdges);

            foreach (string stateName in machine.StateNames)
            {
                if (placements.TryGetValue(stateName, out Vector2 position))
                    AddNode(stateName, position, stateName == machine.InitialStateName);
            }

            Rect anyStateArea = hasAnyStateEdges
                ? AddAnyStateNode()
                : Rect.zero;

            foreach (StateMachineEdge edge in machine.Edges)
                AddEdge(edge, anyStateArea);

            Resize(MeasureExtents());
            MarkDirtyRepaint();
        }

        /// <summary>Moves the highlight to wherever the machine is now.</summary>
        /// <param name="machine">The machine being watched, or null to clear the highlight.</param>
        internal void UpdateLive(IStateMachineInfo machine)
        {
            string current = machine == null
                ? string.Empty
                : machine.CurrentStateName;

            foreach (KeyValuePair<string, StateMachineNodeView> pair in _nodes)
                pair.Value.SetActive(pair.Key == current);

            string key = machine == null
                ? string.Empty
                : BuildKey(machine.PreviousStateName, current, machine.LastTransitionName);

            if (key == _activeEdgeKey)
                return;

            SetLabelActive(_activeEdgeKey, false);

            _activeEdgeKey = _edgeLabels.ContainsKey(key)
                ? key
                : string.Empty;

            SetLabelActive(_activeEdgeKey, true);

            MarkDirtyRepaint();
        }

        private static string BuildKey(string from, string to, string name)
            => string.Format(EdgeKeyFormat, from, to, name);

        private static bool HasAnyStateEdges(IStateMachineInfo machine)
        {
            foreach (StateMachineEdge edge in machine.Edges)
            {
                if (edge.IsFromAnyState)
                    return true;
            }

            return false;
        }

        private static void DrawArrow(Painter2D painter, StateMachineCanvasEdge edge, Color color)
        {
            Vector2 direction = edge.End - edge.ControlB;

            if (direction.sqrMagnitude < Mathf.Epsilon)
                return;

            direction.Normalize();

            Vector2 normal = new(-direction.y, direction.x);
            Vector2 baseline = edge.End - direction * ArrowLength;

            painter.fillColor = color;

            painter.BeginPath();
            painter.MoveTo(edge.End);
            painter.LineTo(baseline + normal * ArrowHalfWidth);
            painter.LineTo(baseline - normal * ArrowHalfWidth);
            painter.ClosePath();
            painter.Fill();
        }

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            if (evt.customStyle.TryGetValue(EdgeColorProperty, out Color edge))
                _edgeColor = edge;

            if (evt.customStyle.TryGetValue(EdgeActiveColorProperty, out Color active))
                _edgeActiveColor = active;

            MarkDirtyRepaint();
        }

        private void OnGenerateVisualContent(MeshGenerationContext context)
        {
            Painter2D painter = context.painter2D;

            painter.lineWidth = EdgeWidth;
            painter.lineCap = LineCap.Round;
            painter.lineJoin = LineJoin.Round;

            foreach (StateMachineCanvasEdge edge in _edges)
            {
                bool isActive = edge.Key == _activeEdgeKey;

                Color color = isActive
                    ? _edgeActiveColor
                    : _edgeColor;

                painter.strokeColor = color;

                painter.BeginPath();
                painter.MoveTo(edge.Start);
                painter.BezierCurveTo(edge.ControlA, edge.ControlB, edge.End);
                painter.Stroke();

                DrawArrow(painter, edge, color);
            }
        }

        private void AddNode(string stateName, Vector2 position, bool isInitial)
        {
            StateMachineNodeView node = new(stateName, position, isInitial);

            _nodes[stateName] = node;

            Add(node);
        }

        private Rect AddAnyStateNode()
        {
            StateMachineNodeView node = new(AnyStateLabel, StateMachineLayout.AnyStatePosition, false);

            node.AddToClassList(StateMachineStyle.AnyNodeClass);

            Add(node);

            return node.Area;
        }

        private void AddEdge(StateMachineEdge edge, Rect anyStateArea)
        {
            if (!_nodes.TryGetValue(edge.To, out StateMachineNodeView target))
                return;

            Rect source = edge.IsFromAnyState
                ? anyStateArea
                : GetArea(edge.From);

            if (source == Rect.zero)
                return;

            StateMachineCanvasEdge drawn = source == target.Area
                ? BuildSelfLoop(edge, source)
                : BuildCurve(edge, source, target.Area);

            _edges.Add(drawn);

            AddEdgeLabel(drawn, edge.Name);
        }

        private Rect GetArea(string stateName) => _nodes.TryGetValue(stateName, out StateMachineNodeView node)
            ? node.Area
            : Rect.zero;

        private StateMachineCanvasEdge BuildCurve(StateMachineEdge edge, Rect source, Rect target)
        {
            bool goesForward = target.center.x >= source.center.x;

            Vector2 start = new(goesForward
                ? source.xMax
                : source.xMin, source.center.y);

            Vector2 end = new(goesForward
                ? target.xMin
                : target.xMax, target.center.y);

            float reach = Mathf.Max(MinimumReach, Mathf.Abs(end.x - start.x) * 0.5f);
            float direction = goesForward
                ? 1f
                : -1f;

            // A transition pointing back the way the machine came would otherwise lie on top of the one
            // going the other way, so it is bowed downwards to keep both readable.
            float bulge = goesForward
                ? 0f
                : BackwardBulge;

            Vector2 controlA = new(start.x + reach * direction, start.y + bulge);
            Vector2 controlB = new(end.x - reach * direction, end.y + bulge);

            return new StateMachineCanvasEdge(BuildKey(edge.From, edge.To, edge.Name), start, controlA, controlB,
                end);
        }

        private StateMachineCanvasEdge BuildSelfLoop(StateMachineEdge edge, Rect area)
        {
            Vector2 start = new(area.xMax - PortInset, area.yMin);
            Vector2 end = new(area.xMin + PortInset, area.yMin);

            Vector2 controlA = new(start.x + SelfLoopSpread, start.y - SelfLoopHeight);
            Vector2 controlB = new(end.x - SelfLoopSpread, end.y - SelfLoopHeight);

            return new StateMachineCanvasEdge(BuildKey(edge.From, edge.To, edge.Name), start, controlA, controlB,
                end);
        }

        private void AddEdgeLabel(StateMachineCanvasEdge edge, string text)
        {
            if (string.IsNullOrEmpty(text) || _edgeLabels.ContainsKey(edge.Key))
                return;

            Vector2 midpoint = edge.Midpoint();

            Label label = new(text);

            label.AddToClassList(StateMachineStyle.EdgeLabelClass);

            label.style.position = Position.Absolute;
            label.style.left = midpoint.x;
            label.style.top = midpoint.y - LabelOffset;

            _edgeLabels[edge.Key] = label;

            Add(label);
        }

        private void SetLabelActive(string key, bool isActive)
        {
            if (string.IsNullOrEmpty(key) || !_edgeLabels.TryGetValue(key, out Label label))
                return;

            label.EnableInClassList(StateMachineStyle.EdgeLabelActiveClass, isActive);
        }

        private Vector2 MeasureExtents()
        {
            Vector2 extents = Vector2.zero;

            foreach (KeyValuePair<string, StateMachineNodeView> pair in _nodes)
            {
                extents.x = Mathf.Max(extents.x, pair.Value.Area.xMax);
                extents.y = Mathf.Max(extents.y, pair.Value.Area.yMax);
            }

            return extents + Vector2.one * StateMachineLayout.Padding;
        }

        private void Resize(Vector2 extents)
        {
            style.width = extents.x;
            style.height = extents.y;
        }
    }
}