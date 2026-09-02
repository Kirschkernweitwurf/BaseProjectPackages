using System;

namespace Base.ToolsPackage.Editor.CodebaseGraph.Model
{
    /// <summary>
    /// Identifies one member across the whole graph. The metadata token is unique inside a module,
    /// so the module name plus the token is exact and cheap to hash. Keys stay valid until the next
    /// domain reload, which is also when the graph cache is dropped.
    /// </summary>
    internal readonly struct MemberKey : IEquatable<MemberKey>
    {
        /// <summary>Name of the module the member is declared in.</summary>
        private readonly string moduleName;

        /// <summary>Metadata token of the member definition.</summary>
        public int Token { get; }

        /// <summary>True when the key points at an actual member instead of being the default value.</summary>
        public bool IsValid => !string.IsNullOrEmpty(moduleName);

        /// <summary>Creates a member key.</summary>
        /// <param name="moduleName">Name of the declaring module.</param>
        /// <param name="token">Metadata token of the member definition.</param>
        public MemberKey(string moduleName, int token)
        {
            this.moduleName = moduleName;
            Token = token;
        }

        /// <inheritdoc/>
        public bool Equals(MemberKey other) => Token == other.Token
            && string.Equals(moduleName, other.moduleName, StringComparison.Ordinal);

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is MemberKey other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(moduleName, Token);

        /// <inheritdoc/>
        public override string ToString() => $"{moduleName}:{Token}";
    }
}