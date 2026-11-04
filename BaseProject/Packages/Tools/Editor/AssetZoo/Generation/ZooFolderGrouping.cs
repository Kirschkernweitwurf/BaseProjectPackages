using System;

namespace Base.ToolsPackage.Editor.AssetZoo.Generation
{
    /// <summary>
    /// Finds the group of an asset from the folder it is in, which is how art is usually sorted already. A folder
    /// that only sorts by file type says nothing about what the asset is, so "Cafe/Models Cafe/Chair.fbx" belongs
    /// to "Cafe", right next to "Cafe/P_Chair.prefab".
    /// </summary>
    internal static class ZooFolderGrouping
    {
        private const string FallbackName = "Assets";

        private static readonly string[] FileTypeFolders =
        {
            "FBX",
            "Geometry",
            "Mesh",
            "Model",
            "Prefab"
        };

        /// <summary>Finds the name of the group an asset belongs in.</summary>
        /// <param name="assetPath">The asset's path in the project.</param>
        /// <returns>The nearest folder that says what the asset is.</returns>
        public static string GetGroupName(string assetPath)
        {
            string[] parts = assetPath.Split('/');

            for (int i = parts.Length - 2; i >= 0; i--)
            {
                if (!IsFileTypeFolder(parts[i]))
                    return parts[i];
            }

            return FallbackName;
        }

        private static bool IsFileTypeFolder(string folder)
        {
            foreach (string word in FileTypeFolders)
            {
                if (folder.StartsWith(word, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}