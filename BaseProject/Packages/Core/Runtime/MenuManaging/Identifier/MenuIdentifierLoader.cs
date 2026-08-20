using System;
using Base.UtilityPackage.Logging;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Base.CorePackage.MenuManaging.Identifier
{
    /// <summary>
    /// Loads <see cref="MenuIdentifier"/> assets at runtime by their asset name.
    /// Uses a <see cref="MenuIdentifierRegistry"/> to resolve them.
    /// </summary>
    public static class MenuIdentifierLoader
    {
        private static MenuIdentifierRegistry _registry;

        /// <summary>
        /// Resolves the identifier asset with the given name.
        /// </summary>
        /// <param name="identifierName">The asset name of the identifier.</param>
        /// <returns>The identifier, or <c>null</c> if it is not registered.</returns>
        public static MenuIdentifier Load(string identifierName)
        {
            if (_registry == null)
                _registry = FindRegistry();

            if (_registry == null)
            {
                CustomLogger.LogError($"No {nameof(MenuIdentifierRegistry)} found under any Resources folder. "
                    + "Regenerate the menu identifiers from the Base Packages tools menu.", null);

                return null;
            }

            return _registry.TryGet(identifierName, out MenuIdentifier identifier)
                ? identifier
                : null;
        }

        /// <summary>
        /// Finds the registry by type rather than by asset name, so it can be renamed and moved
        /// freely as long as it stays under a Resources folder.
        /// </summary>
        private static MenuIdentifierRegistry FindRegistry()
        {
            MenuIdentifierRegistry[] found = Resources.LoadAll<MenuIdentifierRegistry>(string.Empty);
            if (found.Length == 0)
                return null;

            if (found.Length > 1)
                CustomLogger.LogError($"Found {found.Length} {nameof(MenuIdentifierRegistry)} assets, "
                    + $"expected one. Using \"{found[0].name}\". Regenerate to remove the duplicates.", null);

            Array.Sort(found, comparison: (first, second) => string.CompareOrdinal(first.name, second.name));
            return found[0];
        }

#if UNITY_EDITOR
        [InitializeOnEnterPlayMode]
        private static void ResetStatics() => _registry = null;
#endif
    }
}