using System.Collections.Generic;
using Base.ToolPackage.Editor.AudioRules.Data;

namespace Base.ToolPackage.Editor.AudioRules.Model
{
    /// <summary>
    /// What the rules decided for one clip: the settings it should end up with, which rules took
    /// part, which rule won each setting, what the change costs or saves, and whatever the deep
    /// analysis found. Nothing is written until the plan is applied, so a scan is always safe.
    /// </summary>
    internal sealed class AudioClipPlan
    {
        /// <summary>The facts the decision was made from.</summary>
        public AudioClipInfo Info { get; }

        /// <summary>The settings the rules want, starting from what the clip has today.</summary>
        public AudioSettingValues Target { get; }

        /// <summary>Labels of every rule that matched, in the order they were applied.</summary>
        public IReadOnlyList<string> MatchedRules { get; }

        /// <summary>Which rule decided which setting, for the trace in the details pane.</summary>
        public IReadOnlyDictionary<EAudioSetting, string> Decisions { get; }

        /// <summary>The settings that actually differ from what the clip has today.</summary>
        public IReadOnlyList<EAudioSetting> Changes { get; }

        /// <summary>What the deep analysis turned up, empty until it has run.</summary>
        public IReadOnlyList<EAudioFinding> Findings { get; set; }

        /// <summary>The raw analysis numbers, or null while the deep pass has not run.</summary>
        public AudioClipAnalysis Analysis { get; set; }

        /// <summary>Estimated runtime memory with the settings the clip has today.</summary>
        public long CurrentRuntimeBytes { get; }

        /// <summary>Estimated runtime memory with the settings the rules want.</summary>
        public long TargetRuntimeBytes { get; }

        /// <summary>Estimated build size with the settings the clip has today.</summary>
        public long CurrentBuildBytes { get; }

        /// <summary>Estimated build size with the settings the rules want.</summary>
        public long TargetBuildBytes { get; }

        /// <summary>True when applying this plan would change anything.</summary>
        public bool HasChanges => Changes.Count > 0;

        /// <summary>Runtime memory the change saves. Negative when it costs memory.</summary>
        public long RuntimeDelta => CurrentRuntimeBytes - TargetRuntimeBytes;

        /// <summary>Build size the change saves. Negative when it costs size.</summary>
        public long BuildDelta => CurrentBuildBytes - TargetBuildBytes;

        /// <summary>The rule that decided the load type, or an empty string when none did.</summary>
        public string PrimaryRule => MatchedRules.Count > 0
            ? MatchedRules[MatchedRules.Count - 1]
            : string.Empty;

        /// <summary>Creates the plan for one clip.</summary>
        /// <param name="info">The facts the decision was made from.</param>
        /// <param name="target">The settings the rules want.</param>
        /// <param name="matchedRules">Labels of every rule that matched.</param>
        /// <param name="decisions">Which rule decided which setting.</param>
        /// <param name="changes">The settings that differ from today.</param>
        /// <param name="currentRuntimeBytes">Estimated runtime memory today.</param>
        /// <param name="targetRuntimeBytes">Estimated runtime memory after the change.</param>
        /// <param name="currentBuildBytes">Estimated build size today.</param>
        /// <param name="targetBuildBytes">Estimated build size after the change.</param>
        public AudioClipPlan(AudioClipInfo info, AudioSettingValues target, IReadOnlyList<string> matchedRules,
            IReadOnlyDictionary<EAudioSetting, string> decisions, IReadOnlyList<EAudioSetting> changes,
            long currentRuntimeBytes, long targetRuntimeBytes, long currentBuildBytes, long targetBuildBytes)
        {
            Info = info;
            Target = target;
            MatchedRules = matchedRules;
            Decisions = decisions;
            Changes = changes;
            CurrentRuntimeBytes = currentRuntimeBytes;
            TargetRuntimeBytes = targetRuntimeBytes;
            CurrentBuildBytes = currentBuildBytes;
            TargetBuildBytes = targetBuildBytes;
            Findings = new List<EAudioFinding>();
        }
    }
}