using System;
using UnityEngine;

namespace Base.CorePackage.MenuManaging.Identifier
{
    /// <summary>
    /// Holds references to all <see cref="MenuIdentifier"/> assets in the project so they can be resolved at runtime.
    /// </summary>
    /// <remarks>
    /// Created and maintained automatically by the generator, and deliberately not creatable from
    /// the asset menu. Exactly one registry exists per project. To relocate it, move the existing
    /// asset in the Project window. It has to stay somewhere under a Resources folder.
    /// </remarks>
    public class MenuIdentifierRegistry : ScriptableObject
    {
        [SerializeField] private MenuIdentifier[] entries;

        /// <summary>
        /// Tries to find a registered identifier by its asset name.
        /// </summary>
        /// <param name="identifierName">The asset name to look for.</param>
        /// <param name="identifier">The matching identifier if one was found; otherwise, null.</param>
        /// <returns><c>true</c> if the identifier was found; otherwise, <c>false</c>.</returns>
        public bool TryGet(string identifierName, out MenuIdentifier identifier)
        {
            identifier = null;

            if (entries == null)
                return false;

            foreach (MenuIdentifier entry in entries)
            {
                if (entry == null || entry.name != identifierName)
                    continue;

                identifier = entry;
                return true;
            }

            return false;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor-only: replaces all registered entries. Called by the generator.
        /// </summary>
        /// <param name="newEntries">The entries to store.</param>
        public void SetEntries(MenuIdentifier[] newEntries) => entries = newEntries;

        /// <summary>
        /// Editor-only: returns true if the current entries match the given set in the same order.
        /// Lets the generator skip writing the asset when nothing actually changed.
        /// </summary>
        /// <param name="candidate">The entries to compare against.</param>
        /// <returns><c>true</c> if both sets are equal; otherwise, <c>false</c>.</returns>
        public bool EntriesEqual(MenuIdentifier[] candidate)
        {
            MenuIdentifier[] current = entries ?? Array.Empty<MenuIdentifier>();
            MenuIdentifier[] other = candidate ?? Array.Empty<MenuIdentifier>();

            if (current.Length != other.Length)
                return false;

            for (int i = 0; i < current.Length; i++)
            {
                if (current[i] != other[i])
                    return false;
            }

            return true;
        }
#endif
    }
}