using System.Collections.Generic;
using Base.ToolsPackage.Editor.AudioRules.Data;
using Base.ToolsPackage.Editor.AudioRules.Model;
using Base.ToolsPackage.Editor.AudioRules.Scanning;
using Base.UtilityPackage.Logging;
using Base.UtilityPackage.Menus;
using UnityEditor;

namespace Base.ToolsPackage.Editor.AudioRules.Apply
{
    /// <summary>
    /// Brings every clip in line with the rules on every import target at once, so updating the
    /// whole project is one click instead of an Apply per target in the window.
    /// <para>
    /// The default settings go first and are reimported before the platforms are scanned. A
    /// platform without its own override then reads the new defaults, so it only gets an override
    /// where a platform rule really differs from them.
    /// </para>
    /// </summary>
    internal static class AudioAllTargetsApplier
    {
        private const string MenuPath = "Tools/Base Packages/Assets/Apply Audio Rules";

        /// <summary>Applies the rule set of the project to every clip on every target.</summary>
        [DynamicMenuItem(MenuPath)]
        public static void ApplyProjectRules()
        {
            AudioRuleSet ruleSet = AudioRuleSet.Load();

            if (ruleSet == null)
            {
                CustomLogger.LogWarning($"There is no {nameof(AudioRuleSet)} yet. Open the Audio Rules window "
                    + "to create one.", null);
                return;
            }

            Apply(ruleSet);
        }

        /// <summary>Applies the rules to every clip that differs on any target.</summary>
        /// <param name="ruleSet">The rules to apply.</param>
        /// <returns>How many clips were changed.</returns>
        internal static int Apply(AudioRuleSet ruleSet)
        {
            HashSet<string> changed = new();

            foreach (string platform in Targets(ruleSet))
            {
                List<AudioClipPlan> plans = ChangesFor(ruleSet, platform);

                foreach (AudioClipPlan plan in plans)
                    changed.Add(plan.Info.AssetPath);

                AudioSettingsApplier.Apply(plans, platform);
            }

            CustomLogger.Log($"Applied the audio rules to {changed.Count} "
                + $"{Plural(changed.Count)} on every target.", ruleSet);

            return changed.Count;
        }

        /// <summary>
        /// Counts the clips that differ from the rules on any target. Reads import settings only,
        /// so it is cheap enough to run after every scan.
        /// </summary>
        /// <param name="ruleSet">The rules to compare against.</param>
        /// <returns>How many clips an <see cref="Apply"/> would change.</returns>
        internal static int CountChanges(AudioRuleSet ruleSet)
        {
            HashSet<string> changed = new();

            foreach (string platform in Targets(ruleSet))
            {
                foreach (AudioClipPlan plan in ChangesFor(ruleSet, platform))
                    changed.Add(plan.Info.AssetPath);
            }

            return changed.Count;
        }

        private static string Plural(int count) => count == 1
            ? "clip"
            : "clips";

        // The default settings come first, because the platforms inherit them.
        private static IEnumerable<string> Targets(AudioRuleSet ruleSet)
        {
            yield return string.Empty;

            foreach (string platform in ruleSet.Platforms)
            {
                if (!string.IsNullOrWhiteSpace(platform))
                    yield return platform;
            }
        }

        private static List<AudioClipPlan> ChangesFor(AudioRuleSet ruleSet, string platform)
        {
            List<AudioClipPlan> changes = new();

            foreach (AudioClipPlan plan in AudioScanService.Scan(ruleSet, platform, null))
            {
                if (plan.HasChanges)
                    changes.Add(plan);
            }

            return changes;
        }
    }
}