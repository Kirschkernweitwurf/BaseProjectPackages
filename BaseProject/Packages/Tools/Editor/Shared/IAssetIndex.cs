using System.Collections.Generic;

namespace Base.ToolsPackage.Editor.Shared
{
    /// <summary>
    /// The questions a scanning tool asks about what is in the project. A short list rather than a
    /// mirror of <c>AssetDatabase</c>, because a tool that reads the layout and the files in it only
    /// needs to read the layout and the files in it.
    /// </summary>
    /// <remarks>
    /// A scanner calling <c>AssetDatabase</c> directly can only ever be run against the project it is
    /// running in, which means its rules cannot be covered: a test would be asserting against whatever
    /// happens to be in the Assets folder that day. Behind this, the same scanner can be handed a
    /// project layout that was written for one case.
    /// </remarks>
    public interface IAssetIndex
    {
        /// <summary>Whether a path points at a folder that exists.</summary>
        /// <param name="path">Asset path, for example <c>Assets/Art</c>.</param>
        /// <returns>True when the folder exists.</returns>
        bool IsValidFolder(string path);

        /// <summary>The folders directly inside the given one, not their children.</summary>
        /// <param name="path">Asset path of the folder to look inside.</param>
        /// <returns>The asset paths of the direct subfolders, empty when there are none.</returns>
        IReadOnlyList<string> GetSubFolders(string path);

        /// <summary>Every asset path in the project, folders and packages included.</summary>
        /// <returns>The asset paths, in no particular order.</returns>
        IReadOnlyList<string> GetAllAssetPaths();

        /// <summary>Every asset below the given folder that matches the filter, folders included.</summary>
        /// <param name="filter">
        /// Search filter in the syntax the project window uses, for example <c>t:Object</c>.
        /// </param>
        /// <param name="root">Asset path of the folder to search below.</param>
        /// <returns>The asset paths that matched, empty when none did.</returns>
        IReadOnlyList<string> FindAssetPaths(string filter, string root);

        /// <summary>The text of a script, an assembly definition or any other text based asset.</summary>
        /// <param name="path">Asset path of the file to read.</param>
        /// <returns>The file contents, or an empty string when the asset holds no text.</returns>
        string ReadText(string path);
    }
}