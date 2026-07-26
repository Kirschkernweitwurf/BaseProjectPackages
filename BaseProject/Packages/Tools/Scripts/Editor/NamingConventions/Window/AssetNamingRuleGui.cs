using System;
using System.Collections.Generic;
using Base.ToolPackage.Editor.NamingConventions.Data;
using Base.ToolPackage.Editor.NamingConventions.Scanning;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Base.ToolPackage.Editor.NamingConventions.Window
{
    /// <summary>
    /// Draws the editable options and rule table of the asset naming window. Prefix and suffix
    /// lists are edited as comma separated text through delayed fields, so typing never fights
    /// with the value being rebuilt on every repaint. Everything the auto detection writes stays
    /// editable here, and every column carries a short tooltip with an example.
    /// </summary>
    public static class AssetNamingRuleGui
    {
        /// <summary>Returned when no row was marked for removal.</summary>
        public const int NoIndex = -1;

        private const string AnyTypeLabel = "Any Asset";
        private const string CustomTypeLabel = "Custom";
        private const float DigitsWidth = 42f;
        private const float EnabledWidth = 18f;
        private const float FilterWidth = 85f;
        private const float FragmentButtonWidth = 110f;
        private const float LabelWidth = 90f;
        private const float ListWidth = 80f;
        private const int MaxEnumerationDigits = 6;
        private const float PatternWidth = 85f;
        private const float RemoveWidth = 20f;
        private const string Separator = ", ";
        private const float StyleWidth = 95f;
        private const float TypeNameWidth = 130f;
        private const float TypeWidth = 100f;

        private static readonly GUIContent AddFragmentContent = new("Add Fragment",
            "Add another path text to skip. Example: TMP");

        private static readonly GUIContent AddRuleContent = new("Add Rule", "Add an empty rule to the table");

        private static readonly GUIContent CasingHeader = new("Casing",
            "How the name must be cased. Example: PascalCase");

        private static readonly GUIContent DigitsHeader = new("Digits",
            "Length of the number at the end. 0 allows any length. Example: 2 means _01");

        private static readonly GUIContent PackagesContent = new("Scan packages",
            "Also check assets in the Packages folder");

        private static readonly GUIContent PathHeader = new("Path Contains",
            "Only check assets whose path contains this text. Empty checks everywhere. Example: /Art/");

        private static readonly GUIContent PatternHeader = new("Pattern",
            "Optional regular expression. If set, nothing else is checked. Example: ^UI_[A-Z].*");

        private static readonly GUIContent PrefixesHeader = new("Prefixes",
            "The name must start with one of these, comma separated. Example: P_, SM_");

        private static readonly GUIContent RemoveContent = new("x", "Remove this entry");

        private static readonly GUIContent RuleHeader = new("Rule",
            "Name of the rule. Only shown in this window. Example: Prefabs");

        private static readonly GUIContent ScriptsContent = new("Scan scripts",
            "Also check .cs files. Careful: renaming a script breaks its class name.");

        private static readonly GUIContent SuffixesHeader = new("Suffixes",
            "The name must end with one of these, comma separated. Example: _Data");

        private static readonly GUIContent TypeHeader = new("Asset Type",
            "What kind of asset this rule checks. Example: Prefab");

        private static readonly GUIContent TypeNameHeader = new("Type Name",
            "The value behind the popup. Pick Custom to type your own. Empty means every asset. "
            + "Example: UnityEngine.Texture2D");

        private static readonly string[] TypeValues = BuildTypeValues();
        private static readonly string[] TypeLabels = BuildTypeLabels();

        /// <summary>
        /// Draws the scan options. Fragment rows change the control count, so additions and
        /// removals are reported back and applied by the window after the layout pass.
        /// </summary>
        public static bool DrawOptions(AssetNamingRuleSet ruleSet, bool isFragmentsExpanded,
            out bool isFragmentAddRequested, out int fragmentRemovalIndex)
        {
            isFragmentAddRequested = false;
            fragmentRemovalIndex = NoIndex;

            EditorGUI.BeginChangeCheck();

            ruleSet.IncludePackages = EditorGUILayout.ToggleLeft(PackagesContent, ruleSet.IncludePackages);
            ruleSet.IncludeScripts = EditorGUILayout.ToggleLeft(ScriptsContent, ruleSet.IncludeScripts);

            EditorGUILayout.Space(2f);

            isFragmentsExpanded = EditorGUILayout.Foldout(isFragmentsExpanded,
                $"Ignored Path Fragments ({ruleSet.IgnoredPathFragments.Count})", true);

            if (isFragmentsExpanded)
            {
                for (int index = 0; index < ruleSet.IgnoredPathFragments.Count; index++)
                {
                    if (DrawFragment(ruleSet, index))
                        fragmentRemovalIndex = index;
                }

                isFragmentAddRequested = GUILayout.Button(AddFragmentContent, GUILayout.Width(FragmentButtonWidth));
            }

            if (EditorGUI.EndChangeCheck())
                ruleSet.Persist();

            return isFragmentsExpanded;
        }

        /// <summary>Draws the rule table and returns the index the user asked to remove.</summary>
        public static int DrawRules(AssetNamingRuleSet ruleSet)
        {
            int removalIndex = NoIndex;

            EditorGUI.BeginChangeCheck();
            DrawHeader();

            for (int index = 0; index < ruleSet.Rules.Count; index++)
            {
                if (DrawRule(ruleSet.Rules[index]))
                    removalIndex = index;
            }

            if (EditorGUI.EndChangeCheck())
                ruleSet.Persist();

            return removalIndex;
        }

        /// <summary>True when the user asked for another rule.</summary>
        public static bool DrawAddButton() => GUILayout.Button(AddRuleContent, GUILayout.Width(FragmentButtonWidth));

        private static string[] BuildTypeValues() => new[]
        {
            string.Empty,
            AssetKindResolver.PrefabKind,
            AssetKindResolver.ModelKind,
            typeof(Texture2D).FullName,
            typeof(Sprite).FullName,
            typeof(Material).FullName,
            typeof(Shader).FullName,
            typeof(AudioClip).FullName,
            typeof(AnimationClip).FullName,
            typeof(AnimatorController).FullName,
            typeof(Mesh).FullName,
            typeof(Font).FullName,
            typeof(SceneAsset).FullName,
            typeof(ScriptableObject).FullName,
            typeof(MonoScript).FullName
        };

        private static string[] BuildTypeLabels()
        {
            // One extra slot for the Custom entry, which keeps the free text field in charge.
            string[] labels = new string[TypeValues.Length + 1];

            labels[0] = AnyTypeLabel;

            for (int index = 1; index < TypeValues.Length; index++)
            {
                string value = TypeValues[index];
                int separator = value.LastIndexOf('.');

                labels[index] = separator < 0
                    ? value
                    : value[(separator + 1)..];
            }

            labels[^1] = CustomTypeLabel;

            return labels;
        }

        private static bool DrawFragment(AssetNamingRuleSet ruleSet, int index)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                string edited = EditorGUILayout.DelayedTextField(ruleSet.IgnoredPathFragments[index]);

                if (edited != ruleSet.IgnoredPathFragments[index])
                    ruleSet.SetIgnoredFragmentAt(index, edited);

                return GUILayout.Button(RemoveContent, EditorStyles.miniButton, GUILayout.Width(RemoveWidth));
            }
        }

        private static void DrawHeader()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUIStyle style = EditorStyles.miniBoldLabel;

                GUILayout.Space(EnabledWidth + 4f);
                GUILayout.Label(RuleHeader, style, GUILayout.Width(LabelWidth));
                GUILayout.Label(TypeHeader, style, GUILayout.Width(TypeWidth));
                GUILayout.Label(TypeNameHeader, style, GUILayout.Width(TypeNameWidth));
                GUILayout.Label(PathHeader, style, GUILayout.Width(FilterWidth));
                GUILayout.Label(CasingHeader, style, GUILayout.Width(StyleWidth));
                GUILayout.Label(PrefixesHeader, style, GUILayout.Width(ListWidth));
                GUILayout.Label(SuffixesHeader, style, GUILayout.Width(ListWidth));
                GUILayout.Label(PatternHeader, style, GUILayout.Width(PatternWidth));
                GUILayout.Label(DigitsHeader, style, GUILayout.Width(DigitsWidth));
            }
        }

        /// <summary>Draws one rule row and returns true when the user asked to remove it.</summary>
        private static bool DrawRule(AssetNamingRule rule)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                rule.Enabled = EditorGUILayout.Toggle(rule.Enabled, GUILayout.Width(EnabledWidth));

                using (new EditorGUI.DisabledScope(!rule.Enabled))
                {
                    rule.Label = EditorGUILayout.DelayedTextField(rule.Label, GUILayout.Width(LabelWidth));

                    DrawTypePopup(rule);

                    rule.TypeName = EditorGUILayout.DelayedTextField(rule.TypeName, GUILayout.Width(TypeNameWidth));
                    rule.PathFilter = EditorGUILayout.DelayedTextField(rule.PathFilter, GUILayout.Width(FilterWidth));

                    NamingRule naming = rule.Naming;

                    naming.Style = (ENamingStyle)EditorGUILayout.EnumPopup(naming.Style, GUILayout.Width(StyleWidth));

                    DrawList(naming.Prefixes, ListWidth);
                    DrawList(naming.Suffixes, ListWidth);

                    naming.Pattern = EditorGUILayout.DelayedTextField(naming.Pattern, GUILayout.Width(PatternWidth));

                    int digits = EditorGUILayout.DelayedIntField(rule.EnumerationDigits, GUILayout.Width(DigitsWidth));
                    rule.EnumerationDigits = Mathf.Clamp(digits, 0, MaxEnumerationDigits);
                }

                // The remove button sits right after its row instead of at the window edge, so it
                // visually belongs to the rule it deletes.
                return GUILayout.Button(RemoveContent, EditorStyles.miniButton, GUILayout.Width(RemoveWidth));
            }
        }

        private static void DrawTypePopup(AssetNamingRule rule)
        {
            int current = IndexOfType(rule.TypeName);
            int selected = EditorGUILayout.Popup(current, TypeLabels, GUILayout.Width(TypeWidth));

            if (selected == current)
                return;

            // The custom entry has no value behind it and only keeps the text field in charge.
            if (selected >= TypeValues.Length)
                return;

            rule.TypeName = TypeValues[selected];
        }

        private static int IndexOfType(string typeName)
        {
            for (int index = 0; index < TypeValues.Length; index++)
            {
                if (TypeValues[index] == typeName)
                    return index;
            }

            return TypeLabels.Length - 1;
        }

        private static void DrawList(List<string> entries, float width)
        {
            string joined = string.Join(Separator, entries);
            string edited = EditorGUILayout.DelayedTextField(joined, GUILayout.Width(width));

            if (edited == joined)
                return;

            entries.Clear();

            foreach (string entry in edited.Split(','))
            {
                if (string.IsNullOrWhiteSpace(entry))
                    continue;

                entries.Add(entry.Trim());
            }
        }
    }
}
