using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.ToolsPackage.Editor.Shared
{
    /// <summary>
    /// Remembers the assets a scan was told to leave alone. Entries are stored by GUID in a
    /// per-project file under ProjectSettings, so dismissals survive rescans, renames and restarts
    /// and can be committed for the team.
    /// </summary>
    /// <remarks>
    /// One instance owns one file. A tool holds its own instance and exposes only the part of this
    /// API it actually uses, which keeps the file name in a single place per tool.
    /// <para>
    /// The set is read once and then kept in memory. A file changed from outside the editor, by a
    /// hand edit or by pulling a branch, is therefore not picked up until the next domain reload.
    /// </para>
    /// </remarks>
    internal sealed class GuidDismissStore
    {
        private const string LoadFailedFormat = "Could not read {0}: {1}. Starting with no dismissals.";
        private const string NullRangeFormat = "{0} was given no collection. Nothing was dismissed.";

        /// <summary>How many entries are currently dismissed.</summary>
        internal int Count => Guids.Count;

        private HashSet<string> Guids => _guids ??= Load();

        private readonly string _filePath;

        private HashSet<string> _guids;

        /// <summary>Creates a store over one project relative file.</summary>
        /// <param name="filePath">
        /// Project relative path the dismissals live at, for example
        /// <c>ProjectSettings/UnusedAssetsDismissed.json</c>.
        /// </param>
        internal GuidDismissStore(string filePath) => _filePath = filePath;

        /// <summary>True when the entry was dismissed.</summary>
        /// <param name="guid">GUID of the asset to test.</param>
        /// <returns>True when future scans should skip it.</returns>
        internal bool IsDismissed(string guid) => !string.IsNullOrEmpty(guid) && Guids.Contains(guid);

        /// <summary>Excludes the entry from future scans.</summary>
        /// <param name="guid">GUID of the asset to dismiss.</param>
        internal void Dismiss(string guid)
        {
            if (string.IsNullOrEmpty(guid))
                return;

            if (Guids.Add(guid))
                Save();
        }

        /// <summary>Excludes every given entry from future scans in one write.</summary>
        /// <param name="guids">GUIDs of the assets to dismiss. Empty entries are skipped.</param>
        internal void DismissRange(IEnumerable<string> guids)
        {
            if (guids == null)
            {
                CustomLogger.LogError(string.Format(NullRangeFormat, nameof(DismissRange)), null);
                return;
            }

            bool changed = false;

            foreach (string guid in guids)
            {
                if (!string.IsNullOrEmpty(guid) && Guids.Add(guid))
                    changed = true;
            }

            if (changed)
                Save();
        }

        /// <summary>Brings the entry back into future scans.</summary>
        /// <param name="guid">GUID of the asset to restore.</param>
        internal void Restore(string guid)
        {
            if (string.IsNullOrEmpty(guid))
                return;

            if (Guids.Remove(guid))
                Save();
        }

        /// <summary>Drops every dismissal.</summary>
        internal void Clear()
        {
            if (Guids.Count == 0)
                return;

            Guids.Clear();
            Save();
        }

        /// <summary>Snapshot of the dismissed GUIDs, safe to iterate while dismissing or restoring.</summary>
        /// <returns>A copy of the dismissed GUIDs.</returns>
        internal IReadOnlyList<string> GetAll() => Guids.ToList();

        private HashSet<string> Load()
        {
            if (!File.Exists(_filePath))
                return new HashSet<string>();

            try
            {
                GuidDismissFile file = JsonUtility.FromJson<GuidDismissFile>(File.ReadAllText(_filePath));

                return file?.guids != null
                    ? new HashSet<string>(file.guids)
                    : new HashSet<string>();
            }
            catch (Exception exception)
            {
                // Deliberately broad: a file this tool cannot read must not stop the tool from
                // running, and every failure has the same answer. It is still reported, because the
                // next write replaces the file and whatever was in it is gone at that point.
                CustomLogger.LogWarning(string.Format(LoadFailedFormat, _filePath, exception.Message), null);

                return new HashSet<string>();
            }
        }

        private void Save()
        {
            List<string> ordered = Guids.ToList();

            // The file is committed, so a stable order keeps a diff down to the entry that actually
            // changed instead of the whole list reshuffling with the set's iteration order.
            ordered.Sort(StringComparer.Ordinal);

            GuidDismissFile file = new()
            {
                guids = ordered
            };

            File.WriteAllText(_filePath, JsonUtility.ToJson(file, true));
        }
    }
}