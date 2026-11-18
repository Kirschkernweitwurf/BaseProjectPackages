using System;
using System.Linq;
using TMPro;
using UnityEditor;

namespace Base.TextPackage.Editor
{
    /// <summary>
    /// Tells the Text Fit window and other checks that listen when an asset they read has changed: string tables, text
    /// styles, prefabs, scenes and other assets. Font assets are left out, because the check itself fills and clears
    /// dynamic fonts and would otherwise start itself again.
    /// </summary>
    public sealed class TextFitWatcher : AssetPostprocessor
    {
        private static readonly string[] WatchedExtensions = { ".asset", ".prefab", ".unity" };

        /// <summary>Raised after an import, move or delete that can change what the check finds.</summary>
        public static event Action Changed;

#region Unity Callbacks
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (importedAssets.Concat(deletedAssets).Concat(movedAssets).Any(IsWatched))
                Changed?.Invoke();
        }
#endregion

        private static bool IsWatched(string path) => WatchedExtensions.Any(path.EndsWith)
            && AssetDatabase.GetMainAssetTypeAtPath(path) != typeof(TMP_FontAsset);
    }
}