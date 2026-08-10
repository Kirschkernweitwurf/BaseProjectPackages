using System.Collections.Generic;
using Base.ToolPackage.Editor.CodebaseGraph.Model;

namespace Base.ToolPackage.Editor.CodebaseGraph.Scanning
{
    /// <summary>
    /// Folds the raw usages the scanners report into the graph. Duplicate usages are merged into a
    /// single edge with a count, and anything pointing outside the scanned assemblies is tallied as an
    /// external reference instead of pulling Unity into the picture.
    /// </summary>
    internal sealed class GraphUsageSink : IUsageSink
    {
        private readonly CodebaseGraphData _graph;
        private readonly MemberRegistry _registry;

        private readonly Dictionary<(MemberKey Source, MemberKey Target, EUsageKind Kind), UsageEdgeInfo> _edges =
            new();

        /// <summary>Creates a sink that writes into the given graph.</summary>
        /// <param name="graph">Graph that receives the nodes and edges.</param>
        /// <param name="registry">Registry used to fold machinery onto the code that was written.</param>
        public GraphUsageSink(CodebaseGraphData graph, MemberRegistry registry)
        {
            _graph = graph;
            _registry = registry;
        }

        /// <inheritdoc/>
        public void AddMemberUsage(MemberKey sourceKey, MemberKey targetKey, EUsageKind kind)
        {
            MemberNodeInfo source = _graph.FindMember(sourceKey);
            if (source == null)
                return;

            MemberKey resolvedTarget = _registry.Resolve(targetKey);
            MemberNodeInfo target = _graph.FindMember(resolvedTarget);
            if (target == null)
                return;

            if (sourceKey.Equals(resolvedTarget))
                return;

            (MemberKey Source, MemberKey Target, EUsageKind Kind) id = (sourceKey, resolvedTarget, kind);

            if (_edges.TryGetValue(id, out UsageEdgeInfo existing))
            {
                existing.Increment();
                return;
            }

            UsageEdgeInfo edge = new(sourceKey, resolvedTarget, kind);
            _edges[id] = edge;
            source.Outgoing.Add(edge);
            target.Incoming.Add(edge);

            LinkTypes(source.DeclaringTypeKey, target.DeclaringTypeKey);
        }

        /// <inheritdoc/>
        public void AddTypeUsage(MemberKey sourceKey, TypeKey targetKey)
        {
            MemberNodeInfo source = _graph.FindMember(sourceKey);
            if (source == null)
                return;

            TypeNodeInfo sourceType = _graph.FindType(source.DeclaringTypeKey);
            if (sourceType == null)
                return;

            if (_graph.FindType(targetKey) == null)
            {
                sourceType.ExternalReferenceCount++;
                return;
            }

            LinkTypes(source.DeclaringTypeKey, targetKey);
        }

        /// <inheritdoc/>
        public void AddTypeRelation(TypeKey sourceKey, TypeKey targetKey) => LinkTypes(sourceKey, targetKey);

        /// <inheritdoc/>
        public void AddIlSize(MemberKey sourceKey, int size)
        {
            MemberNodeInfo member = _graph.FindMember(sourceKey);
            if (member == null)
                return;

            // Lambdas and iterators resolve onto their owner, so their bodies count towards it too.
            member.IlSize += size;
        }

        /// <inheritdoc/>
        public void ReportUnresolvedToken() => _graph.UnresolvedTokenCount++;

        private void LinkTypes(TypeKey sourceKey, TypeKey targetKey)
        {
            if (sourceKey.Equals(targetKey))
                return;

            TypeNodeInfo source = _graph.FindType(sourceKey);
            TypeNodeInfo target = _graph.FindType(targetKey);

            if (source == null || target == null)
                return;

            source.AddOutgoing(targetKey);
            target.AddIncoming(sourceKey);
        }
    }
}