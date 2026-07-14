#if UNITY_EDITOR
#if !BASE_PACKAGES_DEV
using Base.CorePackage.MenuManaging.Identifier.Editor;
using UnityEditor;

namespace Base.CorePackage.MenuManaging.Identifier
{
    /// <summary>
    /// Watches for changes to <see cref="MenuIdentifier"/> assets
    /// and triggers regeneration of the accessor class and registry.
    /// </summary>
    internal class MenuIdentifierPostprocessor : AssetPostprocessor
    {
        private static bool _pending;

#region Unity Callbacks
        private static void OnPostprocessAllAssets(string[] imported, string[] deleted,
            string[] movedTo, string[] movedFrom)
        {
            bool affected =
                AnyIsMenuIdentifier(imported)
                || AnyIsMenuIdentifier(deleted)
                || AnyIsMenuIdentifier(movedTo)
                || AnyIsMenuIdentifier(movedFrom);

            if (!affected || _pending)
                return;

            // Creating and refreshing assets is not safe inside the postprocess callback,
            // so defer to the next editor tick. Multiple imports coalesce into one run.
            _pending = true;
            EditorApplication.delayCall += RegenerateDeferred;
        }
#endregion

        private static void RegenerateDeferred()
        {
            _pending = false;
            MenuIdentifierGenerator.Regenerate();
        }

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