using System.Collections.Generic;

namespace Base.ToolPackage.Editor.AssemblyGraph.Architecture
{
    /// <summary>
    /// One dependency between two assemblies, with the types that actually justify it. The declared
    /// reference in an asmdef only says that an edge exists. What matters when deciding whether to cut
    /// it is how wide it is, and every cycle this package set ever had started as an edge one type wide.
    /// <br/><br/>
    /// <see cref="Weight"/> counts distinct target types, not usages. Nested types are folded into the
    /// outermost type that declares them, so a helper struct inside a class never makes an edge look
    /// twice as wide as the code reads.
    /// </summary>
    internal sealed class AssemblyEdgeInfo
    {
        /// <summary>Identity of the edge.</summary>
        internal AssemblyEdgeKey Key { get; }

        /// <summary>Name of the assembly the dependency starts at.</summary>
        internal string SourceName => Key.Source;

        /// <summary>Name of the assembly the dependency points at.</summary>
        internal string TargetName => Key.Target;

        /// <summary>Full names of the distinct target types this edge exists for, sorted.</summary>
        internal IReadOnlyList<string> TargetTypeNames { get; }

        /// <summary>Full names of the distinct source types that reach across the edge, sorted.</summary>
        internal IReadOnlyList<string> SourceTypeNames { get; }

        /// <summary>Number of member level usages behind the edge, which is what the scan counted.</summary>
        internal int UsageCount { get; }

        /// <summary>
        /// True when every source type behind the edge is generated, sample or test code. The
        /// dependency is real and still compiles, but no amount of refactoring will remove it, so a
        /// rule that suggests cutting it is wasting the reader's time.
        /// </summary>
        internal bool IsEntirelyExcluded { get; }

        /// <summary>How many distinct target types hold the edge up. This is the number rules read.</summary>
        internal int Weight => TargetTypeNames.Count;

        /// <summary>Creates a finished edge.</summary>
        /// <param name="key">Identity of the edge.</param>
        /// <param name="targetTypeNames">Distinct target types, already sorted.</param>
        /// <param name="sourceTypeNames">Distinct source types, already sorted.</param>
        /// <param name="usageCount">Number of member level usages behind the edge.</param>
        /// <param name="isEntirelyExcluded">Whether every source type is generated, sample or test code.</param>
        public AssemblyEdgeInfo(AssemblyEdgeKey key,
            IReadOnlyList<string> targetTypeNames,
            IReadOnlyList<string> sourceTypeNames,
            int usageCount,
            bool isEntirelyExcluded)
        {
            Key = key;
            TargetTypeNames = targetTypeNames;
            SourceTypeNames = sourceTypeNames;
            UsageCount = usageCount;
            IsEntirelyExcluded = isEntirelyExcluded;
        }

        /// <inheritdoc/>
        public override string ToString() => $"{Key} ({Weight} types, {UsageCount} usages)";
    }
}