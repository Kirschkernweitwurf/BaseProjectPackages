using System.Linq;
using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEngine;

namespace Base.CorePackage.MenuManaging.Identifier
{
    /// <summary>
    /// Loads <see cref="MenuIdentifier"/> assets at runtime by their asset name.
    /// Uses a <see cref="MenuIdentifierRegistry"/> to resolve them.
    /// </summary>
    public static class MenuIdentifierLoader
    {
        private static MenuIdentifierRegistry _registry;

        public static MenuIdentifier Load(string identifierName)
        {
            if (_registry == null)
                _registry = FindRegistry();

            if (_registry == null)
            {
                CustomLogger.LogError("No MenuIdentifierRegistry found under any Resources folder. "
                    + "Run Tools > Base Packages > Menu > Regenerate Menu Identifiers.", null);

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
                CustomLogger.LogError($"Found {found.Length} MenuIdentifierRegistry assets, expected one. "
                    + $"Using '{found[0].name}'. Regenerate to remove the duplicates.", null);

            return found.OrderBy(r => r.name).First();
        }

#if UNITY_EDITOR
        [InitializeOnEnterPlayMode]
        private static void ResetStatics() => _registry = null;
#endif
    }
}
