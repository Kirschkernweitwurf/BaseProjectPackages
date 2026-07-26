using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace Base.ToolPackage.Editor.NamingConventions.Data
{
    /// <summary>
    /// Remembers every rename the tool applied. Stored in a per-project file under
    /// ProjectSettings, so the history survives restarts and can be committed for the team.
    /// Newest entries come first, and the list is capped so the file cannot grow forever.
    /// </summary>
    public static class AssetNamingHistoryStore
    {
        private const string FilePath = "ProjectSettings/AssetNamingHistory.json";
        private const int MaxEntries = 200;

        // The general short pattern follows the user's locale, e.g. 27.08.2026 14:30 or 8/27/2026 2:30 PM.
        private const string TimeFormat = "g";

        private static List<AssetNamingHistoryEntry> _entries;

        /// <summary>Number of remembered renames.</summary>
        public static int Count => Entries.Count;

        /// <summary>Remembered renames, newest first.</summary>
        public static IReadOnlyList<AssetNamingHistoryEntry> Entries => _entries ??= Load();

        /// <summary>Remembers one applied rename.</summary>
        public static void Add(string oldName, string newName, string assetPath)
        {
            if (string.IsNullOrEmpty(oldName)
                || string.IsNullOrEmpty(newName))
                return;

            List<AssetNamingHistoryEntry> entries = (List<AssetNamingHistoryEntry>)Entries;

            entries.Insert(0, new AssetNamingHistoryEntry
            {
                oldName = oldName,
                newName = newName,
                assetPath = assetPath ?? string.Empty,
                time = DateTime.Now.ToString(TimeFormat, CultureInfo.CurrentCulture)
            });

            if (entries.Count > MaxEntries)
                entries.RemoveRange(MaxEntries, entries.Count - MaxEntries);

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
            public List<AssetNamingHistoryEntry> entries = new();
        }
    }
}
