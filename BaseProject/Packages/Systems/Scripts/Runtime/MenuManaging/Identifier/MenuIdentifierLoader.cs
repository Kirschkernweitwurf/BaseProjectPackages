using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.SystemsCorePackage.MenuManaging.Identifier
{
    /// <summary>
    /// Loads <see cref="MenuIdentifier"/> assets at runtime by their asset name.
    /// Uses a <see cref="MenuIdentifierRegistry"/> to resolve them.
    /// </summary>
    public static class MenuIdentifierLoader
    {
        private static MenuIdentifierRegistry _registry;

#if UNITY_EDITOR
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void ResetStatics() => _registry = null;
#endif

        public static MenuIdentifier Load(string identifierName)
        {
            if (_registry == null)
                _registry = Resources.Load<MenuIdentifierRegistry>(nameof(MenuIdentifierRegistry));

            if (_registry == null)
            {
                CustomLogger.LogError("MenuIdentifierRegistry not found in Resources. " +
                               "Run Tools > Menus > Regenerate Menu Identifiers.", null);
                return null;
            }

            return _registry.TryGet(identifierName, out MenuIdentifier identifier) ? identifier : null;
        }
    }
}