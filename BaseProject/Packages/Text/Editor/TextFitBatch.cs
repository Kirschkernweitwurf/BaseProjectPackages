using System.Collections.Generic;
using UnityEditor;

namespace Base.TextPackage.Editor
{
    /// <summary>
    /// Runs the text fit check from the command line, for a build pipeline:
    /// <c>-batchmode -quit -executeMethod Base.TextPackage.Editor.TextFitBatch.Run</c>.
    /// Exits with 1 when a text of a real language does not fit, so the pipeline stops there. Pseudo-locale
    /// problems are warnings and do not fail it.
    /// </summary>
    public static class TextFitBatch
    {
        private const int FailedExitCode = 1;
        private const int PassedExitCode = 0;

        /// <summary>Checks every text, writes the report and exits Unity with the result.</summary>
        public static void Run()
        {
            List<TextFitIssue> issues = TextFitValidator.Run();

            EditorApplication.Exit(TextFitValidator.IsShippable(issues)
                ? PassedExitCode
                : FailedExitCode);
        }
    }
}