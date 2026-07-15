#if UNITY_EDITOR
#if !BASE_PACKAGES_DEV
using UnityEditor;

namespace Base.CorePackage.MenuManaging.Identifier.Editor
{
    /// <summary>
    /// Detects deletion of <see cref="MenuIdentifier"/> assets and queues a regeneration.
    /// </summary>
    /// <remarks>
    /// Deletion cannot be detected in <see cref="AssetPostprocessor"/>, because by then the asset
    /// is gone and its type can no longer be resolved. This callback runs before the delete,
    /// while the asset still exists, so the type check works.
    /// </remarks>
    internal class MenuIdentifierDeleteProcessor : AssetModificationProcessor
    {
#region Unity Callbacks
        private static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions options)
        {
            if (IsMenuIdentifier(path) || FolderContainsMenuIdentifier(path))
                MenuIdentifierGenerator.ScheduleRegenerate();

            // Let Unity perform the actual deletion. The queued regeneration runs afterwards
            // and rebuilds the registry from the assets that are left.
            return AssetDeleteResult.DidNotDelete;
        }
#endregion

        private static bool IsMenuIdentifier(string path) =>
            path.EndsWith(".asset") && AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(MenuIdentifier);

        private static bool FolderContainsMenuIdentifier(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
                return false;

            string[] guids = AssetDatabase.FindAssets($"t:{nameof(MenuIdentifier)}", new[] { path });
            return guids.Length > 0;
        }
    }
}
#endif
#endif
