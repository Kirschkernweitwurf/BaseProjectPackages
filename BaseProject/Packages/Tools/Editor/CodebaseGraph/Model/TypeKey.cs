using System;

namespace Base.ToolsPackage.Editor.CodebaseGraph.Model
{
    /// <summary>
    /// Identifies one type across the whole graph, on the same module and token scheme as
    /// <see cref="MemberKey"/>.
    /// </summary>
    internal readonly struct TypeKey : IEquatable<TypeKey>
    {
        /// <summary>Name of the module the type is declared in.</summary>
        private readonly string _moduleName;

        /// <summary>Metadata token of the type definition.</summary>
        private readonly int _token;

        /// <summary>True when the key points at an actual type instead of being the default value.</summary>
        public bool IsValid => !string.IsNullOrEmpty(_moduleName);

        /// <summary>Creates a type key.</summary>
        /// <param name="moduleName">Name of the declaring module.</param>
        /// <param name="token">Metadata token of the type definition.</param>
        public TypeKey(string moduleName, int token)
        {
            _moduleName = moduleName;
            _token = token;
        }

        /// <inheritdoc/>
        public bool Equals(TypeKey other) => _token == other._token
            && string.Equals(_moduleName, other._moduleName, StringComparison.Ordinal);

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is TypeKey other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(_moduleName, _token);

        /// <inheritdoc/>
        public override string ToString() => $"{_moduleName}:{_token}";
    }
}