using System;

namespace Base.TextPackage
{
    /// <summary>
    /// Tells the text fit check where code places the prefab a field holds, for a list entry that gets its size from
    /// the list, like a row of a settings list. Put it on the prefab field and name the field of the same
    /// component that holds the parent: <c>[SpawnedIn(nameof(rowParent))]</c>. A list of parents uses its first one.
    /// The check then measures the entry's texts there, with the room the game gives them, instead of alone without
    /// any size.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SpawnedInAttribute : Attribute
    {
        /// <summary>Marks the prefab as placed under the parent in <paramref name="parentField"/>.</summary>
        /// <param name="parentField">The name of a Transform field or a list of them, on the same component.</param>
        public SpawnedInAttribute(string parentField) => ParentField = parentField;

        /// <summary>The name of the parent field on the same component.</summary>
        public string ParentField { get; }
    }
}