using System.Collections.Generic;

namespace Base.ToolPackage.Editor.CodebaseGraph.Model
{
    /// <summary>One field, property, method, constructor or event in the graph, with its usages.</summary>
    public sealed class MemberNodeInfo
    {
        /// <summary>Identity of the member.</summary>
        public MemberKey Key { get; }

        /// <summary>Plain member name, without parameters.</summary>
        public string Name { get; }

        /// <summary>Display signature, for example "Move(Vector3, Single) : Boolean".</summary>
        public string Signature { get; }

        /// <summary>Category the member falls into.</summary>
        public EMemberKind Kind { get; }

        /// <summary>Declared visibility.</summary>
        public EAccessLevel Access { get; }

        /// <summary>Type the member is declared in.</summary>
        public TypeKey DeclaringTypeKey { get; }

        /// <summary>True for static members.</summary>
        public bool IsStatic { get; }

        /// <summary>True for readonly fields and get-only properties.</summary>
        public bool IsReadOnly { get; }

        /// <summary>True when the member overrides a base member or implements an interface member.</summary>
        public bool IsOverride { get; set; }

        /// <summary>True when derived types are expected to supply or replace the implementation.</summary>
        public bool IsVirtual { get; }

        /// <summary>True when the member has no body and a derived type has to provide one.</summary>
        public bool IsAbstract { get; }

        /// <summary>True when something outside the code can call or write this member.</summary>
        public bool IsEntryPoint { get; set; }

        /// <summary>Reason the member counts as an entry point, used for the tooltip.</summary>
        public string EntryPointReason { get; set; }

        /// <summary>
        /// True when the text sidecar found the member name used somewhere in source. Only set for consts
        /// and enum members, whose reads the compiler inlines and which are therefore invisible in IL.
        /// </summary>
        public bool HasTextUsage { get; set; }

        /// <summary>True when a reset method assigns this field on entering play mode.</summary>
        public bool IsStateReset { get; set; }

        /// <summary>How many types implement this member, for interface declarations.</summary>
        public int ImplementationCount { get; set; }

        /// <summary>True when an interface the declaring type implements declares a member of this name.</summary>
        public bool ImplementsInterfaceMember { get; set; }

        /// <summary>How many prefabs, scenes or assets set this serialized field.</summary>
        public int AssetUsageCount { get; set; }

        /// <summary>True when the source line carries the ignore marker, which silences every finding.</summary>
        public bool IsSuppressed { get; set; }

        /// <summary>Size of the compiled method body in bytes. Zero for data members.</summary>
        public int IlSize { get; set; }

        /// <summary>Every usage that starts at this member.</summary>
        public List<UsageEdgeInfo> Outgoing { get; }

        /// <summary>Every usage that points at this member.</summary>
        public List<UsageEdgeInfo> Incoming { get; }

        /// <summary>Findings the analyzer reported for this member.</summary>
        public EMemberIssue Issues { get; set; }

        /// <summary>
        /// Number of distinct members that use this one. Not the edge count: one caller that both calls
        /// and reads a member produces two edges but is still a single user.
        /// </summary>
        public int FanIn { get; private set; }

        /// <summary>Number of distinct members this one uses.</summary>
        public int FanOut { get; private set; }

        /// <summary>Stable id used for dismissals, built once so lookups allocate nothing.</summary>
        public string DismissalId { get; set; }

        /// <summary>True when the analyzer reported anything.</summary>
        public bool HasIssues => Issues != EMemberIssue.None;

        /// <summary>True when the member holds data instead of behavior.</summary>
        public bool IsDataMember => Kind == EMemberKind.Field
            || Kind == EMemberKind.SerializedField
            || Kind == EMemberKind.Const
            || Kind == EMemberKind.EnumMember;

        /// <summary>Creates a member node without any usages yet.</summary>
        /// <param name="key">Identity of the member.</param>
        /// <param name="name">Plain member name.</param>
        /// <param name="signature">Display signature.</param>
        /// <param name="kind">Category the member falls into.</param>
        /// <param name="access">Declared visibility.</param>
        /// <param name="declaringTypeKey">Type the member is declared in.</param>
        /// <param name="isStatic">Whether the member is static.</param>
        /// <param name="isReadOnly">Whether the member cannot be written after construction.</param>
        /// <param name="isVirtual">Whether derived types can supply or replace the implementation.</param>
        /// <param name="isAbstract">Whether the member has no body of its own.</param>
        public MemberNodeInfo(MemberKey key,
            string name,
            string signature,
            EMemberKind kind,
            EAccessLevel access,
            TypeKey declaringTypeKey,
            bool isStatic,
            bool isReadOnly,
            bool isVirtual,
            bool isAbstract)
        {
            Key = key;
            Name = name;
            Signature = signature;
            Kind = kind;
            Access = access;
            DeclaringTypeKey = declaringTypeKey;
            IsStatic = isStatic;
            IsReadOnly = isReadOnly;
            IsVirtual = isVirtual;
            IsAbstract = isAbstract;
            Outgoing = new List<UsageEdgeInfo>();
            Incoming = new List<UsageEdgeInfo>();
        }

        /// <summary>
        /// Recounts the distinct users on each side. Called once after scanning, with a scratch set the
        /// caller reuses, so counting every member costs one allocation rather than fifteen thousand.
        /// </summary>
        /// <param name="scratch">A set the caller owns and reuses between members.</param>
        public void RecomputeFanCounts(HashSet<MemberKey> scratch)
        {
            scratch.Clear();
            foreach (UsageEdgeInfo edge in Incoming)
                scratch.Add(edge.SourceKey);

            FanIn = scratch.Count;

            scratch.Clear();
            foreach (UsageEdgeInfo edge in Outgoing)
                scratch.Add(edge.TargetKey);

            FanOut = scratch.Count;
        }

        /// <summary>True when at least one incoming usage writes to this member.</summary>
        public bool HasIncomingWrite()
        {
            foreach (UsageEdgeInfo edge in Incoming)
            {
                if (edge.Kind == EUsageKind.FieldWrite)
                    return true;
            }

            return false;
        }

        /// <summary>True when at least one incoming usage reads this member.</summary>
        public bool HasIncomingRead()
        {
            foreach (UsageEdgeInfo edge in Incoming)
            {
                if (edge.Kind != EUsageKind.FieldWrite)
                    return true;
            }

            return false;
        }
    }
}
