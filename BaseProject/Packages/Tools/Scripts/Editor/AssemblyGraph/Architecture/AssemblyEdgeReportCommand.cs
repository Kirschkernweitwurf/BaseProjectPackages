using System.Collections.Generic;
using System.IO;
using Base.ToolPackage.Editor.CodebaseGraph;
using Base.ToolPackage.Editor.CodebaseGraph.Model;
using Base.UtilityPackage.Menus;
using UnityEditor;

namespace Base.ToolPackage.Editor.AssemblyGraph.Architecture
{
    /// <summary>
    /// Writes the assembly edge roll-up to a file so the numbers can be checked before anything is
    /// built on them. The scan itself is not started from here: it belongs to the Codebase Graph, it
    /// keeps a finding baseline that only that window maintains, and a second place that could trigger
    /// it is a second place for the two to disagree.
    /// </summary>
    internal static class AssemblyEdgeReportCommand
    {
        private const string DefaultReportName = "AssemblyEdgeRollUp.md";
        private const string ExportExtension = "md";
        private const string ExportTitle = "Save assembly edge report";
        private const string MenuPath = "Tools/Base Packages/Unity Editor/Project Health/Assembly Edge Report";
        private const string MissingScanCancel = "Cancel";
        private const string MissingScanConfirm = "Open Codebase Graph";

        private const string MissingScanMessage = "The roll-up reads the Codebase Graph scan, and there "
            + "is none in memory. Metadata tokens only survive one compilation, so the scan is dropped "
            + "on every domain reload and has to be run again.";

        private const string MissingScanTitle = "No scan yet";

        /// <summary>Builds the report and asks where to put it.</summary>
        [DynamicMenuItem(MenuPath)]
        public static void Export()
        {
            CodebaseGraphData graph = CodebaseGraphCache.Get();

            if (graph == null)
            {
                OfferScan();
                return;
            }

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

        private static void OfferScan()
        {
            bool confirmed = EditorUtility.DisplayDialog(MissingScanTitle,
                MissingScanMessage,
                MissingScanConfirm,
                MissingScanCancel);

            if (!confirmed)
                return;

            CodebaseGraphWindow.Open();
        }
    }
}