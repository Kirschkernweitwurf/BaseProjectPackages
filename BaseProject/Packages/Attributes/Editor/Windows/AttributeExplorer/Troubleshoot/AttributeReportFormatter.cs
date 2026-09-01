using System.Collections.Generic;
using System.Text;

namespace Base.AttributePackage.Editor.Windows.AttributeExplorer.Troubleshoot
{
    /// <summary>
    /// Turns a scan into plain text, so a report can leave the window and end up in a commit message, a
    /// ticket or a message to whoever wrote the type.
    /// </summary>
    /// <remarks>
    /// Markdown rather than the window's own layout. A finding is worth passing on far more often than
    /// it is worth reading twice, and a list of headings and bullets survives being pasted anywhere.
    /// </remarks>
    internal static class AttributeReportFormatter
    {
        private const string ErrorMarker = "error";
        private const string WarningMarker = "warning";

        /// <summary>Builds the text for a whole scan.</summary>
        /// <param name="groups">The scanned types and their findings.</param>
        /// <param name="onlyErrors">True to leave the warnings out.</param>
        /// <returns>The report, ready for the clipboard.</returns>
        internal static string Build(List<AttributeIssueGroup> groups, bool onlyErrors)
        {
            StringBuilder builder = new();

            foreach (AttributeIssueGroup group in groups)
            {
                int written = 0;

                foreach (AttributeIssue issue in group.Issues)
                {
                    if (onlyErrors && issue.Severity != EAttributeIssueSeverity.Error)
                        continue;

                    // The heading is written lazily, so a type whose findings were all filtered out does
                    // not leave an empty section behind.
                    if (written == 0)
                        builder.Append("## ").Append(group.DisplayName).Append('\n');

                    written++;

                    builder.Append("- ")
                        .Append(Marker(issue.Severity))
                        .Append(' ')
                        .Append(issue.AttributeName)
                        .Append(" on ")
                        .Append(issue.MemberName)
                        .Append(": ")
                        .Append(issue.Message)
                        .Append('\n');
                }

                if (written > 0)
                    builder.Append('\n');
            }

            return builder.ToString().TrimEnd();
        }

        private static string Marker(EAttributeIssueSeverity severity) => severity == EAttributeIssueSeverity.Error
            ? ErrorMarker
            : WarningMarker;
    }
}