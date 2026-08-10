using Base.ToolPackage.Editor.CodebaseGraph.Model;

namespace Base.ToolPackage.Editor.CodebaseGraph.Scanning
{
    /// <summary>What a metadata token turned out to point at, once resolved.</summary>
    public readonly struct TokenResolution
    {
        /// <summary>Member the token names, or the default when it names no member.</summary>
        public MemberKey Member { get; }

        /// <summary>Type the token names or the member belongs to, or the default when there is none.</summary>
        public TypeKey Type { get; }

        /// <summary>True when the runtime could rebuild the token at all.</summary>
        public bool IsResolved { get; }

        /// <summary>True when the token names a method that reaches other code by name.</summary>
        public bool IsDispatch { get; }

        /// <summary>Creates a resolution.</summary>
        /// <param name="member">Member the token names.</param>
        /// <param name="type">Type the token names or the member belongs to.</param>
        /// <param name="isResolved">Whether the token could be rebuilt.</param>
        /// <param name="isDispatch">Whether the token names a method that dispatches by name.</param>
        public TokenResolution(MemberKey member, TypeKey type, bool isResolved, bool isDispatch)
        {
            Member = member;
            Type = type;
            IsResolved = isResolved;
            IsDispatch = isDispatch;
        }
    }
}
