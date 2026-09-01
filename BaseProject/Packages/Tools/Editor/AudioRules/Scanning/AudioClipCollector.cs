using System;
using System.Collections.Generic;
using System.IO;
using Base.ToolPackage.Editor.AudioRules.Data;
using Base.ToolPackage.Editor.AudioRules.Model;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.AudioRules.Scanning
{
    /// <summary>
    /// Walks the project and gathers the facts about every clip in scope. The length, channel
    /// count and sample rate are only available on the loaded clip, not on the importer, so this
    /// pass loads every clip asset. It does not read the sample data, that is the analyzer's job.
    /// </summary>
    internal static class AudioClipCollector
    {
        private const string ClipFilter = "t:" + nameof(AudioClip);
        private const string ProgressTitle = "Scanning audio";

        private static readonly string[] AssetsOnly =
        {
            "Assets"
        };

        private static readonly string[] AssetsAndPackages =
        {
            "Assets",
            "Packages"
        };

        /// <summary>Collects every clip the rule set is interested in.</summary>
        /// <param name="ruleSet">The rule set that decides the scope.</param>
        /// <param name="platform">The import target to read the current settings from.</param>
        /// <param name="index">The container index that supplies category and loop flag.</param>
        /// <returns>One entry per clip, in project order.</returns>
        internal static List<AudioClipInfo> Collect(AudioRuleSet ruleSet, string platform, AudioContainerIndex index)
        {
            List<AudioClipInfo> results = new();
            string[] guids = AssetDatabase.FindAssets(ClipFilter, SearchFolders(ruleSet));

            try
            {
                for (int position = 0; position < guids.Length; position++)
                {
                    if (ReportProgress(position, guids.Length))
                        break;

                    AudioClipInfo info = Read(guids[position], ruleSet, platform, index);

                    if (info != null)
                        results.Add(info);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            return results;
        }

        /// <summary>Reads the settings a clip has right now for one target.</summary>
        /// <param name="importer">The importer of the clip.</param>
        /// <param name="platform">The import target, empty for the default settings.</param>
        /// <returns>The current settings.</returns>
        internal static AudioSettingValues ReadCurrent(AudioImporter importer, string platform)
        {
            AudioImporterSampleSettings settings = AudioPlatformTargets.Read(importer, platform);

            return new AudioSettingValues
            {
                LoadType = settings.loadType,
                CompressionFormat = settings.compressionFormat,
                Quality = settings.quality,
                SampleRateSetting = settings.sampleRateSetting,
                SampleRateOverride = (int)settings.sampleRateOverride,
                PreloadAudioData = settings.preloadAudioData,
                ForceToMono = importer.forceToMono,
                LoadInBackground = importer.loadInBackground
            };
        }

        private static string[] SearchFolders(AudioRuleSet ruleSet) => ruleSet.IncludePackages
            ? AssetsAndPackages
            : AssetsOnly;

        private static bool ReportProgress(int position, int total)
        {
            if (position % 25 != 0)
                return false;

            return EditorUtility.DisplayCancelableProgressBar(ProgressTitle, $"{position} of {total} clips",
                position / (float)Mathf.Max(1, total));
        }

        private static long ReadFileSize(string assetPath)
        {
            try
            {
                FileInfo file = new(Path.GetFullPath(assetPath));

                return file.Exists
                    ? file.Length
                    : 0L;
            }
            catch (Exception)
            {
                return 0L;
            }
        }

        private static AudioClipInfo Read(string guid, AudioRuleSet ruleSet, string platform,
            AudioContainerIndex index)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (string.IsNullOrEmpty(path)
                || ruleSet.IsIgnoredPath(path))
                return null;

            if (AssetImporter.GetAtPath(path) is not AudioImporter importer)
                return null;

            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);

            if (clip == null)
                return null;

            return new AudioClipInfo(path, guid, Path.GetFileNameWithoutExtension(path), clip.length, clip.channels,
                clip.frequency, ReadFileSize(path), index.GetCategory(guid), index.IsLooping(guid),
                index.HasContainer(guid), ReadCurrent(importer, platform));
        }
    }
}