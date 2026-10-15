using System;
using System.Collections.Generic;
using UnityEditor;

namespace Base.DataHubPackage.Editor
{
    /// <summary>
    /// Path and type of every ScriptableObject asset under Assets, read without loading a single one.
    /// The tree counts, the tables, the collection lookup and the reference cleanup all read from this
    /// one list, so the project is only searched once per change.
    /// </summary>
    internal sealed class DataAssetIndex
    {
        private const string Filter = "t:ScriptableObject";

        private static readonly string[] SearchFolders = { "Assets" };

        private readonly List<DataAssetEntry> _entries = new();

        /// <summary>Every indexed asset.</summary>
        internal IReadOnlyList<DataAssetEntry> Entries => _entries;

        /// <summary>Searches the project again.</summary>
        internal void Rebuild()
        {
            _entries.Clear();

            foreach (string guid in AssetDatabase.FindAssets(Filter, SearchFolders))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Type type = AssetDatabase.GetMainAssetTypeAtPath(path);

                // A missing script leaves an asset without a type, and it cannot be listed under any.
                if (type == null)
                    continue;

                _entries.Add(new DataAssetEntry(path, type));
            }
        }

        /// <summary>Appends the path of every asset of <paramref name="dataType"/> or a type derived from it.</summary>
        /// <param name="dataType">The type to list.</param>
        /// <param name="paths">The list the paths are appended to.</param>
        internal void CollectPaths(Type dataType, List<string> paths)
        {
            foreach (DataAssetEntry entry in _entries)
            {
                if (dataType.IsAssignableFrom(entry.Type))
                    paths.Add(entry.Path);
            }
        }

        /// <summary>How many assets are of <paramref name="dataType"/> or a type derived from it.</summary>
        /// <param name="dataType">The type to count.</param>
        /// <returns>The number of assets.</returns>
        internal int Count(Type dataType)
        {
            int count = 0;

            foreach (DataAssetEntry entry in _entries)
            {
                if (dataType.IsAssignableFrom(entry.Type))
                    count++;
            }

            return count;
        }
    }
}