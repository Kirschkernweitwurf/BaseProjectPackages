using System.Collections.Generic;

namespace Base.ToolPackage.Editor.CodebaseGraph.Model
{
    /// <summary>
    /// One thing the window draws, whatever the current zoom level is. Namespaces, types and members
    /// are all flattened into this shape, so the layout, the graph, the list and the detail pane only
    /// ever deal with one kind of object.
    /// </summary>
    public sealed class GraphEntry
    {
        /// <summary>Unique id inside the current view, used for edges and selection.</summary>
        public string Id { get; }

        /// <summary>Main line shown on the node and in the list.</summary>
        public string Title { get; }

        /// <summary>Second line, for example the kind and the visibility.</summary>
        public string Subtitle { get; }

        /// <summary>Name the node color is derived from, so related entries share a tint.</summary>
        public string ColorSeed { get; }

        /// <summary>How many entries depend on this one.</summary>
        public int FanIn { get; }

        /// <summary>How many entries this one depends on.</summary>
        public int FanOut { get; }

        /// <summary>Ids of the entries this one depends on, limited to what is currently visible.</summary>
        public List<string> TargetIds { get; }

        /// <summary>Findings reported on this entry.</summary>
        public List<EFinding> Findings { get; }

        /// <summary>The namespace this entry stands for, when the view is at namespace level.</summary>
        public NamespaceNodeInfo Namespace { get; set; }

        /// <summary>The type this entry stands for, or the declaring type of the member.</summary>
        public TypeNodeInfo Type { get; set; }

        /// <summary>The member this entry stands for, when the view is at member level.</summary>
        public MemberNodeInfo Member { get; set; }

        /// <summary>True when double clicking the entry opens a deeper level.</summary>
        public bool CanDrillDown { get; set; }

        /// <summary>Number of findings on the members inside this entry, for types and namespaces.</summary>
        public int NestedFindingCount { get; set; }

        /// <summary>True when the analyzer reported anything on the entry itself.</summary>
        public bool HasFindings => Findings.Count > 0;

        /// <summary>
        /// How many badges the node draws. Each one sits on its own line, so the layout can work out a
        /// node's height exactly rather than guessing how many will fit on a row.
        /// </summary>
        public int BadgeCount => Findings.Count + (NestedFindingCount > 0
            ? 1
            : 0);

        /// <summary>Creates an entry without targets or findings yet.</summary>
        /// <param name="id">Unique id inside the current view.</param>
        /// <param name="title">Main line.</param>
        /// <param name="subtitle">Second line.</param>
        /// <param name="colorSeed">Name the node color is derived from.</param>
        /// <param name="fanIn">How many entries depend on this one.</param>
        /// <param name="fanOut">How many entries this one depends on.</param>
        public GraphEntry(string id, string title, string subtitle, string colorSeed, int fanIn, int fanOut)
        {
            Id = id;
            Title = title;
            Subtitle = subtitle;
            ColorSeed = colorSeed;
            FanIn = fanIn;
            FanOut = fanOut;
            TargetIds = new List<string>();
            Findings = new List<EFinding>();
        }
    }
}
