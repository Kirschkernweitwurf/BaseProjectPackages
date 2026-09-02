using System.Collections.Generic;
using System.IO;
using Base.ToolsPackage.Editor.CodebaseGraph.Model;
using UnityEditor;

namespace Base.ToolsPackage.Editor.AssemblyGraph.Architecture
{
    /// <summary>
    /// Writes the assembly edge roll-up to a file so the numbers can be checked before anything is
    /// built on them. The scan itself is not started from here: it belongs to the Codebase Graph, it
    /// keeps a finding baseline that only that window maintains, and a second place that could trigger
    /// it is a second place for the two to disagree.
    /// <para>
    /// That is also why this is reached from the Codebase Graph toolbar rather than from a menu item.
    /// The caller already holds the scan, so there is no state to check and nothing to offer when it
    /// is missing.
    /// </para>
    /// </summary>
    internal static class AssemblyEdgeReportCommand
    {
        private const string DefaultReportName = "AssemblyEdgeRollUp.md";
        private const string ExportExtension = "md";
        private const string ExportTitle = "Save assembly edge report";

        /// <summary>Builds the report from an existing scan and asks where to put it.</summary>
        /// <param name="graph">The scan to roll up. Nothing is written when it is null.</param>
        internal static void Export(CodebaseGraphData graph)
        {
            if (graph == null)
                return;

            string path = EditorUtility.SaveFilePanel(ExportTitle,
                string.Empty,
                DefaultReportName,
                ExportExtension);

            if (string.IsNullOrEmpty(path))
                return;

            AssemblyEdgeGraph edges = AssemblyEdgeRollUp.Build(graph);
            List<AssemblyNodeInfo> nodes = AssemblyGraphModel.Build();

            File.WriteAllText(path, AssemblyEdgeReportWriter.Build(edges, nodes));
            EditorUtility.RevealInFinder(path);
        }
    }
}