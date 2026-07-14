using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Base.UnusedScriptsPackage
{
    /// <summary>
    /// Remembers scripts the user chose to keep. Stored by GUID in a per-project file under
    /// ProjectSettings, so dismissals survive rescans and restarts and can be committed for the team.
    /// </summary>
    public static class UnusedScriptsDismissStore
    {
        private const string FilePath = "ProjectSettings/UnusedScriptsDismissed.json";

        private static HashSet<string> _guids;

        [Serializable]
        private sealed class Data
        {
            public List<string> guids = new List<string>();
        }

        private static HashSet<string> Guids => _guids ??= Load();

        public static int Count => Guids.Count;

        public static bool IsDismissed(string guid)
        {
            return !string.IsNullOrEmpty(guid) && Guids.Contains(guid);
        }

        public static void Dismiss(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return;
            }

            if (Guids.Add(guid))
            {
                Save();
            }
        }

        public static void DismissRange(IEnumerable<string> guids)
        {
            bool changed = false;

            foreach (string guid in guids)
            {
                if (!string.IsNullOrEmpty(guid) && Guids.Add(guid))
                {
                    changed = true;
                }
            }

            if (changed)
            {
                Save();
            }
        }

        public static void Restore(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return;
            }

            if (Guids.Remove(guid))
            {
                Save();
            }
        }

        public static void Clear()
        {
            if (Guids.Count == 0)
            {
                return;
            }

            Guids.Clear();
            Save();
        }

        /// <summary>Snapshot of the dismissed GUIDs, safe to iterate while dismissing or restoring.</summary>
        public static IReadOnlyList<string> GetAll()
        {
            return Guids.ToList();
        }

        private static HashSet<string> Load()
        {
            if (!File.Exists(FilePath))
            {
                return new HashSet<string>();
            }

            try
            {
                Data data = JsonUtility.FromJson<Data>(File.ReadAllText(FilePath));
                return data?.guids != null ? new HashSet<string>(data.guids) : new HashSet<string>();
            }
            catch
            {
                return new HashSet<string>();
            }
        }

        private static void Save()
        {
            Data data = new Data { guids = Guids.ToList() };
            File.WriteAllText(FilePath, JsonUtility.ToJson(data, true));
        }
    }
}
