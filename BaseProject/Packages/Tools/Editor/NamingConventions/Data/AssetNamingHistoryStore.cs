using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Base.ToolsPackage.Editor.NamingConventions.Data
{
    /// <summary>
    /// Remembers every rename, dismiss and restore the tool applied. Stored in a per-project file
    /// under ProjectSettings, so the history survives restarts and can be committed for the team.
    /// Newest entries come first, and the list is capped so the file cannot grow forever.
    /// </summary>
    internal static class AssetNamingHistoryStore
    {
        private const string FilePath = "ProjectSettings/AssetNamingHistory.json";
        private const int MaxEntries = 200;

        // The general short pattern follows the local culture, e.g. 27.08.2026 14:30.
        private const string TimeFormat = "g";

        /// <summary>Number of remembered actions.</summary>
        public static int Count => Entries.Count;

        /// <summary>Remembered actions, newest first.</summary>
        public static IReadOnlyList<AssetNamingHistoryEntry> Entries => _entries ??= Load();

        private static List<AssetNamingHistoryEntry> _entries;

        /// <summary>Remembers one applied rename.</summary>
        public static void AddRename(string oldName, string newName, string assetPath, string guid)
            => Add(EAssetNamingAction.Renamed, oldName, newName, assetPath, guid);

        /// <summary>Remembers that an asset was taken out of the scan.</summary>
        public static void AddDismiss(string name, string assetPath, string guid)
            => Add(EAssetNamingAction.Dismissed, name, string.Empty, assetPath, guid);

        /// <summary>Remembers that an asset was brought back into the scan.</summary>
        public static void AddRestore(string name, string assetPath, string guid)
            => Add(EAssetNamingAction.Restored, name, string.Empty, assetPath, guid);

        /// <summary>Forgets a single entry, used after it was undone.</summary>
        public static void Remove(AssetNamingHistoryEntry entry)
        {
            if (entry == null)
                return;

            if (((List<AssetNamingHistoryEntry>)Entries).Remove(entry))
                Save();
        }

        /// <summary>Drops the whole history.</summary>
        public static void Clear()
        {
            if (Entries.Count == 0)
                return;

            ((List<AssetNamingHistoryEntry>)Entries).Clear();
            Save();
        }

        /// <summary>
        /// GUID behind a history entry. Older entries only stored a path, and a path goes stale as
        /// soon as the asset is renamed again, which is why the GUID is the one that counts.
        /// </summary>
        /// <param name="entry">The entry to resolve.</param>
        /// <returns>The stored GUID, or the one behind the stored path.</returns>
        public static string GuidOf(AssetNamingHistoryEntry entry)
        {
            if (!string.IsNullOrEmpty(entry.guid))
                return entry.guid;

            return AssetDatabase.AssetPathToGUID(entry.assetPath);
        }

        /// <summary>Current path of a history entry, resolved through its GUID.</summary>
        /// <param name="entry">The entry to resolve.</param>
        /// <returns>The current path, or the stored one when the asset is gone.</returns>
        public static string PathOf(AssetNamingHistoryEntry entry)
        {
            string path = AssetDatabase.GUIDToAssetPath(GuidOf(entry));

            return string.IsNullOrEmpty(path)
                ? entry.assetPath
                : path;
        }

        private static void Add(EAssetNamingAction action, string oldName, string newName, string assetPath,
            string guid)
        {
            if (string.IsNullOrEmpty(oldName))
                return;

            List<AssetNamingHistoryEntry> entries = (List<AssetNamingHistoryEntry>)Entries;

            entries.Insert(0, new AssetNamingHistoryEntry
            {
                action = action,
                oldName = oldName,
                newName = newName ?? string.Empty,
                assetPath = assetPath ?? string.Empty,
                guid = guid ?? string.Empty,
                time = DateTime.Now.ToString(TimeFormat, CultureInfo.CurrentCulture)
            });

            if (entries.Count > MaxEntries)
                entries.RemoveRange(MaxEntries, entries.Count - MaxEntries);

            Save();
        }

        private static List<AssetNamingHistoryEntry> Load()
        {
            if (!File.Exists(FilePath))
                return new List<AssetNamingHistoryEntry>();

            try
            {
                Data data = JsonUtility.FromJson<Data>(File.ReadAllText(FilePath));

                return data?.entries ?? new List<AssetNamingHistoryEntry>();
            }
            catch
            {
                // A broken file only loses the history, so starting fresh beats blocking the tool.
                return new List<AssetNamingHistoryEntry>();
            }
        }

        private static void Save()
        {
            Data data = new()
            {
                entries = (List<AssetNamingHistoryEntry>)Entries
            };

            File.WriteAllText(FilePath, JsonUtility.ToJson(data, true));
        }

        [Serializable]
        private sealed class Data
        {
            /// <summary>Every recorded rename, newest last.</summary>
            public List<AssetNamingHistoryEntry> entries = new();
        }
    }
}