using System;
using System.Collections.Generic;
using System.IO;
using Base.ToolPackage.Editor.AudioRules.Model;
using UnityEngine;

namespace Base.ToolPackage.Editor.AudioRules.Scanning
{
    /// <summary>
    /// Remembers what the analyzer measured, so a second deep pass only reads the files that
    /// actually changed. Lives under Library because it is derived data: deleting it costs one
    /// rescan and nothing else.
    /// </summary>
    internal static class AudioAnalysisCache
    {
        private const string FilePath = "Library/AudioRulesAnalysis.json";

        private static Dictionary<string, Entry> Entries => _entries ??= Load();

        private static Dictionary<string, Entry> _entries;
        private static bool _isDirty;

        /// <summary>Looks up a clip, but only accepts the entry when the file is unchanged.</summary>
        /// <param name="guid">GUID of the clip.</param>
        /// <param name="fileSize">Current size of the source file.</param>
        /// <param name="writeTicks">Current write time of the source file.</param>
        /// <param name="analysis">The cached measurements, or null.</param>
        /// <returns>True when a usable entry was found.</returns>
        public static bool TryGet(string guid, long fileSize, long writeTicks, out AudioClipAnalysis analysis)
        {
            analysis = null;

            if (!Entries.TryGetValue(guid, out Entry entry))
                return false;

            if (entry.fileSize != fileSize
                || entry.writeTicks != writeTicks)
                return false;

            analysis = entry.ToAnalysis();

            return true;
        }

        /// <summary>Stores what the analyzer measured for one clip.</summary>
        /// <param name="guid">GUID of the clip.</param>
        /// <param name="fileSize">Size of the source file the measurement belongs to.</param>
        /// <param name="writeTicks">Write time of the source file the measurement belongs to.</param>
        /// <param name="analysis">The measurements.</param>
        public static void Set(string guid, long fileSize, long writeTicks, AudioClipAnalysis analysis)
        {
            Entries[guid] = Entry.From(guid, fileSize, writeTicks, analysis);
            _isDirty = true;
        }

        /// <summary>Writes the cache to disk if anything changed since the last write.</summary>
        public static void Flush()
        {
            if (!_isDirty)
                return;

            Store store = new()
            {
                entries = new List<Entry>(Entries.Values)
            };

            try
            {
                File.WriteAllText(FilePath, JsonUtility.ToJson(store));
                _isDirty = false;
            }
            catch (Exception)
            {
                // A cache that cannot be written is not worth interrupting the user over.
                _isDirty = false;
            }
        }

        /// <summary>Throws the cache away so the next deep pass reads every file again.</summary>
        public static void Clear()
        {
            Entries.Clear();
            _isDirty = true;

            Flush();
        }

        private static Dictionary<string, Entry> Load()
        {
            Dictionary<string, Entry> loaded = new();

            if (!File.Exists(FilePath))
                return loaded;

            try
            {
                Store store = JsonUtility.FromJson<Store>(File.ReadAllText(FilePath));

                if (store?.entries == null)
                    return loaded;

                foreach (Entry entry in store.entries)
                    loaded[entry.guid] = entry;
            }
            catch (Exception)
            {
                loaded.Clear();
            }

            return loaded;
        }

        [Serializable]
        private sealed class Store
        {
            public List<Entry> entries = new();
        }

        [Serializable]
        private sealed class Entry
        {
            public string guid;
            public long fileSize;
            public long writeTicks;
            public bool hasData;
            public bool isStereo;
            public float peak;
            public float rms;
            public float dcOffset;
            public float leadingSilence;
            public float trailingSilence;
            public float channelDifference;
            public int clippedSamples;

            public static Entry From(string guid, long fileSize, long writeTicks, AudioClipAnalysis analysis)
                => new()
                {
                    guid = guid,
                    fileSize = fileSize,
                    writeTicks = writeTicks,
                    hasData = analysis.HasData,
                    isStereo = analysis.IsStereo,
                    peak = analysis.Peak,
                    rms = analysis.Rms,
                    dcOffset = analysis.DcOffset,
                    leadingSilence = analysis.LeadingSilence,
                    trailingSilence = analysis.TrailingSilence,
                    channelDifference = analysis.ChannelDifference,
                    clippedSamples = analysis.ClippedSamples
                };

            public AudioClipAnalysis ToAnalysis() => new()
            {
                HasData = hasData,
                IsStereo = isStereo,
                Peak = peak,
                Rms = rms,
                DcOffset = dcOffset,
                LeadingSilence = leadingSilence,
                TrailingSilence = trailingSilence,
                ChannelDifference = channelDifference,
                ClippedSamples = clippedSamples
            };
        }
    }
}