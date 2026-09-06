using System.IO;
using UnityEngine;

namespace Base.ToolsPackage.Editor.AssemblyGraph
{
    /// <summary>Resolves asset paths against the project root, since plain file IO cannot read one.</summary>
    internal static class ProjectPaths
    {
        /// <summary>Turns a path relative to the project root into an absolute one.</summary>
        /// <param name="assetPath">Path relative to the project root, such as a script or asmdef path.</param>
        /// <returns>The absolute path.</returns>
        internal static string ToAbsolute(string assetPath)
            => Path.Combine(Directory.GetParent(Application.dataPath).FullName, assetPath);
    }
}