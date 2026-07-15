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
        public bool TryGet(string identifierName, out MenuIdentifier identifier)
        {
            if (entries != null)
                foreach (MenuIdentifier entry in entries)
                {
                    if (entry == null || entry.name != identifierName)
                        continue;

                    identifier = entry;
                    return true;
                }

            identifier = null;
            return false;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor-only: replaces all registered entries. Called by the generator.
        /// </summary>
        public void SetEntries(MenuIdentifier[] newEntries) => entries = newEntries;

        /// <summary>
        /// Editor-only: returns true if the current entries match the given set in the same order.
        /// Lets the generator skip writing the asset when nothing actually changed.
        /// </summary>
        public bool EntriesEqual(MenuIdentifier[] candidate)
        {
            int currentCount = entries?.Length ?? 0;
            int candidateCount = candidate?.Length ?? 0;
            if (currentCount != candidateCount)
                return false;

            for (int i = 0; i < currentCount; i++)
            {
                if (entries == null || candidate == null)
                    return false;

                if (entries[i] != candidate[i])
                    return false;
            }

            return true;
        }
#endif
    }
}