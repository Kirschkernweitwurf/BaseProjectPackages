using System.Collections.Generic;
using Base.ToolsPackage.Editor.Shared;

namespace Base.ToolsPackage.Editor.NamingConventions.Data
{
    /// <summary>
    /// Remembers assets the user chose to exclude from the naming scan. Stored by GUID in a
    /// per-project file under ProjectSettings, so dismissals survive rescans, renames and restarts
    /// and can be committed for the team.
    /// </summary>
    /// <remarks>
    /// A named front for one <see cref="GuidDismissStore"/>, which owns the reading, writing and
    /// error handling. Only the part of that API this window uses is exposed, so the file name stays
    /// in one place and the window keeps calling the store it already knows.
    /// </remarks>
    internal static class AssetNamingDismissStore
    {
        private const string FilePath = "ProjectSettings/AssetNamingDismissed.json";

        private static readonly GuidDismissStore Store = new(FilePath);

        /// <summary>True when the asset was dismissed.</summary>
        /// <param name="guid">GUID of the asset to test.</param>
        /// <returns>True when future scans should skip it.</returns>
        internal static bool IsDismissed(string guid) => Store.IsDismissed(guid);

        /// <summary>Excludes the asset from future scans.</summary>
        /// <param name="guid">GUID of the asset to dismiss.</param>
        internal static void Dismiss(string guid) => Store.Dismiss(guid);

        /// <summary>Brings the asset back into future scans.</summary>
        /// <param name="guid">GUID of the asset to restore.</param>
        internal static void Restore(string guid) => Store.Restore(guid);

        /// <summary>Drops every dismissal.</summary>
        internal static void Clear() => Store.Clear();

        /// <summary>Snapshot of the dismissed GUIDs, safe to iterate while dismissing or restoring.</summary>
        /// <returns>A copy of the dismissed GUIDs.</returns>
        internal static IReadOnlyList<string> GetAll() => Store.GetAll();
    }
}