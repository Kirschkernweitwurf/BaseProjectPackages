using System;
using System.Collections.Generic;
using Base.ToolsPackage.Editor.AudioRules.Data;
using UnityEngine.UIElements;

namespace Base.ToolsPackage.Editor.AudioRules.Window
{
    /// <summary>
    /// The rule list. Order is meaning here, so the list is reorderable by drag, and every row
    /// carries the number of clips it matched in the last scan. That badge is the fastest way to
    /// spot a rule that matches nothing, or one that quietly matches everything.
    /// </summary>
    internal sealed class AudioRuleListView : VisualElement
    {
        private const float RowHeight = 22f;

        /// <summary>Raised when the selected rule changes.</summary>
        internal event Action<AudioRule> SelectionChanged;

        /// <summary>Raised whenever the list itself was edited, so the owner can save and rescan.</summary>
        internal event Action Changed;

        private readonly ListView _list = new();
        private readonly Dictionary<string, int> _matchCounts = new();

        private List<AudioRule> _rules = new();

        /// <summary>Builds the list and its footer.</summary>
        public AudioRuleListView()
        {
            style.flexGrow = 1f;

            _list.fixedItemHeight = RowHeight;
            _list.reorderable = true;
            _list.reorderMode = ListViewReorderMode.Animated;
            _list.selectionType = SelectionType.Single;
            _list.showBoundCollectionSize = false;
            _list.style.flexGrow = 1f;
            _list.makeItem = MakeRow;
            _list.bindItem = BindRow;

            _list.itemIndexChanged += (_, _) => Changed?.Invoke();
            _list.selectionChanged += OnSelectionChanged;

            Add(_list);
            Add(BuildFooter());
        }

        /// <summary>Points the list at the rules of a rule set.</summary>
        /// <param name="rules">The live rule list, edited in place.</param>
        internal void SetRules(List<AudioRule> rules)
        {
            _rules = rules;
            _list.itemsSource = _rules;

            _list.Rebuild();
        }

        /// <summary>Feeds in how many clips each rule matched during the last scan.</summary>
        /// <param name="counts">Counts by rule label.</param>
        internal void SetMatchCounts(IReadOnlyDictionary<string, int> counts)
        {
            _matchCounts.Clear();

            foreach (KeyValuePair<string, int> pair in counts)
                _matchCounts[pair.Key] = pair.Value;

            _list.RefreshItems();
        }

        /// <summary>Redraws the rows, for example after a rule was renamed in the editor pane.</summary>
        internal void Refresh() => _list.RefreshItems();

        private static VisualElement MakeRow()
        {
            VisualElement row = new();

            row.AddToClassList(AudioRulesStyle.RuleRowClass);

            Toggle toggle = new();

            row.Add(toggle);

            Label label = new();

            label.AddToClassList(AudioRulesStyle.RuleLabelClass);
            row.Add(label);

            Label count = new();

            count.AddToClassList(AudioRulesStyle.BadgeClass);
            row.Add(count);

            Label target = new();

            target.AddToClassList(AudioRulesStyle.RuleTargetClass);
            row.Add(target);

            return row;
        }

        private static Button ToolButton(string text, string tooltip, Action onClick, bool isDanger)
        {
            Button button = new(onClick)
            {
                text = text,
                tooltip = tooltip
            };

            button.AddToClassList(AudioRulesStyle.ToolButtonClass);
            button.EnableInClassList(AudioRulesStyle.ToolButtonDangerClass, isDanger);

            return button;
        }

        private VisualElement BuildFooter()
        {
            VisualElement footer = new();

            footer.AddToClassList(AudioRulesStyle.FooterClass);

            footer.Add(ToolButton("Add", "Adds an empty rule at the end of the cascade.", AddRule, false));
            footer.Add(ToolButton("Duplicate", "Copies the selected rule, conditions and all.", DuplicateRule,
                false));

            footer.Add(ToolButton("Remove", "Deletes the selected rule.", RemoveRule, true));

            return footer;
        }

        private void BindRow(VisualElement element, int index)
        {
            AudioRule rule = _rules[index];
            Toggle toggle = element.Q<Toggle>();
            Label label = element.Q<Label>(className: AudioRulesStyle.RuleLabelClass);
            Label target = element.Q<Label>(className: AudioRulesStyle.RuleTargetClass);
            Label count = element.Q<Label>(className: AudioRulesStyle.BadgeClass);

            toggle.SetValueWithoutNotify(rule.Enabled);
            toggle.UnregisterCallback<ChangeEvent<bool>>(OnToggled);
            toggle.userData = rule;
            toggle.RegisterCallback<ChangeEvent<bool>>(OnToggled);

            label.text = rule.Label;
            target.text = rule.PlatformTarget;

            // Hiding instead of clearing keeps the column, so the counts stay in one line.
            target.style.visibility = rule.IsDefaultTarget
                ? Visibility.Hidden
                : Visibility.Visible;

            element.EnableInClassList(AudioRulesStyle.RuleRowOffClass, !rule.Enabled);

            _matchCounts.TryGetValue(rule.Label, out int matches);

            count.text = matches.ToString();
            count.EnableInClassList(AudioRulesStyle.BadgeZeroClass, matches == 0);
            count.tooltip = $"Matched {matches} {AudioRulesFormat.Plural(matches, "clip", "clips")} "
                + "in the last scan.";
        }

        private void OnToggled(ChangeEvent<bool> evt)
        {
            if (evt.target is not Toggle toggle
                || toggle.userData is not AudioRule rule)
                return;

            rule.Enabled = evt.newValue;

            Changed?.Invoke();
        }

        private void OnSelectionChanged(IEnumerable<object> selected)
        {
            foreach (object item in selected)
            {
                if (item is AudioRule rule)
                {
                    SelectionChanged?.Invoke(rule);
                    return;
                }
            }

            SelectionChanged?.Invoke(null);
        }

        private void AddRule()
        {
            _rules.Add(new AudioRule("New Rule"));

            _list.Rebuild();
            _list.selectedIndex = _rules.Count - 1;

            Changed?.Invoke();
        }

        private void DuplicateRule()
        {
            if (_list.selectedIndex < 0)
                return;

            AudioRule source = _rules[_list.selectedIndex];
            AudioRule copy = new(source.Label + " copy")
            {
                Enabled = source.Enabled,
                PlatformTarget = source.PlatformTarget,
                RequireAllConditions = source.RequireAllConditions
            };

            foreach (AudioRuleCondition condition in source.Conditions)
            {
                copy.Conditions.Add(new AudioRuleCondition(condition.Field, condition.Operator, condition.Number)
                {
                    Text = condition.Text
                });
            }

            source.Overrides.CopyTo(copy.Overrides);

            _rules.Insert(_list.selectedIndex + 1, copy);
            _list.Rebuild();

            Changed?.Invoke();
        }

        private void RemoveRule()
        {
            if (_list.selectedIndex < 0)
                return;

            _rules.RemoveAt(_list.selectedIndex);
            _list.Rebuild();

            SelectionChanged?.Invoke(null);
            Changed?.Invoke();
        }
    }
}