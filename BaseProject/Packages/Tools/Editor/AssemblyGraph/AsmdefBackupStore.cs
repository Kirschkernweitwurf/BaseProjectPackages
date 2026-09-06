using System.IO;
using UnityEditor;

namespace Base.ToolsPackage.Editor.AssemblyGraph
{
    /// <summary>
    /// Keeps the untouched text of the last asmdef this tool rewrote, so a removal that turns out to
    /// break the compile can be put back. It lives in session state because the rewrite triggers a
    /// domain reload, and the reload is what has to happen before the mistake is visible at all.
    /// </summary>
    internal static class AsmdefBackupStore
    {
        private const string PathKey = "Base.ToolsPackage.AssemblyGraph.BackupPath";
        private const string TextKey = "Base.ToolsPackage.AssemblyGraph.BackupText";

        /// <summary>Asset path of the stored file, or an empty string when nothing is stored.</summary>
        internal static string BackupPath => SessionState.GetString(PathKey, string.Empty);

        /// <summary>True when a rewrite from this session can still be undone.</summary>
        internal static bool HasBackup => !string.IsNullOrEmpty(BackupPath);

        /// <summary>Remembers the text of a file about to be rewritten, replacing any earlier one.</summary>
        /// <param name="assetPath">Asset path of the file.</param>
        /// <param name="originalText">The file text as it stands before the rewrite.</param>
        internal static void Store(string assetPath, string originalText)
        {
            SessionState.SetString(PathKey, assetPath);
            SessionState.SetString(TextKey, originalText);
        }

        /// <summary>Writes the stored text back and forgets it.</summary>
        /// <returns>The restored asset path, or null when there was nothing to restore.</returns>
        internal static string Restore()
        {
            string assetPath = BackupPath;
            if (string.IsNullOrEmpty(assetPath))
                return null;

            string text = SessionState.GetString(TextKey, string.Empty);
            Clear();

            File.WriteAllText(ProjectPaths.ToAbsolute(assetPath), text);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            return assetPath;
        }

        /// <summary>Drops the stored file.</summary>
        internal static void Clear()
        {
            SessionState.EraseString(PathKey);
            SessionState.EraseString(TextKey);
        }
    }
}