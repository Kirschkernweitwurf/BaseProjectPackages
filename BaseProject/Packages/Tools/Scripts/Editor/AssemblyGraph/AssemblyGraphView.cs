using System;
using System.Collections.Generic;
using Base.AssemblyGraphPackage.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Base.ToolPackage.Editor.AssemblyGraph
{
    /// <summary>Renders assemblies as draggable nodes with reference edges.</summary>
    public sealed class AssemblyGraphView : GraphView
    {
        private const float ColumnWidth = 340f;
        private const float RowHeight = 240f;
        private static readonly Color UnusedEdgeColor = new(0.90f, 0.32f, 0.32f);

        private readonly Action<AssemblyNodeInfo> _onCleanupRequested;

        public AssemblyGraphView(Action<AssemblyNodeInfo> onCleanupRequested)
        {
            _onCleanupRequested = onCleanupRequested;

            style.flexGrow = 1f;
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            GridBackground grid = new();
            Insert(0, grid);
            grid.StretchToParentSize();
        }

        /// <summary>Clears and rebuilds the graph from the given visible nodes.</summary>
        public void Rebuild(IReadOnlyList<AssemblyNodeInfo> visibleNodes)
        {
            DeleteElements(graphElements.ToList());

            HashSet<string> visibleNames = new();
            foreach (AssemblyNodeInfo info in visibleNodes)
                visibleNames.Add(info.Name);

            Dictionary<string, int> levels = ComputeLevels(visibleNodes, visibleNames);
            Dictionary<int, int> rowInLevel = new();
            Dictionary<string, AssemblyGraphNode> byName = new();

            foreach (AssemblyNodeInfo info in visibleNodes)
            {
                AssemblyGraphNode node = new(info, _onCleanupRequested);

                int level = levels[info.Name];
                rowInLevel.TryGetValue(level, out int row);
                rowInLevel[level] = row + 1;

                node.SetPosition(new Rect(level * ColumnWidth, row * RowHeight, 260f, 120f));
                AddElement(node);
                byName[info.Name] = node;
            }

            foreach (AssemblyNodeInfo info in visibleNodes)
            {
                AssemblyGraphNode source = byName[info.Name];
                foreach (AssemblyReferenceInfo reference in info.References)
                {
                    if (!byName.TryGetValue(reference.TargetName, out AssemblyGraphNode target))
                        continue;

                    AddEdge(source, target, reference.IsUnused);
                }
            }
        }

        /// <summary>The graph is read only, so manual port connections are disabled.</summary>
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) => new();

        private static Dictionary<string, int> ComputeLevels(IReadOnlyList<AssemblyNodeInfo> nodes,
            HashSet<string> visibleNames)
        {
            Dictionary<string, AssemblyNodeInfo> map = new();
            foreach (AssemblyNodeInfo info in nodes)
                map[info.Name] = info;

            Dictionary<string, int> levels = new();
            foreach (AssemblyNodeInfo info in nodes)
                ResolveLevel(info, map, visibleNames, levels, new HashSet<string>());

            return levels;
        }

        private static int ResolveLevel(AssemblyNodeInfo info,
            Dictionary<string, AssemblyNodeInfo> map,
            HashSet<string> visibleNames,
            Dictionary<string, int> levels,
            HashSet<string> guard)
        {
            if (levels.TryGetValue(info.Name, out int cached))
                return cached;

            if (!guard.Add(info.Name))
                return 0;

            int level = 0;
            foreach (AssemblyReferenceInfo reference in info.References)
            {
                if (!visibleNames.Contains(reference.TargetName))
                    continue;

                if (!map.TryGetValue(reference.TargetName, out AssemblyNodeInfo target))
                    continue;

                level = Math.Max(level, ResolveLevel(target, map, visibleNames, levels, guard) + 1);
            }

            guard.Remove(info.Name);
            levels[info.Name] = level;
            return level;
        }

        private void AddEdge(AssemblyGraphNode source, AssemblyGraphNode target, bool unused)
        {
            Edge edge = source.OutputPort.ConnectTo(target.InputPort);

            if (unused)
            {
                edge.AddToClassList("unused-edge");
                edge.edgeControl.inputColor = UnusedEdgeColor;
                edge.edgeControl.outputColor = UnusedEdgeColor;
            }

            AddElement(edge);
        }
    }
}