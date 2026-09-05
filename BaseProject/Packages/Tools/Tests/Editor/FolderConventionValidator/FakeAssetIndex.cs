using System;
using System.Collections.Generic;
using Base.ToolsPackage.Editor.Shared;

namespace Base.ToolsPackage.Editor.Tests
{
    /// <summary>
    /// A project layout that exists only for the test that wrote it. Folders and assets are handed in
    /// as paths, so a scanner can be pointed at exactly the case under test instead of at whatever
    /// happens to be in the Assets folder.
    /// </summary>
    internal sealed class FakeAssetIndex : IAssetIndex
    {
        private const char PathSeparator = '/';

        private readonly HashSet<string> _folders = new(StringComparer.Ordinal);
        private readonly List<string> _assets = new();

        /// <summary>Adds a folder and every folder above it, the way a real path implies its parents.</summary>
        /// <param name="path">Asset path of the folder, for example <c>Assets/Art/Textures</c>.</param>
        /// <returns>The same index, so a layout reads as one statement.</returns>
        internal FakeAssetIndex WithFolder(string path)
        {
            string[] segments = path.Split(PathSeparator, StringSplitOptions.RemoveEmptyEntries);
            string current = string.Empty;

            foreach (string segment in segments)
            {
                current = current.Length == 0
                    ? segment
                    : current + PathSeparator + segment;

                _folders.Add(current);
            }

            return this;
        }

        /// <summary>Adds a file, without implying the folders around it.</summary>
        /// <param name="path">Asset path of the file, for example <c>Assets/Loose.png</c>.</param>
        /// <returns>The same index, so a layout reads as one statement.</returns>
        internal FakeAssetIndex WithAsset(string path)
        {
            _assets.Add(path);

            return this;
        }

        /// <inheritdoc/>
        public bool IsValidFolder(string path) => _folders.Contains(path);

        /// <inheritdoc/>
        public IReadOnlyList<string> GetSubFolders(string path)
        {
            List<string> children = new();
            string prefix = path + PathSeparator;

            foreach (string folder in _folders)
            {
                if (IsDirectChild(folder, prefix))
                    children.Add(folder);
            }

            children.Sort(StringComparer.Ordinal);

            return children;
        }

        /// <inheritdoc/>
        public IReadOnlyList<string> GetAllAssetPaths()
        {
            List<string> all = new(_assets);
            all.AddRange(_folders);

            return all;
        }

        /// <summary>
        /// Every asset and folder below the root. The filter is ignored on purpose: the scanner only
        /// ever passes one, and honoring it would mean reimplementing the project window's syntax.
        /// </summary>
        /// <param name="filter">Ignored.</param>
        /// <param name="root">Asset path of the folder to search below.</param>
        /// <returns>The asset paths below the root.</returns>
        public IReadOnlyList<string> FindAssetPaths(string filter, string root)
        {
            List<string> found = new();
            string prefix = root + PathSeparator;

            foreach (string asset in _assets)
            {
                if (asset.StartsWith(prefix, StringComparison.Ordinal))
                    found.Add(asset);
            }

            foreach (string folder in _folders)
            {
                if (folder.StartsWith(prefix, StringComparison.Ordinal))
                    found.Add(folder);
            }

            return found;
        }

        /// <summary>Whether a path sits directly under the prefix rather than deeper below it.</summary>
        private static bool IsDirectChild(string path, string prefix)
            => path.StartsWith(prefix, StringComparison.Ordinal)
                && path.IndexOf(PathSeparator, prefix.Length) < 0;
    }
}