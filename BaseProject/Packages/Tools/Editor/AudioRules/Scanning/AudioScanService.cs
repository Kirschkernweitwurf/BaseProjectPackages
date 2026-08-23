using System.Collections.Generic;
using System.IO;
using Base.ToolPackage.Editor.AudioRules.Data;
using Base.ToolPackage.Editor.AudioRules.Model;
using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.AudioRules.Scanning
{
    /// <summary>
    /// The one entry point the window uses. A scan never writes anything: it collects the facts,
    /// runs the cascade and hands back a plan per clip.
    /// <para>
    /// Reading sample data is far slower than reading import settings, so it is not part of the
    /// scan itself. The window fills in whatever the cache already knows for free and then works
    /// through the rest a few clips at a time, which is why the two passes are separate here and
    /// look like one button to the user.
    /// </para>
    /// </summary>
    internal static class AudioScanService
    {
        /// <summary>Collects and resolves every clip in scope.</summary>
        /// <param name="ruleSet">The rules to run.</param>
        /// <param name="platform">The import target, empty for the default settings.</param>
        /// <param name="matchCounts">Filled with how many clips each rule matched.</param>
        /// <returns>One plan per clip.</returns>
        public static List<AudioClipPlan> Scan(AudioRuleSet ruleSet, string platform,
            IDictionary<string, int> matchCounts)
        {
            List<AudioClipPlan> plans = new();

            if (ruleSet == null)
            {
                CustomLogger.LogError($"Scanning needs an {nameof(AudioRuleSet)}.", null);
                return plans;
            }

            AudioContainerIndex index = AudioContainerIndex.Build(ruleSet);

            foreach (AudioClipInfo info in AudioClipCollector.Collect(ruleSet, platform, index))
                plans.Add(AudioRuleResolver.Resolve(info, ruleSet, platform, matchCounts));

            return plans;
        }

        /// <summary>
        /// Fills in what the cache already knows about a clip. Costs nothing, so it runs for every
        /// plan right after a scan.
        /// </summary>
        /// <param name="plan">The plan to fill in.</param>
        /// <param name="settings">The thresholds to judge by.</param>
        /// <returns>True when the cache had a usable entry.</returns>
        public static bool FillFromCache(AudioClipPlan plan, AudioAnalysisSettings settings)
        {
            if (!AudioAnalysisCache.TryGet(plan.Info.Guid, plan.Info.FileSizeBytes, ReadWriteTicks(plan),
                    out AudioClipAnalysis cached))
                return false;

            plan.Analysis = cached;
            plan.Findings = AudioClipAnalyzer.Evaluate(cached, settings);

            return true;
        }

        /// <summary>Reads the sample data of one clip and caches the result.</summary>
        /// <param name="plan">The plan to fill in.</param>
        /// <param name="settings">The thresholds to judge by.</param>
        public static void AnalyzeOne(AudioClipPlan plan, AudioAnalysisSettings settings)
        {
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(plan.Info.AssetPath);
            AudioClipAnalysis analysis = AudioClipAnalyzer.Analyze(clip, settings);

            plan.Analysis = analysis;
            plan.Findings = AudioClipAnalyzer.Evaluate(analysis, settings);

            AudioAnalysisCache.Set(plan.Info.Guid, plan.Info.FileSizeBytes, ReadWriteTicks(plan), analysis);
        }

        /// <summary>Writes the analysis cache to disk.</summary>
        public static void FlushCache() => AudioAnalysisCache.Flush();

        private static long ReadWriteTicks(AudioClipPlan plan)
        {
            try
            {
                return File.GetLastWriteTimeUtc(Path.GetFullPath(plan.Info.AssetPath)).Ticks;
            }
            catch (IOException)
            {
                return 0L;
            }
        }
    }
}