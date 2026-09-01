using System.Collections.Generic;

namespace Base.ToolPackage.Editor.CodebaseGraph.Model
{
    /// <summary>One namespace in the graph, aggregated from the types it contains.</summary>
    internal sealed class NamespaceNodeInfo
    {
        /// <summary>Full namespace name, or "(global)" when the types have no namespace.</summary>
        internal string Name { get; }

        /// <summary>Types declared in this namespace.</summary>
        internal List<TypeNodeInfo> Types { get; }

        /// <summary>Namespaces this one depends on, with how many type level usages back that up.</summary>
        internal Dictionary<string, int> Outgoing { get; }

        /// <summary>Namespaces that depend on this one.</summary>
        internal Dictionary<string, int> Incoming { get; }

        /// <summary>Names of the other namespaces in the same dependency cycle, if any.</summary>
        internal List<string> CyclePartners { get; }

        /// <summary>Identifies the cycle this namespace belongs to, shared by every one in the loop.</summary>
        internal string CycleId { get; set; }

        /// <summary>The edges that close the loop, written out so the cycle can be checked by reading.</summary>
        internal string CycleDescription { get; set; }

        /// <summary>How many namespaces are tangled together around this loop.</summary>
        internal int CycleComponentSize { get; set; }

        /// <summary>The edge in the loop held together by the fewest usages, offered as a hint.</summary>
        internal string CycleCutHint { get; set; }

        /// <summary>Stable id used for dismissals, built once so lookups allocate nothing.</summary>
        internal string DismissalId { get; set; }

        /// <summary>True when something reported here was not reported by the previous scan.</summary>
        internal bool HasNewFindings { get; set; }

        /// <summary>Number of namespaces that depend on this one.</summary>
        internal int FanIn => Incoming.Count;

        /// <summary>Number of namespaces this one depends on.</summary>
        internal int FanOut => Outgoing.Count;

        /// <summary>Creates an empty namespace node.</summary>
        /// <param name="name">Full namespace name.</param>
        public NamespaceNodeInfo(string name)
        {
            Name = name;
            Types = new List<TypeNodeInfo>();
            Outgoing = new Dictionary<string, int>();
            Incoming = new Dictionary<string, int>();
            CyclePartners = new List<string>();
        }

        /// <summary>Adds one type level usage to the namespace level relation.</summary>
        /// <param name="target">Namespace that is being used.</param>
        internal void AddOutgoing(string target)
        {
            Outgoing.TryGetValue(target, out int count);
            Outgoing[target] = count + 1;
        }

        /// <summary>Records that another namespace uses this one.</summary>
        /// <param name="source">Namespace that uses this one.</param>
        internal void AddIncoming(string source)
        {
            Incoming.TryGetValue(source, out int count);
            Incoming[source] = count + 1;
        }
    }
}