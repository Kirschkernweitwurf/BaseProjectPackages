#if UNITY_EDITOR
using Base.ToolPackage.Identification;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.Identification
{
    /// <summary>
    /// Runs after assets are imported (including newly created or duplicated). <br/>
    /// Makes sure each imported <see cref="IUniquelyIdentifiable"/> gets a valid UniqueId. <br/>
    /// No per-object boilerplate required.
    /// </summary>
    internal class UniqueIdAssetPostprocessor : AssetPostprocessor
    {
#region Unity Callbacks
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (!UniqueIdSettings.Enabled)
                return;

            foreach (string path in importedAssets)
            {
                ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (asset is IUniquelyIdentifiable)
                    UniqueIdAssigner.EnsureForAsset(asset);
            }
        }
#endregion
    }
}
#endif