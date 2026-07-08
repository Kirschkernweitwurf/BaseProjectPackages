#if UNITY_EDITOR
#if !BASE_PACKAGES_DEV
using UnityEditor;

namespace Base.CorePackage.MenuManaging.Identifier.Editor
{
    /// <summary>
    /// Watches for changes to <see cref="MenuIdentifier"/> assets
    /// and triggers regeneration of the accessor class and registry.
    /// </summary>
    internal class MenuIdentifierPostprocessor : AssetPostprocessor
    {
#region Unity Callbacks
        private static void OnPostprocessAllAssets(string[] imported, string[] deleted,
            string[] movedTo, string[] movedFrom)
        {
            bool affected =
                AnyIsMenuIdentifier(imported)
                || AnyIsMenuIdentifier(deleted)
                || AnyIsMenuIdentifier(movedTo)
                || AnyIsMenuIdentifier(movedFrom);

            if (affected)
                MenuIdentifierGenerator.Regenerate();
        }
#endregion

        private static bool AnyIsMenuIdentifier(string[] paths)
        {
            foreach (string path in paths)
            {
                if (!path.EndsWith(".asset"))
                    continue;

                if (AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(MenuIdentifier))
                    return true;
            }

            return false;
        }
    }
}
#endif
#endif