using System.IO;
using UnityEditor;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Base.ToolPackage.Editor.TodoOverview.Scanning
{
    /// <summary>
    /// Reads the text of a source file and finds the real file behind a project relative path.
    /// <para>
    /// Disk is tried first because it is far cheaper than loading an asset, and the detour over the
    /// package manager matters: a package installed from Git lives under a virtual <c>Packages</c> path
    /// whose file only exists somewhere in the package cache.
    /// </para>
    /// </summary>
    internal static class TodoSourceReader
    {
        private const char PathSeparator = '/';

        /// <summary>Reads the full text of a file, or an empty string when it cannot be read.</summary>
        /// <param name="assetPath">Project relative path of the file.</param>
        /// <returns>The text of the file.</returns>
        internal static string Read(string assetPath)
        {
            string fullPath = ResolveFullPath(assetPath);

            if (!string.IsNullOrEmpty(fullPath))
                try
                {
                    return File.ReadAllText(fullPath);
                }
                catch (IOException)
                {
                    return string.Empty;
                }

            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);

            if (script != null)
                return script.text;

            TextAsset text = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);

            return text == null
                ? string.Empty
                : text.text;
        }

        /// <summary>Finds the file on disk a project relative path points at.</summary>
        /// <param name="assetPath">Project relative path of the file.</param>
        /// <returns>The absolute path, or an empty string when there is no file behind it.</returns>
        internal static string ResolveFullPath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return string.Empty;

            if (File.Exists(assetPath))
                return Path.GetFullPath(assetPath);

            PackageInfo package = PackageInfo.FindForAssetPath(assetPath);

            if (package == null || string.IsNullOrEmpty(package.assetPath))
                return string.Empty;

            string relative = assetPath[package.assetPath.Length..].TrimStart(PathSeparator);
            string resolved = Path.Combine(package.resolvedPath, relative);

            return File.Exists(resolved)
                ? resolved
                : string.Empty;
        }
    }
}