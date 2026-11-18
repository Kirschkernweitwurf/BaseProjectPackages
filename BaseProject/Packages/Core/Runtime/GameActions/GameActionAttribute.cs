using System;

namespace Base.CorePackage.GameActions
{
    /// <summary>
    /// Optional marker on a concrete <see cref="GameAction"/> that overrides its stable id in
    /// <see cref="GameActionRegistry"/>. Actions are discovered by type regardless of this
    /// attribute; without it the id defaults to the type name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class GameActionAttribute : Attribute
    {
        /// <summary>
        /// Stable id for this action or <c>null</c> to fall back to the type name.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Creates the attribute with an optional stable id.
        /// </summary>
        /// <param name="id">Stable id or <c>null</c> to use the type name.</param>
        public GameActionAttribute(string id = null) => Id = id;
    }
}