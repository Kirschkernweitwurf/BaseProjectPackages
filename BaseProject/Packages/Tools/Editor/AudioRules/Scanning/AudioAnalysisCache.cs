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
        internal static bool TryGet(string guid, long fileSize, long writeTicks, out AudioClipAnalysis analysis)
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
        internal static void Set(string guid, long fileSize, long writeTicks, AudioClipAnalysis analysis)
        {
            Entries[guid] = Entry.From(guid, fileSize, writeTicks, analysis);
            _isDirty = true;
        }

        /// <summary>Writes the cache to disk if anything changed since the last write.</summary>
        internal static void Flush()
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
        internal static void Clear()
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
        private sealed class Entry
        {
            /// <summary>GUID of the clip the measurement belongs to.</summary>
            public string guid;

            /// <summary>
            /// Size of the source file when it was measured. Together with the write time this is what
            /// decides whether the entry still describes the file on disk.
            /// </summary>
            public long fileSize;

            /// <summary>Write time of the source file when it was measured, in ticks.</summary>
            public long writeTicks;

            /// <summary>False when the clip could not be read, so an empty result is not measured again.</summary>
            public bool hasData;

            /// <summary>Whether the clip has more than one channel.</summary>
            public bool isStereo;

            /// <summary>Loudest sample in the clip, as a normalized amplitude.</summary>
            public float peak;

            /// <summary>Average loudness across the clip, as a normalized amplitude.</summary>
            public float rms;

            /// <summary>How far the waveform sits off the zero line, which points at a recording fault.</summary>
            public float dcOffset;

            /// <summary>Seconds of silence before the clip starts, which a rule can require trimmed.</summary>
            public float leadingSilence;

            /// <summary>Seconds of silence after the clip ends.</summary>
            public float trailingSilence;

            /// <summary>
            /// How far the two channels differ. Near zero means a stereo clip carrying mono content,
            /// which is twice the data for nothing.
            /// </summary>
            public float channelDifference;

            /// <summary>How many samples hit the ceiling, which is what audible clipping sounds like.</summary>
            public int clippedSamples;

            /// <summary>Builds a cache entry from one measurement.</summary>
            /// <param name="guid">GUID of the clip.</param>
            /// <param name="fileSize">Size of the source file at the time of measuring.</param>
            /// <param name="writeTicks">Write time of the source file at the time of measuring.</param>
            /// <param name="analysis">The measurements to store.</param>
            /// <returns>The entry to put in the cache.</returns>
            internal static Entry From(string guid, long fileSize, long writeTicks, AudioClipAnalysis analysis) => new()
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

            /// <summary>Rebuilds the measurement this entry was created from.</summary>
            /// <returns>The restored analysis.</returns>
            internal AudioClipAnalysis ToAnalysis() => new()
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

        [Serializable]
        private sealed class Store
        {
            /// <summary>
            /// Every cached measurement. A list rather than a dictionary because JsonUtility cannot
            /// serialize one; it is turned back into a lookup on load.
            /// </summary>
            public List<Entry> entries = new();
        }
    }
}