using System.Collections.Generic;
using System.Linq;
using Base.ToolPackage.Editor.AudioRules.Data;
using Base.ToolPackage.Editor.AudioRules.Model;
using Base.ToolPackage.Editor.AudioRules.Scanning;
using UnityEngine.UIElements;

namespace Base.ToolPackage.Editor.AudioRules.Window
{
    /// <summary>
    /// Shows one clip setting by setting: what it has now, what the rules want, which rule decided
    /// it and why that rule exists. This is where "why does this clip want streaming" gets
    /// answered, which is the price a cascading rule list has to pay to be worth using.
    /// </summary>
    internal sealed class AudioClipDetailsView : VisualElement
    {
        private const float CurrentWidth = 150f;
        private const float LabelWidth = 160f;
        private const string NoRule = "Unchanged";
        private const float RuleWidth = 150f;
        private const float TargetWidth = 150f;

        private readonly ScrollView _body = new();

        private AudioRuleSet _ruleSet;

        /// <summary>Builds the empty pane.</summary>
        public AudioClipDetailsView()
        {
            style.flexGrow = 1f;

            _body.style.flexGrow = 1f;

            Add(_body);
            SetPlan(null);
        }

        /// <summary>Points the pane at the rule set, so it can look up why a rule exists.</summary>
        /// <param name="ruleSet">The rule set the plans were resolved with.</param>
        internal void SetRuleSet(AudioRuleSet ruleSet) => _ruleSet = ruleSet;

        /// <summary>Shows a clip, or the empty state when nothing is selected.</summary>
        /// <param name="plan">The plan to describe.</param>
        internal void SetPlan(AudioClipPlan plan)
        {
            _body.Clear();

            if (plan == null)
            {
                _body.Add(Text("Select a clip to see what the rules decided for it.",
                    AudioRulesStyle.DetailPlaceholderClass));

                return;
            }

            BuildHeader(plan);
            BuildCascade(plan);
            BuildSettings(plan);
            BuildAnalysis(plan);
        }

        private static Label Text(string value)
        {
            Label label = new(value);

            label.AddToClassList(AudioRulesStyle.DetailClass);

            return label;
        }

        private static Label Text(string value, string extraClass)
        {
            Label label = Text(value);

            label.AddToClassList(extraClass);

            return label;
        }

        private static Label Chip(string text, string variant, string tooltip)
        {
            Label chip = new(text)
            {
                tooltip = tooltip
            };

            chip.AddToClassList(AudioRulesStyle.ChipClass);

            if (!string.IsNullOrEmpty(variant))
                chip.AddToClassList(variant);

            return chip;
        }

        private static VisualElement MetaRow()
        {
            VisualElement row = new();

            row.AddToClassList(AudioRulesStyle.MetaRowClass);

            return row;
        }

        private static VisualElement Section(string title)
        {
            VisualElement section = new();

            section.AddToClassList(AudioRulesStyle.SectionClass);

            Label label = new(title);

            label.AddToClassList(AudioRulesStyle.SectionTitleClass);

            VisualElement line = new();

            line.AddToClassList(AudioRulesStyle.SectionRuleClass);

            section.Add(label);
            section.Add(line);

            return section;
        }

        private static Label Cell(string text, float width, bool isHead, bool dim, string tooltip)
        {
            Label label = new(text)
            {
                tooltip = tooltip
            };

            label.AddToClassList(AudioRulesStyle.GridCellClass);
            label.EnableInClassList(AudioRulesStyle.GridCellHeadClass, isHead);
            label.EnableInClassList(AudioRulesStyle.GridCellDimClass, dim);

            if (width > 0f)
                label.style.width = width;

            return label;
        }

        private static string Variant(long delta)
        {
            if (delta > 0L)
                return AudioRulesStyle.ChipGoodClass;

            return delta < 0L
                ? AudioRulesStyle.ChipBadClass
                : null;
        }

        private static VisualElement Row(string label, string current, string target, string rule, bool changed,
            bool isHead, string reason)
        {
            VisualElement row = new();

            row.AddToClassList(AudioRulesStyle.GridRowClass);
            row.EnableInClassList(AudioRulesStyle.GridRowHeadClass, isHead);
            row.EnableInClassList(AudioRulesStyle.GridRowChangedClass, changed);

            row.Add(Cell(label, LabelWidth, isHead, false, null));
            row.Add(Cell(current, CurrentWidth, isHead, changed, null));
            row.Add(Cell(target, TargetWidth, isHead, false, null));
            row.Add(Cell(rule, RuleWidth, isHead, !changed, reason));

            return row;
        }

        private string ReasonFor(string ruleLabel)
        {
            if (_ruleSet == null
                || string.IsNullOrEmpty(ruleLabel))
                return null;

            AudioRule rule = _ruleSet.Rules.FirstOrDefault(entry => entry.Label == ruleLabel);

            return rule == null || string.IsNullOrWhiteSpace(rule.Notes)
                ? null
                : rule.Notes;
        }

        private void BuildHeader(AudioClipPlan plan)
        {
            _body.Add(Text(plan.Info.Name, AudioRulesStyle.DetailTitleClass));
            _body.Add(Text(plan.Info.AssetPath, AudioRulesStyle.DetailPathClass));

            VisualElement meta = MetaRow();

            meta.Add(Chip(AudioRulesFormat.Seconds(plan.Info.LengthSeconds), null, "Length"));
            meta.Add(Chip($"{plan.Info.Channels} ch", null, "Channels as imported"));
            meta.Add(Chip(AudioRulesFormat.Kilohertz(plan.Info.SampleRate), null, "Sample rate as imported"));
            meta.Add(Chip(AudioRulesFormat.Size(plan.Info.FileSizeBytes) + " on disk", null, "Source file size"));

            if (plan.Info.HasContainer)
                meta.Add(Chip(plan.Info.Category, null, "Category the containers reference this clip with"));

            if (plan.Info.IsLooping)
                meta.Add(Chip("Looping", null, "A container plays this clip as a loop"));

            meta.Add(Chip($"Build {AudioRulesFormat.Size(plan.CurrentBuildBytes)} to "
                + AudioRulesFormat.Size(plan.TargetBuildBytes), Variant(plan.BuildDelta), "Estimated build size"));

            meta.Add(Chip($"Memory {AudioRulesFormat.Size(plan.CurrentRuntimeBytes)} to "
                + AudioRulesFormat.Size(plan.TargetRuntimeBytes), Variant(plan.RuntimeDelta),
                "Estimated runtime memory while loaded"));

            _body.Add(meta);
        }

        private void BuildCascade(AudioClipPlan plan)
        {
            _body.Add(Section("CASCADE"));

            if (plan.MatchedRules.Count == 0)
            {
                _body.Add(Text("No rule matched this clip."));
                return;
            }

            foreach (string label in plan.MatchedRules)
            {
                _body.Add(Text(label, AudioRulesStyle.DetailRuleClass));

                string reason = ReasonFor(label);

                if (!string.IsNullOrEmpty(reason))
                    _body.Add(Text(reason, AudioRulesStyle.DetailReasonClass));
            }
        }

        private void BuildSettings(AudioClipPlan plan)
        {
            _body.Add(Section("SETTINGS"));
            _body.Add(Row("Setting", "Current", "Target", "Decided by", false, true, null));

            // Settings that do nothing for this clip sink to the bottom, so the rows that matter
            // are the ones the eye lands on first.
            foreach (EAudioSetting setting in Ordered(plan))
                _body.Add(BuildSettingRow(plan, setting));
        }

        private static IEnumerable<EAudioSetting> Ordered(AudioClipPlan plan)
        {
            IReadOnlyList<EAudioSetting> settings = AudioRuleResolver.Settings();

            return settings.Where(setting => AudioRuleResolver.IsApplicable(plan.Target, setting))
                .Concat(settings.Where(setting => !AudioRuleResolver.IsApplicable(plan.Target, setting)));
        }

        private VisualElement BuildSettingRow(AudioClipPlan plan, EAudioSetting setting)
        {
            bool changed = plan.Changes.Contains(setting);
            bool applies = AudioRuleResolver.IsApplicable(plan.Target, setting);
            string decided = string.Empty;
            string reason = null;

            if (applies)
            {
                decided = plan.Decisions.TryGetValue(setting, out string label)
                    ? label
                    : NoRule;

                reason = ReasonFor(decided);
            }

            VisualElement row = Row(setting.ToString(), AudioRuleResolver.Describe(plan.Info.Current, setting),
                AudioRuleResolver.Describe(plan.Target, setting), decided, changed, false, reason);

            row.EnableInClassList(AudioRulesStyle.GridRowMutedClass, !applies);

            return row;
        }

        private void BuildAnalysis(AudioClipPlan plan)
        {
            _body.Add(Section("SAMPLE DATA"));

            if (plan.Analysis == null)
            {
                _body.Add(Text("Not read yet."));
                return;
            }

            if (!plan.Analysis.HasData)
            {
                _body.Add(Text("The sample data could not be read. Streaming clips do not hand it over."));
                return;
            }

            VisualElement meta = MetaRow();

            meta.Add(Chip($"Peak {AudioRulesFormat.Decibels(plan.Analysis.Peak)}", null,
                $"Loudest sample, {plan.Analysis.Peak:0.000} linear"));

            meta.Add(Chip($"RMS {AudioRulesFormat.Decibels(plan.Analysis.Rms)}", null,
                $"Average level over the whole clip, {plan.Analysis.Rms:0.000} linear"));

            meta.Add(Chip($"DC offset {plan.Analysis.DcOffset:0.0000}", null,
                "How far the average sample sits away from zero"));

            meta.Add(Chip($"Head silence {AudioRulesFormat.Seconds(plan.Analysis.LeadingSilence)}", null,
                "Near silence before the first audible sample"));

            meta.Add(Chip($"Tail silence {AudioRulesFormat.Seconds(plan.Analysis.TrailingSilence)}", null,
                "Near silence after the last audible sample"));

            if (plan.Analysis.IsStereo)
                meta.Add(Chip($"Channel difference {plan.Analysis.ChannelDifference:0.0000}", null,
                    "Largest difference between the two channels. Zero means the second one is wasted."));

            _body.Add(meta);

            IReadOnlyList<EAudioFinding> findings = plan.Findings;

            if (findings.Count == 0)
            {
                _body.Add(Text("Nothing to report."));
                return;
            }

            foreach (EAudioFinding finding in findings)
                _body.Add(Text(Describe(finding, plan)));
        }

        private string Describe(EAudioFinding finding, AudioClipPlan plan) => finding switch
        {
            EAudioFinding.Clipping => $"Clipping: {plan.Analysis.ClippedSamples} samples sit at full scale.",
            EAudioFinding.DcOffset => "DC offset: the waveform is not centered, which wastes headroom.",
            EAudioFinding.FakeStereo => "Fake stereo: both channels carry the same signal. Force to mono halves "
                + "the file and the memory.",
            EAudioFinding.LeadingSilence => "Head silence: playback starts late and the silence still costs "
                + "memory.",
            EAudioFinding.LowPeak => $"Quiet: the loudest sample only reaches "
                + $"{AudioRulesFormat.Decibels(plan.Analysis.Peak)}.",
            _ => "Tail silence: the end of the clip is silent."
        };
    }
}