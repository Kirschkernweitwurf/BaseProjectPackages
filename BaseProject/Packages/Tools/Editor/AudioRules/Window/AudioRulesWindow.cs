using System;
using System.Collections.Generic;
using System.Linq;
using Base.ToolPackage.Editor.AudioRules.Apply;
using Base.ToolPackage.Editor.AudioRules.Data;
using Base.ToolPackage.Editor.AudioRules.Model;
using Base.ToolPackage.Editor.AudioRules.Scanning;
using Base.UtilityPackage.Logging;
using Base.UtilityPackage.Menus;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Base.ToolPackage.Editor.AudioRules.Window
{
    /// <summary>
    /// The audio rules window. Three panes that can all be dragged to any size: the rule list on
    /// the left, the results table on the right, the details of whatever is selected underneath.
    /// <para>
    /// A scan never writes anything. It resolves what the rules want and shows it as a diff, and
    /// only Apply touches an importer. Reading sample data is the slow half of a scan, so it runs
    /// in the background a few clips at a time and streams its findings into the table instead of
    /// blocking behind a modal progress bar.
    /// </para>
    /// </summary>
    public sealed class AudioRulesWindow : EditorWindow
    {
        /// <summary>Label the default import settings are shown with in the target dropdown.</summary>
        public const string DefaultTargetLabel = "Default";

        private const int AnalysisBatch = 3;
        private const long AnalysisInterval = 16L;
        private const float DetailsHeight = 240f;
        private const string MenuPath = "Tools/Base Packages/Assets/Audio Rules";
        private const float RulesWidth = 250f;
        private const string StyleSheetFilter = "AudioRulesWindow t:StyleSheet";
        private const string WindowTitle = "Audio Rules";

        private static readonly Vector2 MinWindowSize = new(940f, 520f);

        private readonly List<AudioClipPlan> _plans = new();
        private readonly List<AudioClipPlan> _pending = new();
        private readonly Dictionary<string, int> _matchCounts = new();

        [SerializeField] private AudioRuleSet ruleSet;

        private AudioClipDetailsView _details;
        private AudioClipTableView _table;
        private AudioRuleEditorView _ruleEditor;
        private AudioRuleListView _ruleList;
        private AudioRulesMessageView _message;
        private AudioRulesPane _clipsPane;
        private AudioRulesPane _detailsPane;
        private AudioRulesPane _rulesPane;
        private Button _applyButton;
        private DropdownField _targetDropdown;
        private IVisualElementScheduledItem _analysisLoop;
        private Label _status;
        private ObjectField _ruleSetField;
        private ToolbarToggle _onlyChanges;
        private ToolbarToggle _onlyFindings;
        private VisualElement _body;
        private VisualElement _progress;
        private VisualElement _progressFill;
        private int _analysisTotal;
        private string _platform = string.Empty;
        private string _search = string.Empty;
        private bool _hasScanned;
        private bool _showsRuleEditor;

#region Unity Callbacks
        private void CreateGUI()
        {
            if (ruleSet == null)
                ruleSet = AudioRuleSet.Load();

            rootVisualElement.AddToClassList("ar-root");

            if (!EditorGUIUtility.isProSkin)
                rootVisualElement.AddToClassList("ar-light");

            LoadStyleSheet();

            rootVisualElement.Add(BuildToolbar());

            _body = new VisualElement
            {
                style =
                {
                    flexGrow = 1f
                }
            };

            rootVisualElement.Add(_body);
            rootVisualElement.Add(BuildStatusBar());

            BuildPanes();
            BindRuleSet();
        }

        private void OnDisable()
        {
            StopAnalysis();
            AudioPreviewPlayer.Stop();
        }
#endregion

        /// <summary>Opens or focuses the window.</summary>
        [DynamicMenuItem(MenuPath)]
        public static void Open()
        {
            AudioRulesWindow window = GetWindow<AudioRulesWindow>();

            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = MinWindowSize;
            window.Show();
        }

        private static bool Matches(AudioClipPlan plan, string term)
            => plan.Info.AssetPath.Contains(term, StringComparison.OrdinalIgnoreCase)
            || plan.PrimaryRule.Contains(term, StringComparison.OrdinalIgnoreCase)
            || plan.Info.Category.Contains(term, StringComparison.OrdinalIgnoreCase);

        private static Label Chip(string text, string variant)
        {
            Label chip = new(text);

            chip.AddToClassList("ar-chip");

            if (!string.IsNullOrEmpty(variant))
                chip.AddToClassList(variant);

            return chip;
        }

        private VisualElement BuildToolbar()
        {
            Toolbar toolbar = new();

            toolbar.AddToClassList("ar-toolbar");

            _ruleSetField = new ObjectField
            {
                objectType = typeof(AudioRuleSet),
                allowSceneObjects = false,
                value = ruleSet
            };

            _ruleSetField.AddToClassList("ar-ruleset");
            _ruleSetField.RegisterValueChangedCallback(evt =>
            {
                ruleSet = evt.newValue as AudioRuleSet;
                BindRuleSet();
            });

            toolbar.Add(_ruleSetField);
            toolbar.Add(new ToolbarButton(Rescan)
            {
                text = "Scan",
                tooltip = "Compares every clip against the rules and, unless it is turned off in the rule set, "
                    + "reads sample data in the background afterwards."
            });

            _targetDropdown = BuildTargetDropdown();

            toolbar.Add(_targetDropdown);

            _onlyChanges = new ToolbarToggle
            {
                text = "Changes"
            };

            _onlyChanges.RegisterValueChangedCallback(_ => RefreshTable());
            toolbar.Add(_onlyChanges);

            _onlyFindings = new ToolbarToggle
            {
                text = "Findings"
            };

            _onlyFindings.RegisterValueChangedCallback(_ => RefreshTable());
            toolbar.Add(_onlyFindings);

            toolbar.Add(new ToolbarSpacer
            {
                style =
                {
                    flexGrow = 1f
                }
            });

            ToolbarSearchField search = new();

            search.AddToClassList("ar-search");
            search.RegisterValueChangedCallback(evt =>
            {
                _search = evt.newValue;
                RefreshTable();
            });

            toolbar.Add(search);

            return toolbar;
        }

        private DropdownField BuildTargetDropdown()
        {
            List<string> choices = new()
            {
                DefaultTargetLabel
            };

            DropdownField target = new(choices, 0)
            {
                tooltip = "Which import target is shown and written. A platform without its own override "
                    + "inherits the default settings."
            };

            target.AddToClassList("ar-target");
            target.RegisterValueChangedCallback(evt =>
            {
                _platform = evt.newValue == DefaultTargetLabel
                    ? string.Empty
                    : evt.newValue;

                Rescan();
            });

            return target;
        }

        private void BuildPanes()
        {
            _ruleList = new AudioRuleListView();
            _ruleList.SelectionChanged += OnRuleSelected;
            _ruleList.Changed += OnRulesEdited;

            _table = new AudioClipTableView();
            _table.SelectionChanged += OnClipSelected;
            _table.SelectionCountChanged += UpdateApplyButton;

            _message = new AudioRulesMessageView();

            _rulesPane = new AudioRulesPane("RULES");
            _rulesPane.Body.Add(_ruleList);

            _clipsPane = new AudioRulesPane("CLIPS");
            _clipsPane.Body.Add(_table);
            _clipsPane.Body.Add(_message);

            _ruleEditor = new AudioRuleEditorView();
            _ruleEditor.Changed += OnRulesEdited;

            _details = new AudioClipDetailsView();

            _detailsPane = new AudioRulesPane("CLIP");
            _detailsPane.Body.Add(_details);

            TwoPaneSplitView top = new(0, RulesWidth, TwoPaneSplitViewOrientation.Horizontal);

            top.Add(_rulesPane);
            top.Add(_clipsPane);

            TwoPaneSplitView split = new(1, DetailsHeight, TwoPaneSplitViewOrientation.Vertical);

            split.Add(top);
            split.Add(_detailsPane);
            split.style.flexGrow = 1f;

            _body.Add(split);
        }

        private VisualElement BuildStatusBar()
        {
            VisualElement bar = new();

            bar.AddToClassList("ar-status");

            _status = new Label(string.Empty);
            _status.AddToClassList("ar-status__text");

            _progressFill = new VisualElement();
            _progressFill.AddToClassList("ar-progress__fill");

            _progress = new VisualElement();
            _progress.AddToClassList("ar-progress");
            _progress.Add(_progressFill);
            _progress.style.display = DisplayStyle.None;

            _applyButton = new Button(ApplyChanges)
            {
                text = "Apply"
            };

            _applyButton.AddToClassList("ar-primary");

            bar.Add(_status);
            bar.Add(_progress);
            bar.Add(_applyButton);

            return bar;
        }

        private void LoadStyleSheet()
        {
            foreach (string guid in AssetDatabase.FindAssets(StyleSheetFilter))
            {
                StyleSheet sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath(guid));

                if (sheet == null)
                    continue;

                rootVisualElement.styleSheets.Add(sheet);

                return;
            }
        }

        private void BindRuleSet()
        {
            StopAnalysis();

            _plans.Clear();
            _hasScanned = false;

            if (ruleSet == null)
            {
                _ruleList.SetRules(new List<AudioRule>());
                _table.SetItems(Array.Empty<AudioClipPlan>());

                ShowMessage(AudioRulesMessageView.NeutralGlyph, "No rule set yet",
                    "A rule set holds the import conventions of this project as a plain asset, so they are "
                    + "versioned with it. The default cascade is a good starting point and every rule can be "
                    + "changed afterwards.", null, "Create Rule Set", CreateRuleSet);

                _status.text = string.Empty;

                UpdateApplyButton();

                return;
            }

            ruleSet.EnsureDefaults();
            RefreshTargetChoices();

            _ruleSetField.SetValueWithoutNotify(ruleSet);
            _details.SetRuleSet(ruleSet);
            _ruleList.SetRules(ruleSet.Rules);
            _ruleEditor.SetRule(null, ruleSet.Platforms);

            Rescan();
        }

        private void CreateRuleSet()
        {
            ruleSet = AudioRuleSet.Create();

            Selection.activeObject = ruleSet;

            BindRuleSet();
        }

        // The platform list lives in the rule set, so the dropdown only knows it once one is assigned.
        private void RefreshTargetChoices()
        {
            List<string> choices = new()
            {
                DefaultTargetLabel
            };

            choices.AddRange(ruleSet.Platforms);

            _targetDropdown.choices = choices;

            if (!choices.Contains(_targetDropdown.value))
                _targetDropdown.SetValueWithoutNotify(DefaultTargetLabel);
        }

        private void OnRuleSelected(AudioRule rule)
        {
            _showsRuleEditor = rule != null;

            _detailsPane.Body.Clear();

            if (rule == null)
            {
                _detailsPane.SetTitle("CLIP");
                _detailsPane.SetNote(string.Empty);
                _detailsPane.Body.Add(_details);

                return;
            }

            _detailsPane.SetTitle("RULE");
            _detailsPane.SetNote(rule.Label);
            _ruleEditor.SetRule(rule, ruleSet.Platforms);
            _detailsPane.Body.Add(_ruleEditor);
        }

        private void OnClipSelected(AudioClipPlan plan)
        {
            if (plan == null)
                return;

            if (_showsRuleEditor)
            {
                _showsRuleEditor = false;

                _detailsPane.Body.Clear();
                _detailsPane.Body.Add(_details);
            }

            _detailsPane.SetTitle("CLIP");
            _detailsPane.SetNote(plan.Info.Name);
            _details.SetPlan(plan);
        }

        // Rules are edited in place, so the asset has to be written and the results are stale.
        private void OnRulesEdited()
        {
            if (ruleSet == null)
                return;

            ruleSet.Persist();

            _ruleList.Refresh();

            Rescan();
        }

        private void Rescan()
        {
            if (ruleSet == null)
                return;

            StopAnalysis();

            _matchCounts.Clear();
            _plans.Clear();
            _plans.AddRange(AudioScanService.Scan(ruleSet, _platform, _matchCounts));
            _hasScanned = true;

            _ruleList.SetMatchCounts(_matchCounts);
            _rulesPane.SetNote($"{ruleSet.Rules.Count} in cascade");
            _details.SetPlan(null);

            StartAnalysis();
            RefreshTable();
        }

        // Whatever the cache knows is free, so it lands before the first repaint. The rest streams
        // in over the next frames instead of freezing the editor behind a modal bar.
        private void StartAnalysis()
        {
            _pending.Clear();

            if (!ruleSet.AnalyzeSampleData)
                return;

            foreach (AudioClipPlan plan in _plans)
            {
                if (!AudioScanService.FillFromCache(plan, ruleSet.Analysis))
                    _pending.Add(plan);
            }

            _analysisTotal = _pending.Count;

            if (_analysisTotal == 0)
                return;

            _progress.style.display = DisplayStyle.Flex;
            _analysisLoop = rootVisualElement.schedule.Execute(AnalysisStep).Every(AnalysisInterval);
        }

        private void AnalysisStep()
        {
            int budget = Mathf.Min(AnalysisBatch, _pending.Count);

            for (int step = 0; step < budget; step++)
            {
                AudioScanService.AnalyzeOne(_pending[_pending.Count - 1], ruleSet.Analysis);
                _pending.RemoveAt(_pending.Count - 1);
            }

            UpdateProgress();

            if (_pending.Count > 0)
                return;

            StopAnalysis();
            RefreshTable();
        }

        private void StopAnalysis()
        {
            if (_analysisLoop != null)
            {
                _analysisLoop.Pause();
                _analysisLoop = null;

                AudioScanService.FlushCache();
            }

            _pending.Clear();
            _analysisTotal = 0;

            if (_progress != null)
                _progress.style.display = DisplayStyle.None;
        }

        private void UpdateProgress()
        {
            float done = _analysisTotal - _pending.Count;

            _progressFill.style.width = Length.Percent(done / Mathf.Max(1, _analysisTotal) * 100f);
            _table.Refresh();

            _clipsPane.SetNote($"listening to {_pending.Count} more");
        }

        private List<AudioClipPlan> Filtered()
        {
            IEnumerable<AudioClipPlan> query = _plans;

            if (_onlyChanges.value)
                query = query.Where(plan => plan.HasChanges);

            if (_onlyFindings.value)
                query = query.Where(plan => plan.Findings.Count > 0);

            if (!string.IsNullOrWhiteSpace(_search))
            {
                string term = _search.Trim();

                query = query.Where(plan => Matches(plan, term));
            }

            return query.ToList();
        }

        private void RefreshTable()
        {
            if (ruleSet == null)
                return;

            List<AudioClipPlan> filtered = Filtered();

            _table.SetItems(filtered);
            UpdateStatus(filtered);
            UpdateEmptyState(filtered);
        }

        private void UpdateEmptyState(List<AudioClipPlan> filtered)
        {
            int changes = _plans.Count(plan => plan.HasChanges);
            int findings = _plans.Count(plan => plan.Findings.Count > 0);

            if (!_hasScanned)
            {
                ShowTable();
                return;
            }

            if (_plans.Count == 0)
            {
                ShowMessage(AudioRulesMessageView.NeutralGlyph, "No clips in scope",
                    "Nothing matched the scan scope.\nCheck the ignored path fragments in the rule set.", null,
                    null, null);

                return;
            }

            if (changes == 0
                && findings == 0
                && _pending.Count == 0)
            {
                ShowMessage(AudioRulesMessageView.SuccessGlyph, "Everything matches",
                    $"All {_plans.Count} clips are imported the way the rules want them, and nothing turned up "
                    + "in the sample data.", "ar-good", null, null);

                return;
            }

            if (filtered.Count == 0)
            {
                ShowMessage(AudioRulesMessageView.NeutralGlyph, "Nothing to show",
                    "No clip matches the current filters.", null, null, null);

                return;
            }

            ShowTable();
        }

        private void ShowTable()
        {
            _table.style.display = DisplayStyle.Flex;
            _message.style.display = DisplayStyle.None;
        }

        private void ShowMessage(string glyph, string title, string body, string variant, string buttonText,
            Action onClick)
        {
            _message.Show(glyph, title, body, variant, buttonText, onClick);
            _message.style.display = DisplayStyle.Flex;
            _table.style.display = DisplayStyle.None;
        }

        private void UpdateStatus(List<AudioClipPlan> filtered)
        {
            List<AudioClipPlan> changed = filtered.Where(plan => plan.HasChanges).ToList();
            long build = changed.Sum(plan => plan.BuildDelta);
            long runtime = changed.Sum(plan => plan.RuntimeDelta);
            int findings = filtered.Count(plan => plan.Findings.Count > 0);

            _clipsPane.SetNote($"{filtered.Count} of {_plans.Count}");

            _clipsPane.HeaderRight.Clear();
            _clipsPane.HeaderRight.Add(Chip($"{changed.Count} to change", changed.Count > 0
                ? "ar-chip--warn"
                : null));

            _clipsPane.HeaderRight.Add(Chip($"{findings} with findings", findings > 0
                ? "ar-chip--bad"
                : null));

            _clipsPane.HeaderRight.Add(SizeChip("Build", build));
            _clipsPane.HeaderRight.Add(SizeChip("Memory", runtime));

            _status.text = $"{_plans.Count} clips scanned for the {TargetName()} target. Sizes are estimates.";

            UpdateApplyButton();
        }

        private static Label SizeChip(string label, long delta)
        {
            if (delta == 0L)
                return Chip($"{label} unchanged", null);

            return Chip($"{label} {AudioRulesFormat.Delta(delta)}", delta > 0L
                ? "ar-chip--good"
                : "ar-chip--bad");
        }

        private string TargetName() => string.IsNullOrEmpty(_platform)
            ? DefaultTargetLabel
            : _platform;

        private void UpdateApplyButton()
        {
            if (_applyButton == null)
                return;

            int selected = _table.Selection.Count(plan => plan.HasChanges);
            int all = Filtered().Count(plan => plan.HasChanges);
            int count = selected > 0
                ? selected
                : all;

            _applyButton.text = selected > 0
                ? $"Apply Selected ({selected})"
                : $"Apply All ({all})";

            _applyButton.SetEnabled(count > 0);
        }

        private void ApplyChanges()
        {
            List<AudioClipPlan> selected = _table.Selection.Where(plan => plan.HasChanges).ToList();
            List<AudioClipPlan> targets = selected.Count > 0
                ? selected
                : Filtered().Where(plan => plan.HasChanges).ToList();

            if (targets.Count == 0)
                return;

            bool confirmed = EditorUtility.DisplayDialog(WindowTitle,
                $"Reimport {targets.Count} {AudioRulesFormat.Plural(targets.Count, "clip", "clips")} with the "
                + $"settings the rules want, for the {TargetName()} target?", "Apply", "Cancel");

            if (!confirmed)
                return;

            int changed = AudioSettingsApplier.Apply(targets, _platform);

            CustomLogger.Log($"Applied the audio rules to {changed} "
                + $"{AudioRulesFormat.Plural(changed, "clip", "clips")}.", ruleSet);

            Rescan();
        }
    }
}