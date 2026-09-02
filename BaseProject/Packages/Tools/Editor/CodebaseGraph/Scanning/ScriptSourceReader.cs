using System.IO;
using UnityEditor;

namespace Base.ToolsPackage.Editor.CodebaseGraph.Scanning
{
    /// <summary>
    /// Reads script source. Disk is tried first because it is far cheaper than asking the asset
    /// database to hand over a MonoScript and its text, and the fallback matters: a package installed
    /// from Git lives under a virtual Packages path that never exists as a real file.
    /// </summary>
    internal static class ScriptSourceReader
    {
        /// <summary>Returns the source of a script asset, or an empty string when it cannot be read.</summary>
        /// <param name="assetPath">Asset path of the script.</param>
        /// <returns>The source text.</returns>
        internal static string Read(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return string.Empty;

            string fromDisk = ReadFromDisk(assetPath);
            if (!string.IsNullOrEmpty(fromDisk))
                return fromDisk;

            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);

            return script == null
                ? string.Empty
                : script.text;
        }

        private static string ReadFromDisk(string assetPath)
        {
            try
            {
                return File.Exists(assetPath)
                    ? File.ReadAllText(assetPath)
                    : string.Empty;
            }
            catch (IOException)
            {
                return string.Empty;
            }
        }
    }
}