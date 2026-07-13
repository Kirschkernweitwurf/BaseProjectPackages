using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Base.ToolPackage.Editor.UnusedAssetsOverviewWindow
{
    /// <summary>
    /// Remembers assets the user chose to keep. Stored by GUID in a per-project file under
    /// ProjectSettings, so dismissals survive rescans and restarts and can be committed for the team.
    /// </summary>
    public static class UnusedAssetsDismissStore
    {
        private const string FilePath = "ProjectSettings/UnusedAssetsDismissed.json";

        public static int Count => Guids.Count;

        private static HashSet<string> Guids => _guids ??= Load();

        private static HashSet<string> _guids;

        public static bool IsDismissed(string guid) => !string.IsNullOrEmpty(guid) && Guids.Contains(guid);

        public static void Dismiss(string guid)
        {
            if (string.IsNullOrEmpty(guid))
                return;

            if (Guids.Add(guid))
                Save();
        }

        public static void DismissRange(IEnumerable<string> guids)
        {
            bool changed = false;

            foreach (string guid in guids)
            {
                if (!string.IsNullOrEmpty(guid) && Guids.Add(guid))
                    changed = true;
            }

            if (changed)
                Save();
        }

        public static void Clear()
        {
            if (Guids.Count == 0)
                return;

            Guids.Clear();
            Save();
        }

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
            public List<string> guids = new();
        }
    }
}