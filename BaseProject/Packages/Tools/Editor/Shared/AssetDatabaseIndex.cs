using System;
using System.Collections.Generic;
using UnityEditor;

namespace Base.ToolsPackage.Editor.Shared
{
    /// <summary>
    /// The live project, read through <see cref="AssetDatabase"/>. This is what every tool runs
    /// against outside a test.
    /// </summary>
    internal sealed class AssetDatabaseIndex : IAssetIndex
    {
        /// <summary>The one instance tools use, since it holds nothing and answers for the whole project.</summary>
        internal static readonly AssetDatabaseIndex Default = new();

        private static readonly string[] NoPaths = Array.Empty<string>();

        private AssetDatabaseIndex() { }

        /// <inheritdoc/>
        public bool IsValidFolder(string path) => AssetDatabase.IsValidFolder(path);

        /// <inheritdoc/>
        public IReadOnlyList<string> GetSubFolders(string path) => AssetDatabase.GetSubFolders(path);

        /// <inheritdoc/>
        public IReadOnlyList<string> GetAllAssetPaths() => AssetDatabase.GetAllAssetPaths();

        /// <inheritdoc/>
        public IReadOnlyList<string> FindAssetPaths(string filter, string root)
        {
            string[] guids = AssetDatabase.FindAssets(filter, new[]
            {
                root
            });

            if (guids.Length == 0)
                return NoPaths;

            string[] paths = new string[guids.Length];

            for (int index = 0; index < guids.Length; index++)
                paths[index] = AssetDatabase.GUIDToAssetPath(guids[index]);

            return paths;
        }
    }
}