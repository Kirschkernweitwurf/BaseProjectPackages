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
    }
}
