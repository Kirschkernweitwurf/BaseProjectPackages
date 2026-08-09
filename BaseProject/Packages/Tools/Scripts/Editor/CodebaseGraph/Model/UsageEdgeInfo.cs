namespace Base.ToolPackage.Editor.CodebaseGraph.Model
{
    /// <summary>One directed usage from a source member to a target member, with how often it occurs.</summary>
    public sealed class UsageEdgeInfo
    {
        /// <summary>Member the usage originates from.</summary>
        public MemberKey SourceKey { get; }

        /// <summary>Member that is being used.</summary>
        public MemberKey TargetKey { get; }

        /// <summary>What kind of usage this is.</summary>
        public EUsageKind Kind { get; }

        /// <summary>How many times the usage was found.</summary>
        public int Count { get; private set; }

        /// <summary>Creates a usage edge with a count of one.</summary>
        /// <param name="sourceKey">Member the usage originates from.</param>
        /// <param name="targetKey">Member that is being used.</param>
        /// <param name="kind">What kind of usage this is.</param>
        public UsageEdgeInfo(MemberKey sourceKey, MemberKey targetKey, EUsageKind kind)
        {
            SourceKey = sourceKey;
            TargetKey = targetKey;
            Kind = kind;
            Count = 1;
        }

        /// <summary>Raises the occurrence count by one.</summary>
        public void Increment() => Count++;
    }
}
