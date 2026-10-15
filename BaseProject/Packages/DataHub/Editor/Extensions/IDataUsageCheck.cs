using System;

namespace Base.DataHubPackage.Editor.Extensions
{
    /// <summary>
    /// Asks the Data Hub to warn about assets of <see cref="TargetType"/> that nothing uses yet, like a card
    /// that is in no deck. Being in its own collection does not count as being used. It is a warning, not a
    /// problem, because a new asset is always unused at first.
    /// </summary>
    /// <remarks>
    /// Other data assets count as use, and so do the saved scenes, with the prefabs in them. Leave out types
    /// the game finds some other way, like every asset of a collection used as a whole at runtime, or each of
    /// them would be flagged.
    /// </remarks>
    public interface IDataUsageCheck
    {
        /// <summary>The type whose unused assets get a warning, including derived types.</summary>
        Type TargetType { get; }
    }
}