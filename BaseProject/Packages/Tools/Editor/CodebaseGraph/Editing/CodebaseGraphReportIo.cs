using System.IO;
using Base.ToolPackage.Editor.CodebaseGraph.Analysis;
using Base.ToolPackage.Editor.CodebaseGraph.Model;
using UnityEditor;

namespace Base.ToolPackage.Editor.CodebaseGraph.Editing
{
    /// <summary>
    /// Moves findings and dismissals across the window's edge: the report out, and instructions back in.
    /// It lives apart from the window because none of it is about the graph on screen, and a window that
    /// also owns file dialogs and clipboard parsing is doing two jobs.
    /// </summary>
    internal static class CodebaseGraphReportIo
    {
        private const string DefaultReportName = "CodebaseGraphFindings.md";
        private const string ExportExtension = "md";
        private const string ExportTitle = "Save findings report";
        private const string ImportCancel = "Cancel";
        private const string ImportFromClipboard = "Paste from clipboard";
        private const string ImportFromFile = "Load from file";
        private const string ImportLabel = "Update dismissals";

        private const string ImportMessage = "Reads a list of dismissals, the same block the findings "
            + "report writes at the end. One per line:\n\n"
            + "  dismiss <id>\n  dismiss-tree <id>\n  restore <id>\n  restore-tree <id>\n\n"
            + "Anything you leave out stays as it is. Only restore removes a dismissal.";

        private const string ImportOpenTitle = "Open dismissal instructions";

        private const string ImportResultFormat = "Applied {0} lines.\n{1} were ignored: unknown word, "
            + "broken id or already in that state.";

        private const string ScopeSuffix = "-Scope";
        private const string ScopeTitle = "Save scope report";

        /// <summary>Asks where to put the findings report and writes it there.</summary>
        /// <param name="graph">Graph to report on.</param>
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

            File.WriteAllText(path, FindingReportWriter.Build(graph));
            EditorUtility.RevealInFinder(path);
        }

        /// <summary>
        /// Writes everything about one namespace or assembly to a file of its own. A whole project
        /// report is the wrong thing to hand to someone about to work on one feature, and the right
        /// thing is small enough to read in full.
        /// </summary>
        /// <param name="graph">Graph to read from.</param>
        /// <param name="scope">Namespace or assembly name.</param>
        /// <param name="isAssembly">True when the scope names an assembly.</param>
        internal static void ExportScope(CodebaseGraphData graph, string scope, bool isAssembly)
        {
            if (graph == null || string.IsNullOrEmpty(scope))
                return;

            string path = EditorUtility.SaveFilePanel(ScopeTitle,
                string.Empty,
                $"{scope}{ScopeSuffix}",
                ExportExtension);

            if (string.IsNullOrEmpty(path))
                return;

            File.WriteAllText(path, ScopeReportWriter.Build(graph, scope, isAssembly));
            EditorUtility.RevealInFinder(path);
        }

        /// <summary>Reads dismissal instructions from the clipboard or a file and applies them.</summary>
        /// <returns>True when anything changed and the view should be rebuilt.</returns>
        internal static bool Import()
        {
            int choice = EditorUtility.DisplayDialogComplex(ImportLabel,
                ImportMessage,
                ImportFromClipboard,
                ImportCancel,
                ImportFromFile);

            if (choice == 1)
                return false;

            string text = choice == 0
                ? EditorGUIUtility.systemCopyBuffer
                : ReadInstructionFile();

            if (string.IsNullOrEmpty(text))
                return false;

            DismissalTextFormat.Apply(text, out int applied, out int ignored);

            EditorUtility.DisplayDialog(ImportLabel,
                string.Format(ImportResultFormat, applied, ignored),
                "OK");

            return applied > 0;
        }

        private static string ReadInstructionFile()
        {
            string path = EditorUtility.OpenFilePanel(ImportOpenTitle, string.Empty, ExportExtension);

            return string.IsNullOrEmpty(path) || !File.Exists(path)
                ? string.Empty
                : File.ReadAllText(path);
        }
    }
}