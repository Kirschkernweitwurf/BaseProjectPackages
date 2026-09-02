using System.Collections.Generic;

namespace Base.ToolsPackage.Editor.CodebaseGraph.Model
{
    /// <summary>
    /// One thing the window draws, whatever the current zoom level is. Namespaces, types and members
    /// are all flattened into this shape, so the layout, the graph, the list and the detail pane only
    /// ever deal with one kind of object. The level is kept on the entry because a namespace, a class
    /// and a field are drawn at different sizes and with different silhouettes.
    /// </summary>
    internal sealed class GraphEntry
    {
        /// <summary>Unique id inside the current view, used for edges and selection.</summary>
        internal string Id { get; }

        /// <summary>Main line shown on the node and in the list.</summary>
        internal string Title { get; }

        /// <summary>Second line, for example the kind and the visibility.</summary>
        internal string Subtitle { get; }

        /// <summary>Name the node color is derived from, so related entries share a tint.</summary>
        internal string ColorSeed { get; }

        /// <summary>How many entries depend on this one.</summary>
        internal int FanIn { get; }

        /// <summary>How many entries this one depends on.</summary>
        internal int FanOut { get; }

        /// <summary>Which zoom level this entry belongs to.</summary>
        internal EGraphScope Level { get; }

        /// <summary>Single letter standing for the kind, shown in the node title.</summary>
        internal string Glyph { get; set; }

        /// <summary>Declared visibility, which colors the accent stripe.</summary>
        internal EAccessLevel Access { get; set; }

        /// <summary>Relations this entry points at, limited to what is currently visible.</summary>
        internal List<GraphEdgeInfo> Targets { get; }

        /// <summary>Members listed inside the node, for type entries.</summary>
        internal List<GraphMemberRow> Rows { get; }

        /// <summary>Members that exist but did not fit in the list.</summary>
        internal int HiddenRowCount { get; set; }

        /// <summary>Findings reported on this entry.</summary>
        internal List<EFinding> Findings { get; }

        /// <summary>The namespace this entry stands for, when the view is at namespace level.</summary>
        internal NamespaceNodeInfo Namespace { get; set; }

        /// <summary>The type this entry stands for, or the declaring type of the member.</summary>
        internal TypeNodeInfo Type { get; set; }

        /// <summary>The member this entry stands for, when the view is at member level.</summary>
        internal MemberNodeInfo Member { get; set; }

        /// <summary>True when double-clicking the entry opens a deeper level.</summary>
        internal bool CanDrillDown { get; set; }

        /// <summary>Number of findings on the members inside this entry, for types and namespaces.</summary>
        internal int NestedFindingCount { get; set; }

        /// <summary>True when this entry was reviewed and dismissed, so its findings are silenced.</summary>
        internal bool IsDismissed { get; set; }

        /// <summary>Number of members inside this entry whose findings were dismissed.</summary>
        internal int DismissedNestedCount { get; set; }

        /// <summary>True when the entry is drawn with a dashed border, which marks a contract.</summary>
        internal bool IsContract { get; set; }

        /// <summary>True when something reported here is still waiting to be dealt with.</summary>
        internal bool HasOpenFindings => Findings.Count > 0 || NestedFindingCount > 0;

        /// <summary>True when the entry has been dismissed, itself or through what it contains.</summary>
        internal bool HasDismissals => IsDismissed || DismissedNestedCount > 0;

        /// <summary>
        /// How many badges the node draws. Each one sits on its own line, so the layout can work out a
        /// node's height exactly rather than guessing how many will fit on a row. Dismissed findings
        /// get a badge too: a decision that leaves no trace on screen is one nobody can review.
        /// </summary>
        internal int BadgeCount => Findings.Count
            + (NestedFindingCount > 0
                ? 1
                : 0)
            + (IsDismissed
                ? 1
                : 0)
            + (DismissedNestedCount > 0
                ? 1
                : 0);

        /// <summary>Creates an entry without relations, rows or findings yet.</summary>
        /// <param name="id">Unique id inside the current view.</param>
        /// <param name="title">Main line.</param>
        /// <param name="subtitle">Second line.</param>
        /// <param name="colorSeed">Name the node color is derived from.</param>
        /// <param name="fanIn">How many entries depend on this one.</param>
        /// <param name="fanOut">How many entries this one depends on.</param>
        /// <param name="level">Which zoom level the entry belongs to.</param>
        public GraphEntry(string id,
            string title,
            string subtitle,
            string colorSeed,
            int fanIn,
            int fanOut,
            EGraphScope level)
        {
            Id = id;
            Title = title;
            Subtitle = subtitle;
            ColorSeed = colorSeed;
            FanIn = fanIn;
            FanOut = fanOut;
            Level = level;
            Targets = new List<GraphEdgeInfo>();
            Rows = new List<GraphMemberRow>();
            Findings = new List<EFinding>();
        }
    }
}