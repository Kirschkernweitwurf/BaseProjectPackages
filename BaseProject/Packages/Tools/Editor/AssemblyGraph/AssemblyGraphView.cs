using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Base.ToolsPackage.Editor.AssemblyGraph
{
    /// <summary>Renders assemblies as draggable nodes with reference edges.</summary>
    internal sealed class AssemblyGraphView : GraphView
    {
        private const string CandidateEdgeClass = "candidate-edge";

        private static readonly Color CandidateEdgeColor = new(0.90f, 0.32f, 0.32f);

        private readonly Action<AssemblyNodeInfo> _onFocusRequested;
        private readonly Action<AssemblyNodeInfo> _onCleanupRequested;

        /// <summary>Builds the graph view and wires the callbacks its nodes raise.</summary>
        public AssemblyGraphView(Action<AssemblyNodeInfo> onFocusRequested, Action<AssemblyNodeInfo> onCleanupRequested)
        {
            _onFocusRequested = onFocusRequested;
            _onCleanupRequested = onCleanupRequested;

            style.flexGrow = 1f;
            SetupZoom(0.08f, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            GridBackground grid = new();
            Insert(0, grid);
            grid.StretchToParentSize();
        }

        /// <summary>The graph is read only, so manual port connections are disabled.</summary>
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) => new();

        /// <summary>Clears and rebuilds the graph from the given visible nodes.</summary>
        internal void Rebuild(IReadOnlyList<AssemblyNodeInfo> visibleNodes, string focusedName)
        {
            DeleteElements(graphElements.ToList());
            if (visibleNodes.Count == 0)
                return;

            Dictionary<string, Rect> placements = AssemblyGraphLayout.Calculate(visibleNodes);
            Dictionary<string, AssemblyGraphNode> byName = new();

            foreach (AssemblyNodeInfo info in visibleNodes)
            {
                bool isFocused = info.Name == focusedName;
                AssemblyGraphNode node = new(info, isFocused, _onFocusRequested, _onCleanupRequested);
                node.SetPosition(placements[info.Name]);
                AddElement(node);
                byName[info.Name] = node;
            }

            foreach (AssemblyNodeInfo info in visibleNodes)
            {
                AssemblyGraphNode source = byName[info.Name];

                foreach (AssemblyReferenceInfo reference in info.References)
                {
                    if (reference.TargetName == info.Name)
                        continue;

                    if (!byName.TryGetValue(reference.TargetName, out AssemblyGraphNode target))
                        continue;

                    AddEdge(source, target, reference.IsCandidate);
                }
            }

            schedule.Execute(() => FrameAll()).ExecuteLater(50);
        }

        private void AddEdge(AssemblyGraphNode source, AssemblyGraphNode target, bool isCandidate)
        {
            Edge edge = source.OutputPort.ConnectTo(target.InputPort);

            if (isCandidate)
            {
                edge.AddToClassList(CandidateEdgeClass);
                edge.edgeControl.inputColor = CandidateEdgeColor;
                edge.edgeControl.outputColor = CandidateEdgeColor;
            }

            AddElement(edge);
        }
    }
}