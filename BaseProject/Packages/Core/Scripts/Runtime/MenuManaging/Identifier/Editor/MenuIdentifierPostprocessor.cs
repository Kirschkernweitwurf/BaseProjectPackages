#if UNITY_EDITOR
#if !BASE_PACKAGES_DEV
using UnityEditor;

namespace Base.CorePackage.MenuManaging.Identifier.Editor
{
    /// <summary>
    /// Watches for created and moved <see cref="MenuIdentifier"/> assets
    /// and queues regeneration of the accessor class and registry.
    /// </summary>
    /// <remarks>
    /// Deletion is handled by <see cref="MenuIdentifierDeleteProcessor"/>. It cannot be handled here,
    /// because a deleted or moved-from path no longer resolves to a type.
    /// </remarks>
    internal class MenuIdentifierPostprocessor : AssetPostprocessor
    {
#region Unity Callbacks
        private static void OnPostprocessAllAssets(string[] imported, string[] deleted,
            string[] movedTo, string[] movedFrom)
        {
            if (AnyIsMenuIdentifier(imported) || AnyIsMenuIdentifier(movedTo))
                MenuIdentifierGenerator.ScheduleRegenerate();
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
