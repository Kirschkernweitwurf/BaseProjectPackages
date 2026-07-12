using System;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Path helpers shared by the folder and file path drawers.
    /// </summary>
    public static class PathUtility
    {
        /// <summary>Converts backslashes to forward slashes.</summary>
        public static string Normalize(string path) => path?.Replace("\\", "/");

        /// <summary>
        /// Returns a path relative to the project ("Assets/..."). Paths outside the project are
        /// returned normalized but absolute.
        /// </summary>
        public static string ToProjectRelative(string absolute)
        {
            string dataPath = Normalize(Application.dataPath);
            string normalized = Normalize(absolute);

            if (normalized == dataPath)
                return "Assets";

            if (normalized != null && normalized.StartsWith(dataPath + "/"))
                return "Assets" + normalized.Substring(dataPath.Length);

            return normalized;
        }

        /// <summary>
        /// Converts an asset path to a path relative to its Resources folder, without extension,
        /// ready for Resources.Load. Returns null when the asset is not under a Resources folder.
        /// </summary>
        public static string ToResourcesPath(string assetPath)
        {
            string normalized = Normalize(assetPath);
            if (string.IsNullOrEmpty(normalized))
                return null;

            const string marker = "/Resources/";
            int index = normalized.LastIndexOf(marker, StringComparison.Ordinal);
            if (index < 0)
                return null;

            string relative = normalized.Substring(index + marker.Length);
            int dot = relative.LastIndexOf('.');
            if (dot >= 0)
                relative = relative.Substring(0, dot);

            return relative;
        }
    }
}