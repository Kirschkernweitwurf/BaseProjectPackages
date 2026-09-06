using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.ToolsPackage.Editor.Shared
{
    /// <summary>
    /// The live project, read through <see cref="AssetDatabase"/>. This is what every tool runs
    /// against outside a test.
    /// </summary>
    public sealed class AssetDatabaseIndex : IAssetIndex
    {
        /// <summary>The one instance tools use, since it holds nothing and answers for the whole project.</summary>
        public static readonly AssetDatabaseIndex Default = new();

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

        /// <summary>
        /// The text of a text based asset, read through the asset system rather than off disk, because
        /// an asset path only resolves to a real file for an embedded package.
        /// </summary>
        /// <param name="path">Asset path of the file to read.</param>
        /// <returns>The file contents, or an empty string when the asset holds no text.</returns>
        public string ReadText(string path)
        {
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);

            return asset switch
            {
                MonoScript script => script.text,
                TextAsset text => text.text,
                _ => string.Empty
            };
        }
    }
}