using System;

namespace Base.ToolsPackage.Editor.AssemblyGraph.Architecture
{
    /// <summary>Identifies one directed edge between two assemblies.</summary>
    internal readonly struct AssemblyEdgeKey : IEquatable<AssemblyEdgeKey>
    {
        /// <summary>Name of the assembly the dependency starts at.</summary>
        public string Source { get; }

        /// <summary>Name of the assembly the dependency points at.</summary>
        public string Target { get; }

        /// <summary>True when the key points at an actual edge instead of being the default value.</summary>
        public bool IsValid => !string.IsNullOrEmpty(Source) && !string.IsNullOrEmpty(Target);

        /// <summary>Creates an edge key.</summary>
        /// <param name="source">Name of the depending assembly.</param>
        /// <param name="target">Name of the depended upon assembly.</param>
        public AssemblyEdgeKey(string source, string target)
        {
            Source = source;
            Target = target;
        }

        /// <inheritdoc/>
        public bool Equals(AssemblyEdgeKey other) => string.Equals(Source, other.Source, StringComparison.Ordinal)
            && string.Equals(Target, other.Target, StringComparison.Ordinal);

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is AssemblyEdgeKey other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Source, Target);

        /// <inheritdoc/>
        public override string ToString() => $"{Source} -> {Target}";
    }
}