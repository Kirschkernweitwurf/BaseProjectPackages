using UnityEngine;

namespace Base.SystemsCorePackage.MenuManaging.Identifier
{
    /// <summary>
    /// Holds references to all <see cref="MenuIdentifier"/> assets in the project so they can be resolved at runtime.
    /// </summary>
    [CreateAssetMenu(fileName = "MenuIdentifierRegistry",
        menuName = "ScriptableObjects/Base/Menus/Menu Identifier Registry")]
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
#endif
    }
}