using System;

namespace Base.ToolPackage.Editor.CodebaseGraph.Model
{
    /// <summary>
    /// Identifies one type across the whole graph, on the same module and token scheme as
    /// <see cref="MemberKey"/>.
    /// </summary>
    public readonly struct TypeKey : IEquatable<TypeKey>
    {
        /// <summary>Name of the module the type is declared in.</summary>
        public string ModuleName { get; }

        /// <summary>Metadata token of the type definition.</summary>
        public int Token { get; }

        /// <summary>True when the key points at an actual type instead of being the default value.</summary>
        public bool IsValid => !string.IsNullOrEmpty(ModuleName);

        /// <summary>Creates a type key.</summary>
        /// <param name="moduleName">Name of the declaring module.</param>
        /// <param name="token">Metadata token of the type definition.</param>
        public TypeKey(string moduleName, int token)
        {
            ModuleName = moduleName;
            Token = token;
        }

        /// <inheritdoc/>
        public bool Equals(TypeKey other)
            => Token == other.Token && string.Equals(ModuleName, other.ModuleName, StringComparison.Ordinal);

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is TypeKey other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(ModuleName, Token);

        /// <inheritdoc/>
        public override string ToString() => $"{ModuleName}:{Token}";
    }
}
