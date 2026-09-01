using System;
using System.Collections.Generic;
using Base.ToolPackage.Editor.AudioRules.Data;
using Base.ToolPackage.Editor.AudioRules.Model;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.AudioRules.Scanning
{
    /// <summary>
    /// Runs the cascade for one clip. The target starts as a copy of what the clip has today, then
    /// every matching rule writes the settings it owns, so a setting no rule touches is never
    /// reported as a change. The order of the rule list is the order of the cascade.
    /// </summary>
    internal static class AudioRuleResolver
    {
        private const string NotApplicable = "n/a";
        private const float QualityTolerance = 0.005f;

        private static readonly EAudioSetting[] AllSettings =
        {
            EAudioSetting.CompressionFormat,
            EAudioSetting.ForceToMono,
            EAudioSetting.LoadInBackground,
            EAudioSetting.LoadType,
            EAudioSetting.PreloadAudioData,
            EAudioSetting.Quality,
            EAudioSetting.SampleRate
        };

        /// <summary>Resolves one clip against the rule set.</summary>
        /// <param name="info">The facts about the clip.</param>
        /// <param name="ruleSet">The rules to run.</param>
        /// <param name="platform">The import target being resolved, empty for the default settings.</param>
        /// <param name="matchCounts">Counts per rule label, raised for every rule that matched.</param>
        /// <returns>The plan for the clip.</returns>
        internal static AudioClipPlan Resolve(AudioClipInfo info, AudioRuleSet ruleSet, string platform,
            IDictionary<string, int> matchCounts)
        {
            AudioSettingValues target = info.Current.Clone();
            Dictionary<EAudioSetting, string> decisions = new();
            List<string> matched = new();

            foreach (AudioRule rule in ruleSet.Rules)
            {
                if (!rule.Enabled
                    || !rule.AppliesToTarget(platform)
                    || !rule.Matches(info))
                    continue;

                matched.Add(rule.Label);
                rule.Overrides.ApplyTo(target, decisions, rule.Label);

                if (matchCounts == null)
                    continue;

                matchCounts.TryGetValue(rule.Label, out int matches);
                matchCounts[rule.Label] = matches + 1;
            }

            List<EAudioSetting> changes = CollectChanges(info.Current, target);

            return new AudioClipPlan(info, target, matched, decisions, changes,
                AudioMemoryEstimator.EstimateRuntimeBytes(info, info.Current),
                AudioMemoryEstimator.EstimateRuntimeBytes(info, target),
                AudioMemoryEstimator.EstimateBuildBytes(info, info.Current),
                AudioMemoryEstimator.EstimateBuildBytes(info, target));
        }

        /// <summary>The value a setting holds, formatted for the table and the details pane.</summary>
        /// <param name="values">The settings to read.</param>
        /// <param name="setting">The setting to describe.</param>
        /// <returns>The value as text.</returns>
        internal static string Describe(AudioSettingValues values, EAudioSetting setting) => setting switch
        {
            EAudioSetting.CompressionFormat => values.CompressionFormat.ToString(),
            EAudioSetting.ForceToMono => values.ForceToMono.ToString(),
            EAudioSetting.LoadInBackground => values.LoadInBackground.ToString(),
            EAudioSetting.LoadType => values.LoadType.ToString(),
            EAudioSetting.PreloadAudioData => values.PreloadAudioData.ToString(),
            EAudioSetting.Quality => DescribeQuality(values),
            EAudioSetting.SampleRate => DescribeSampleRate(values),
            _ => string.Empty
        };

        /// <summary>Every setting the tool knows about, in a fixed order for the details pane.</summary>
        /// <returns>The settings.</returns>
        internal static IReadOnlyList<EAudioSetting> Settings() => AllSettings;

        /// <summary>
        /// False when a setting does nothing for these values, so the window can say so instead of
        /// showing a number that has no effect on the build.
        /// </summary>
        /// <param name="values">The settings to read.</param>
        /// <param name="setting">The setting to ask about.</param>
        /// <returns>True when the setting matters.</returns>
        internal static bool IsApplicable(AudioSettingValues values, EAudioSetting setting)
            => setting != EAudioSetting.Quality || IsLossy(values.CompressionFormat);

        // The slider only reaches a lossy encoder. On PCM and ADPCM the stored number is real but
        // changes nothing, and showing it invites the question why the tool is not fixing it.
        private static string DescribeQuality(AudioSettingValues values) => IsLossy(values.CompressionFormat)
            ? Mathf.RoundToInt(values.Quality * 100f) + "%"
            : NotApplicable;

        private static string DescribeSampleRate(AudioSettingValues values)
            => values.SampleRateSetting == AudioSampleRateSetting.OverrideSampleRate
                ? $"{values.SampleRateOverride} Hz"
                : values.SampleRateSetting.ToString();

        // The quality slider only reaches a lossy encoder, so comparing it on PCM or ADPCM would
        // report a difference that changes nothing in the build.
        // A setting nobody compares yet falls through to false, so adding one to the enum without a
        // case here understates the change rather than reporting a difference that was never checked.
        private static bool Differs(AudioSettingValues current, AudioSettingValues target, EAudioSetting setting)
            => setting switch
            {
                EAudioSetting.CompressionFormat => current.CompressionFormat != target.CompressionFormat,
                EAudioSetting.ForceToMono => current.ForceToMono != target.ForceToMono,
                EAudioSetting.LoadInBackground => current.LoadInBackground != target.LoadInBackground,
                EAudioSetting.LoadType => current.LoadType != target.LoadType,
                EAudioSetting.PreloadAudioData => current.PreloadAudioData != target.PreloadAudioData,
                EAudioSetting.Quality => DiffersInQuality(current, target),
                EAudioSetting.SampleRate => DiffersInSampleRate(current, target),
                _ => false
            };

        private static bool DiffersInQuality(AudioSettingValues current, AudioSettingValues target)
        {
            if (!IsLossy(target.CompressionFormat))
                return false;

            return Math.Abs(current.Quality - target.Quality) > QualityTolerance;
        }

        private static bool DiffersInSampleRate(AudioSettingValues current, AudioSettingValues target)
        {
            if (current.SampleRateSetting != target.SampleRateSetting)
                return true;

            return target.SampleRateSetting == AudioSampleRateSetting.OverrideSampleRate
                && current.SampleRateOverride != target.SampleRateOverride;
        }

        private static bool IsLossy(AudioCompressionFormat format)
            => format == AudioCompressionFormat.Vorbis || format == AudioCompressionFormat.MP3;

        private static List<EAudioSetting> CollectChanges(AudioSettingValues current, AudioSettingValues target)
        {
            List<EAudioSetting> changes = new();

            foreach (EAudioSetting setting in AllSettings)
            {
                if (Differs(current, target, setting))
                    changes.Add(setting);
            }

            return changes;
        }
    }
}