using Base.CorePackage.GameActions;
using UnityEditor;

namespace Base.CorePackage.Editor.GameActions
{
    /// <summary>
    /// Invalidates <see cref="GameActionRegistry"/> whenever assets are imported, so the
    /// next access rebuilds the cache from Unity's up-to-date <c>TypeCache</c>.
    /// </summary>
    public sealed class GameActionRegistryPostprocessor : AssetPostprocessor
    {
#region Unity Callbacks
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths) => GameActionRegistry.Invalidate();
#endregion
    }
}