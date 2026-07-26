#if UNITY_EDITOR
#if !BASE_PACKAGES_DEV
using System;
using UnityEditor;

namespace Base.CorePackage.MenuManaging.Identifier.Editor
{
    /// <summary>
    /// Shared asset path checks for <see cref="MenuIdentifier"/> assets, used by the asset callbacks.
    /// </summary>
    internal static class MenuIdentifierAssets
    {
        private const string AssetExtension = ".asset";

        /// <summary>Returns true when the path points at a <see cref="MenuIdentifier"/> asset.</summary>
        internal static bool IsMenuIdentifier(string path)
        {
            if (!path.EndsWith(AssetExtension, StringComparison.OrdinalIgnoreCase))
                return false;

            return AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(MenuIdentifier);
        }

        /// <summary>Returns true when any of the paths points at a <see cref="MenuIdentifier"/> asset.</summary>
        internal static bool AnyIsMenuIdentifier(string[] paths)
        {
            foreach (string path in paths)
            {
                if (IsMenuIdentifier(path))
                    return true;
            }

            return false;
        }

        /// <summary>Returns true when the folder holds at least one <see cref="MenuIdentifier"/> asset.</summary>
        internal static bool FolderContainsMenuIdentifier(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
                return false;

            return AssetDatabase.FindAssets($"t:{nameof(MenuIdentifier)}", new[] { path }).Length > 0;
        }
    }
}
#endif
#endif