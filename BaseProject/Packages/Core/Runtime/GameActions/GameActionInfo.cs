using System;

namespace Base.CorePackage.GameActions
{
    /// <summary>
    /// Immutable cache entry pairing a <see cref="GameAction"/> type with its stable id.
    /// </summary>
    public readonly struct GameActionInfo
    {
        /// <summary>
        /// The concrete action type.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// The stable id resolved from <see cref="GameActionAttribute"/> or the type name.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Creates an entry.
        /// </summary>
        /// <param name="type">The concrete action type.</param>
        /// <param name="id">The stable id.</param>
        public GameActionInfo(Type type, string id)
        {
            Type = type;
            Id = id;
        }
    }
}