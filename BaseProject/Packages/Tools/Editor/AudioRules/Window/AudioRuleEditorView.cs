using System;
using System.Collections.Generic;
using Base.ToolPackage.Editor.AudioRules.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Base.ToolPackage.Editor.AudioRules.Window
{
    /// <summary>
    /// Edits the selected rule as what it is: a list of conditions and a list of settings. The
    /// operator dropdown only offers what the chosen field can actually be compared with, so a
    /// rule cannot be built that silently never matches.
    /// </summary>
    internal sealed class AudioRuleEditorView : VisualElement
    {
        private const float ConditionFieldWidth = 148f;
        private const float ConditionOperatorWidth = 124f;
        private const string MatchAll = "All";
        private const string MatchAny = "Any";

        private static readonly EConditionOperator[] FlagOperators =
        {
            EConditionOperator.Equals,
            EConditionOperator.NotEquals
        };

        private static readonly EConditionOperator[] NumberOperators =
        {
            EConditionOperator.LessThan,
            EConditionOperator.LessOrEqual,
            EConditionOperator.GreaterOrEqual,
            EConditionOperator.GreaterThan,
            EConditionOperator.Equals,
            EConditionOperator.NotEquals
        };

        private static readonly EConditionOperator[] TextOperators =
        {
            EConditionOperator.Contains,
            EConditionOperator.NotContains,
            EConditionOperator.Equals,
            EConditionOperator.NotEquals,
            EConditionOperator.Matches
        };

        /// <summary>Raised whenever the rule was edited, so the owner can save and rescan.</summary>
        public event Action Changed;

        private readonly ScrollView _body = new();

        private AudioRule _rule;
        private IReadOnlyList<string> _platforms = Array.Empty<string>();

        /// <summary>Builds the empty pane.</summary>
        public AudioRuleEditorView()
        {
            style.flexGrow = 1f;

            _body.style.flexGrow = 1f;

            Add(_body);
            Rebuild();
        }

        /// <summary>Shows a rule, or the empty state when there is none.</summary>
        /// <param name="rule">The rule to edit.</param>
        /// <param name="platforms">The platforms offered as a target.</param>
        public void SetRule(AudioRule rule, IReadOnlyList<string> platforms)
        {
            _rule = rule;
            _platforms = platforms;

            Rebuild();
        }

        private static VisualElement Row()
        {
            VisualElement row = new();

            row.AddToClassList("ar-row");
            row.style.flexDirection = FlexDirection.Row;

            return row;
        }

        private static VisualElement Header(string text)
        {
            VisualElement section = new();

            section.AddToClassList("ar-section");

            Label label = new(text);

            label.AddToClassList("ar-section__title");

            VisualElement line = new();

            line.AddToClassList("ar-section__rule");

            section.Add(label);
            section.Add(line);

            return section;
        }

        private static VisualElement Card()
        {
            VisualElement card = new();

            card.AddToClassList("ar-card");

            return card;
        }

        private static string[] Names(IEnumerable<EConditionOperator> operators)
        {
            List<string> names = new();

            foreach (EConditionOperator value in operators)
                names.Add(value.ToString());

            return names.ToArray();
        }

        private static EConditionOperator[] OperatorsFor(AudioRuleCondition condition)
        {
            if (condition.IsFlag)
                return FlagOperators;

            return condition.IsNumeric
                ? NumberOperators
                : TextOperators;
        }

        private void Rebuild()
        {
            _body.Clear();

            if (_rule == null)
            {
                _body.Add(new Label("Select a rule on the left, or add one.")
                {
                    style =
                    {
                        marginTop = 8f,
                        marginLeft = 8f
                    }
                });

                return;
            }

            BuildHeader();
            BuildConditions();
            BuildSettings();
        }

        private void BuildHeader()
        {
            TextField label = new("Label")
            {
                value = _rule.Label
            };

            label.RegisterValueChangedCallback(evt =>
            {
                _rule.Label = evt.newValue;
                Changed?.Invoke();
            });

            VisualElement card = Card();

            card.Add(label);

            List<string> choices = new()
            {
                AudioRulesWindow.DefaultTargetLabel
            };

            foreach (string platform in _platforms)
                choices.Add(platform);

            DropdownField target = new("Platform", choices, 0)
            {
                value = _rule.IsDefaultTarget
                    ? AudioRulesWindow.DefaultTargetLabel
                    : _rule.PlatformTarget,
                tooltip = "Default applies the rule to every target. A platform applies it only while that "
                    + "platform is selected in the toolbar."
            };

            target.RegisterValueChangedCallback(evt =>
            {
                _rule.PlatformTarget = evt.newValue == AudioRulesWindow.DefaultTargetLabel
                    ? string.Empty
                    : evt.newValue;

                Changed?.Invoke();
            });

            card.Add(target);

            TextField notes = new("Why")
            {
                value = _rule.Notes,
                multiline = true,
                tooltip = "Shown wherever this rule decided something, so the reasoning does not live only in "
                    + "your head."
            };

            notes.AddToClassList("ar-notes");
            notes.RegisterValueChangedCallback(evt =>
            {
                _rule.Notes = evt.newValue;
                Changed?.Invoke();
            });

            card.Add(notes);
            _body.Add(card);
        }

        private void BuildConditions()
        {
            VisualElement header = Header("APPLIES WHEN");
            List<string> modes = new()
            {
                MatchAll,
                MatchAny
            };

            DropdownField match = new(modes, _rule.RequireAllConditions
                ? MatchAll
                : MatchAny)
            {
                tooltip = "All, every condition has to hold. Any, one is enough. A rule without conditions "
                    + "always applies."
            };

            match.AddToClassList("ar-match");
            match.RegisterValueChangedCallback(evt =>
            {
                _rule.RequireAllConditions = evt.newValue == MatchAll;

                Changed?.Invoke();
                Rebuild();
            });

            header.Insert(1, match);
            _body.Add(header);

            VisualElement card = Card();

            if (_rule.Conditions.Count == 0)
                card.Add(new Label("No conditions, so this rule applies to every clip.")
                {
                    style =
                    {
                        marginBottom = 4f
                    }
                });

            for (int index = 0; index < _rule.Conditions.Count; index++)
                card.Add(BuildConditionRow(_rule.Conditions[index], index));

            Button add = new(AddCondition)
            {
                text = "Add Condition"
            };

            add.AddToClassList("ar-add");
            card.Add(add);

            _body.Add(card);
        }

        private Label Connector(int index)
        {
            string text = index == 0
                ? "if"
                : Joiner();

            Label label = new(text);

            label.AddToClassList("ar-conn");

            return label;
        }

        private string Joiner() => _rule.RequireAllConditions
            ? "and"
            : "or";

        private VisualElement BuildConditionRow(AudioRuleCondition condition, int index)
        {
            VisualElement row = Row();

            row.Add(Connector(index));

            EnumField field = new(condition.Field)
            {
                style =
                {
                    width = ConditionFieldWidth
                }
            };

            field.RegisterValueChangedCallback(evt =>
            {
                condition.Field = (EConditionField)evt.newValue;
                Changed?.Invoke();
                Rebuild();
            });

            row.Add(field);
            row.Add(BuildOperatorField(condition));
            row.Add(BuildValueField(condition));

            Button remove = new(() => RemoveCondition(index))
            {
                text = "\u2715"
            };

            remove.AddToClassList("ar-ghost");
            row.Add(remove);

            return row;
        }

        private VisualElement BuildOperatorField(AudioRuleCondition condition)
        {
            EConditionOperator[] allowed = OperatorsFor(condition);
            List<string> choices = new(Names(allowed));

            string current = Array.IndexOf(allowed, condition.Operator) >= 0
                ? condition.Operator.ToString()
                : choices[0];

            DropdownField dropdown = new(choices, current)
            {
                style =
                {
                    width = ConditionOperatorWidth
                }
            };

            dropdown.RegisterValueChangedCallback(evt =>
            {
                condition.Operator = Enum.Parse<EConditionOperator>(evt.newValue);
                Changed?.Invoke();
            });

            return dropdown;
        }

        private VisualElement BuildValueField(AudioRuleCondition condition)
        {
            if (condition.IsFlag)
            {
                Toggle flag = new()
                {
                    value = condition.Number > 0.5f
                };

                flag.RegisterValueChangedCallback(evt =>
                {
                    condition.Number = evt.newValue
                        ? 1f
                        : 0f;

                    Changed?.Invoke();
                });

                return flag;
            }

            if (condition.IsNumeric)
            {
                FloatField number = new()
                {
                    value = condition.Number,
                    style =
                    {
                        flexGrow = 1f
                    }
                };

                number.RegisterValueChangedCallback(evt =>
                {
                    condition.Number = evt.newValue;
                    Changed?.Invoke();
                });

                return number;
            }

            TextField text = new()
            {
                value = condition.Text,
                style =
                {
                    flexGrow = 1f
                }
            };

            text.RegisterValueChangedCallback(evt =>
            {
                condition.Text = evt.newValue;
                Changed?.Invoke();
            });

            return text;
        }

        private void BuildSettings()
        {
            _body.Add(Header("THEN WRITE"));

            VisualElement card = Card();
            AudioSettingOverrides overrides = _rule.Overrides;

            EnumField loadType = new(overrides.LoadType);

            loadType.RegisterValueChangedCallback(evt =>
            {
                overrides.LoadType = (AudioClipLoadType)evt.newValue;
                Changed?.Invoke();
            });

            card.Add(SettingRow("Load type", overrides.SetsLoadType, loadType,
                value => overrides.SetsLoadType = value));

            EnumField format = new(overrides.CompressionFormat);

            format.RegisterValueChangedCallback(evt =>
            {
                overrides.CompressionFormat = (AudioCompressionFormat)evt.newValue;
                Changed?.Invoke();
            });

            card.Add(SettingRow("Compression", overrides.SetsCompressionFormat, format,
                value => overrides.SetsCompressionFormat = value));

            Slider quality = new(0f, 1f)
            {
                value = overrides.Quality,
                showInputField = true,
                tooltip = "Only reaches the lossy formats. The importer inspector shows this as a percentage."
            };

            quality.RegisterValueChangedCallback(evt =>
            {
                overrides.Quality = evt.newValue;
                Changed?.Invoke();
            });

            card.Add(SettingRow("Quality", overrides.SetsQuality, quality,
                value => overrides.SetsQuality = value));

            card.Add(BuildSampleRateRow(overrides));

            card.Add(SettingRow("Force to mono", overrides.SetsForceToMono,
                BuildFlag(overrides.ForceToMono, value => overrides.ForceToMono = value),
                value => overrides.SetsForceToMono = value));

            card.Add(SettingRow("Load in background", overrides.SetsLoadInBackground,
                BuildFlag(overrides.LoadInBackground, value => overrides.LoadInBackground = value),
                value => overrides.SetsLoadInBackground = value));

            card.Add(SettingRow("Preload audio data", overrides.SetsPreloadAudioData,
                BuildFlag(overrides.PreloadAudioData, value => overrides.PreloadAudioData = value),
                value => overrides.SetsPreloadAudioData = value));

            _body.Add(card);
        }

        private VisualElement BuildSampleRateRow(AudioSettingOverrides overrides)
        {
            VisualElement group = new();

            group.style.flexDirection = FlexDirection.Row;
            group.style.flexGrow = 1f;

            EnumField setting = new(overrides.SampleRateSetting)
            {
                style =
                {
                    flexGrow = 1f
                }
            };

            IntegerField rate = new()
            {
                value = overrides.SampleRateOverride,
                style =
                {
                    width = ConditionOperatorWidth
                }
            };

            rate.SetEnabled(overrides.SampleRateSetting == AudioSampleRateSetting.OverrideSampleRate);

            setting.RegisterValueChangedCallback(evt =>
            {
                overrides.SampleRateSetting = (AudioSampleRateSetting)evt.newValue;
                rate.SetEnabled(overrides.SampleRateSetting == AudioSampleRateSetting.OverrideSampleRate);
                Changed?.Invoke();
            });

            rate.RegisterValueChangedCallback(evt =>
            {
                overrides.SampleRateOverride = evt.newValue;
                Changed?.Invoke();
            });

            group.Add(setting);
            group.Add(rate);

            return SettingRow("Sample rate", overrides.SetsSampleRate, group,
                value => overrides.SetsSampleRate = value);
        }

        private Toggle BuildFlag(bool value, Action<bool> setter)
        {
            Toggle toggle = new()
            {
                value = value
            };

            toggle.RegisterValueChangedCallback(evt =>
            {
                setter(evt.newValue);
                Changed?.Invoke();
            });

            return toggle;
        }

        private VisualElement SettingRow(string label, bool isSet, VisualElement control, Action<bool> setter)
        {
            VisualElement row = Row();

            Toggle enabled = new(label)
            {
                value = isSet,
                tooltip = "Off, this rule leaves the setting alone."
            };

            enabled.AddToClassList("ar-setting-toggle");

            control.SetEnabled(isSet);

            enabled.RegisterValueChangedCallback(evt =>
            {
                setter(evt.newValue);
                control.SetEnabled(evt.newValue);
                Changed?.Invoke();
            });

            control.style.flexGrow = 1f;

            row.Add(enabled);
            row.Add(control);

            return row;
        }

        private void AddCondition()
        {
            _rule.Conditions.Add(new AudioRuleCondition());

            Changed?.Invoke();
            Rebuild();
        }

        private void RemoveCondition(int index)
        {
            _rule.Conditions.RemoveAt(index);

            Changed?.Invoke();
            Rebuild();
        }
    }
}