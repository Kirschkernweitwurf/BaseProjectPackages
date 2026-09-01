using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Base.ToolPackage.Editor.NamingConventions.Data
{
    /// <summary>
    /// Remembers assets the user chose to exclude from the naming scan. Stored by GUID in a
    /// per-project file under ProjectSettings, so dismissals survive rescans, renames and restarts
    /// and can be committed for the team.
    /// </summary>
    internal static class AssetNamingDismissStore
    {
        private const string FilePath = "ProjectSettings/AssetNamingDismissed.json";

        private static HashSet<string> Guids => _guids ??= Load();

        private static HashSet<string> _guids;

        /// <summary>True when the asset was dismissed.</summary>
        public static bool IsDismissed(string guid) => !string.IsNullOrEmpty(guid) && Guids.Contains(guid);

        /// <summary>Excludes the asset from future scans.</summary>
        public static void Dismiss(string guid)
        {
            if (string.IsNullOrEmpty(guid))
                return;

            if (Guids.Add(guid))
                Save();
        }

        /// <summary>Brings the asset back into future scans.</summary>
        public static void Restore(string guid)
        {
            if (string.IsNullOrEmpty(guid))
                return;

            if (Guids.Remove(guid))
                Save();
        }

        /// <summary>Drops every dismissal.</summary>
        public static void Clear()
        {
            if (Guids.Count == 0)
                return;

            Guids.Clear();
            Save();
        }

        /// <summary>Snapshot of the dismissed GUIDs, safe to iterate while dismissing or restoring.</summary>
        public static IReadOnlyList<string> GetAll() => Guids.ToList();

        private static HashSet<string> Load()
        {
            if (!File.Exists(FilePath))
                return new HashSet<string>();

            try
            {
                Data data = JsonUtility.FromJson<Data>(File.ReadAllText(FilePath));

                return data?.guids != null
                    ? new HashSet<string>(data.guids)
                    : new HashSet<string>();
            }
            catch
            {
                // A broken file only loses the dismissals, so starting fresh beats blocking the tool.
                return new HashSet<string>();
            }
        }

        private static void Save()
        {
            Data data = new()
            {
                guids = Guids.ToList()
            };

            File.WriteAllText(FilePath, JsonUtility.ToJson(data, true));
        }

        [Serializable]
        private sealed class Data
        {
            /// <summary>
            /// The dismissed assets. A list rather than a set because JsonUtility cannot serialize one;
            /// it becomes a set again on load.
            /// </summary>
            public List<string> guids = new();
        }
    }
}