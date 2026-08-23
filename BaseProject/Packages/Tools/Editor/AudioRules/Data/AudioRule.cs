using System;
using System.Collections.Generic;
using Base.ToolPackage.Editor.AudioRules.Model;
using UnityEngine;

namespace Base.ToolPackage.Editor.AudioRules.Data
{
    /// <summary>
    /// One rule: if every condition holds, write these settings. Rules cascade instead of stopping
    /// at the first match, so a list can read as a base rule, then length bands, then a handful of
    /// targeted exceptions, without repeating the shared settings in every band.
    /// </summary>
    [Serializable]
    public sealed class AudioRule
    {
        private const string DefaultLabel = "New Rule";

        [field: Tooltip("Shown in the rule list, the results table and the decision trace.")]
        [field: SerializeField] public string Label { get; set; } = DefaultLabel;

        [field: Tooltip("Why this rule exists. Shown wherever the rule decided something, so the next person"
            + " does not have to guess the reasoning.")]
        [field: TextArea]
        [field: SerializeField] public string Notes { get; set; } = string.Empty;

        [field: Tooltip("Turns the rule off without deleting it.")]
        [field: SerializeField] public bool Enabled { get; set; } = true;

        [field: Tooltip("Empty applies the rule to the default import settings and to every platform."
            + " A platform name applies it only while that platform is selected.")]
        [field: SerializeField] public string PlatformTarget { get; set; } = string.Empty;

        [field: Tooltip("If true every condition has to hold, otherwise one is enough."
            + " A rule without conditions always applies.")]
        [field: SerializeField] public bool RequireAllConditions { get; set; } = true;

        [field: Tooltip("The tests a clip has to pass for this rule to apply.")]
        [field: SerializeField] public List<AudioRuleCondition> Conditions { get; private set; } = new();

        [field: Tooltip("The settings this rule writes.")]
        [field: SerializeField] public AudioSettingOverrides Overrides { get; private set; } = new();

        /// <summary>True when the rule applies to the default settings rather than one platform.</summary>
        public bool IsDefaultTarget => string.IsNullOrWhiteSpace(PlatformTarget);

        /// <summary>Creates an empty rule. Needed by the serializer.</summary>
        public AudioRule() { }

        /// <summary>Creates a labeled rule.</summary>
        /// <param name="label">The label shown to the user.</param>
        public AudioRule(string label) => Label = label;

        /// <summary>True when the rule is written for the target currently being resolved.</summary>
        /// <param name="platform">The platform being resolved, empty for the default settings.</param>
        /// <returns>True when the rule takes part in this resolve.</returns>
        public bool AppliesToTarget(string platform)
        {
            if (IsDefaultTarget)
                return true;

            return string.Equals(PlatformTarget, platform, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>True when the clip passes the conditions of this rule.</summary>
        /// <param name="info">The facts gathered about the clip.</param>
        /// <returns>True when the rule applies to the clip.</returns>
        public bool Matches(AudioClipInfo info)
        {
            if (Conditions.Count == 0)
                return true;

            foreach (AudioRuleCondition condition in Conditions)
            {
                bool holds = Evaluate(condition, info);

                if (RequireAllConditions
                    && !holds)
                    return false;

                if (!RequireAllConditions
                    && holds)
                    return true;
            }

            return RequireAllConditions;
        }

        private static bool Evaluate(AudioRuleCondition condition, AudioClipInfo info) => condition.Field switch
        {
            EConditionField.Category => condition.MatchesText(info.Category),
            EConditionField.Channels => condition.MatchesNumber(info.Channels),
            EConditionField.DurationSeconds => condition.MatchesNumber(info.LengthSeconds),
            EConditionField.FileSizeKilobytes => condition.MatchesNumber(info.FileSizeKilobytes),
            EConditionField.IsLooping => condition.MatchesFlag(info.IsLooping),
            EConditionField.Name => condition.MatchesText(info.Name),
            EConditionField.Path => condition.MatchesText(info.AssetPath),
            EConditionField.SampleRate => condition.MatchesNumber(info.SampleRate),
            _ => false
        };
    }
}