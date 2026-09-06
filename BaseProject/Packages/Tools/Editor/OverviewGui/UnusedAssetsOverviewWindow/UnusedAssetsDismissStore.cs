using System.Collections.Generic;
using Base.ToolsPackage.Editor.Shared;

namespace Base.ToolsPackage.Editor.OverviewGui.UnusedAssetsOverviewWindow
{
    /// <summary>
    /// Remembers assets the user chose to keep. Stored by GUID in a per-project file under
    /// ProjectSettings, so dismissals survive rescans and restarts and can be committed for the team.
    /// </summary>
    /// <remarks>
    /// A named front for one <see cref="GuidDismissStore"/>, which owns the reading, writing and
    /// error handling. Only the part of that API this window uses is exposed, so the file name stays
    /// in one place and the window keeps calling the store it already knows.
    /// </remarks>
    internal static class UnusedAssetsDismissStore
    {
        private const string FilePath = "ProjectSettings/UnusedAssetsDismissed.json";

        /// <summary>How many assets are currently dismissed, shown beside the clear button.</summary>
        internal static int Count => Store.Count;

        private static readonly GuidDismissStore Store = new(FilePath);

        /// <summary>True when the asset was dismissed.</summary>
        /// <param name="guid">GUID of the asset to test.</param>
        /// <returns>True when future scans should skip it.</returns>
        internal static bool IsDismissed(string guid) => Store.IsDismissed(guid);

        /// <summary>Excludes the asset from future scans.</summary>
        /// <param name="guid">GUID of the asset to dismiss.</param>
        internal static void Dismiss(string guid) => Store.Dismiss(guid);

        /// <summary>Excludes every given asset from future scans in one write.</summary>
        /// <param name="guids">GUIDs of the assets to dismiss. Empty entries are skipped.</param>
        internal static void DismissRange(IEnumerable<string> guids) => Store.DismissRange(guids);

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