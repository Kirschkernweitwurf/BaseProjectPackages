using System.Collections.Generic;
using System.IO;
using System.Text;
using Base.ToolsPackage.Editor.CodebaseGraph.Analysis;
using Base.ToolsPackage.Editor.CodebaseGraph.Model;
using UnityEditor;

namespace Base.ToolsPackage.Editor.CodebaseGraph.Editing
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
        private const string ImportAppliedFormat = "Applied {0} of {1} lines.";
        private const string ImportAppliedOnly = "Applied all {0} lines.";
        private const string ImportCancel = "Cancel";
        private const string ImportFromClipboard = "Paste from clipboard";
        private const string ImportFromFile = "Load from file";
        private const string ImportLabel = "Update dismissals";

        private const string ImportMessage = "Reads a list of dismissals, the same block the findings "
            + "report writes at the end. One per line:\n\n"
            + "  dismiss <id>\n  dismiss-tree <id>\n  restore <id>\n  restore-tree <id>\n\n"
            + "Anything you leave out stays as it is. Only restore removes a dismissal.";

        private const string ImportNothing = "Nothing to apply. Every line was blank or a comment.";
        private const string ImportOpenTitle = "Open dismissal instructions";
        private const string ImportSkippedHeader = "\n\nThese changed nothing:";
        private const string ImportSkippedLineFormat = "\n\nLine {0}: {1}\n    {2}";
        private const string ImportSkippedMoreFormat = "\n\nand {0} more.";

        // Past a handful the dialog stops being readable, and the ones shown are enough to recognize
        // the mistake. The count says how many more are of the same shape.
        private const int ImportSkippedShown = 6;

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

            IReadOnlyList<DismissalLineResult> results = DismissalTextFormat.Apply(text);
            int applied = 0;

            foreach (DismissalLineResult result in results)
            {
                if (result.IsApplied)
                    applied++;
            }

            EditorUtility.DisplayDialog(ImportLabel, Describe(results, applied), "OK");

            return applied > 0;
        }

        /// <summary>
        /// Says what happened, naming every line that changed nothing and why. A count on its own
        /// leaves the reader to guess which line it meant, and whether it was a mistake or a no-op.
        /// </summary>
        /// <param name="results">One result per instruction line.</param>
        /// <param name="applied">How many of them changed something.</param>
        /// <returns>The dialog text.</returns>
        private static string Describe(IReadOnlyList<DismissalLineResult> results, int applied)
        {
            if (results.Count == 0)
                return ImportNothing;

            StringBuilder builder = new();

            builder.Append(applied == results.Count
                ? string.Format(ImportAppliedOnly, applied)
                : string.Format(ImportAppliedFormat, applied, results.Count));

            if (applied == results.Count)
                return builder.ToString();

            builder.Append(ImportSkippedHeader);

            int shown = 0;
            int hidden = 0;

            foreach (DismissalLineResult result in results)
            {
                if (result.IsApplied)
                    continue;

                if (shown == ImportSkippedShown)
                {
                    hidden++;
                    continue;
                }

                builder.AppendFormat(ImportSkippedLineFormat, result.Number, result.Describe(), result.Text);
                shown++;
            }

            if (hidden > 0)
                builder.AppendFormat(ImportSkippedMoreFormat, hidden);

            return builder.ToString();
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