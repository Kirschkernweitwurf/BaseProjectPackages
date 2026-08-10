using Base.ToolPackage.Editor.CodebaseGraph.Model;

namespace Base.ToolPackage.Editor.CodebaseGraph.Analysis
{
    /// <summary>One line of the findings report, with everything needed to rank and print it.</summary>
    internal sealed class FindingEntry
    {
        /// <summary>Which finding this line reports.</summary>
        public EFinding Finding { get; }

        /// <summary>How much attention it deserves.</summary>
        public ESeverity Severity { get; }

        /// <summary>Stable dismissal id, which doubles as the readable name.</summary>
        public string Id { get; }

        /// <summary>Asset path and line, or an empty string when the script could not be resolved.</summary>
        private readonly string location;

        /// <summary>Extra detail such as the other members of a cycle.</summary>
        private readonly string detail;

        /// <summary>Creates a report entry.</summary>
        /// <param name="finding">Which finding this line reports.</param>
        /// <param name="severity">How much attention it deserves.</param>
        /// <param name="id">Stable dismissal id.</param>
        /// <param name="location">Asset path and line.</param>
        /// <param name="detail">Extra detail, or an empty string.</param>
        public FindingEntry(EFinding finding, ESeverity severity, string id, string location, string detail)
        {
            Finding = finding;
            Severity = severity;
            Id = id;
            this.location = location;
            this.detail = detail;
        }

        /// <summary>Formats the entry as a Markdown list item.</summary>
        /// <returns>The line to write.</returns>
        public string Format() => $"- `{Id}`{location}{detail}";
    }
}