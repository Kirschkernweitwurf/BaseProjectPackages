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
    /// Draws the editable options and rule table of the asset naming window. Both tables are laid
    /// out by explicit rectangles through <see cref="AssetNamingColumnLayout"/>, so the header and
    /// every row line up exactly and each column can be resized by dragging its divider. List
    /// columns are edited as comma separated text through delayed fields, so typing never fights
    /// with the value being rebuilt on every repaint.
    /// </summary>
    internal static class AssetNamingRuleGui
    {
        /// <summary>Returned when no row was marked for removal.</summary>
        internal const int NoIndex = -1;

        private const string AnyTypeLabel = "Any Asset";
        private const string CustomTypeLabel = "Custom";
        private const float EditedMarkerWidth = 2f;
        private const float FragmentGap = 1f;
        private const int MaxEnumerationDigits = 6;
        private const string PrefsKey = "Base.AssetNaming.RuleColumns";
        private const float RemoveWidth = 20f;
        private const string Separator = ", ";

        private static readonly AssetNamingColumnLayout Columns = new(PrefsKey,
            26f, 90f, 100f, 130f, 90f, 130f, 80f, 80f, 30f, 80f, 85f, 46f);

        private static readonly GUIContent[] Headers =
        {
            new("On", "Turn the rule off without deleting it"),
            new("Rule", "Name of the rule. Only shown in this window. A blue bar on the left means the "
                + "rule was created or changed by hand, so auto-detect leaves those fields alone. "
                + "Example: \"Prefab\""),
            new("Asset Type", "What kind of asset this rule checks. Sprite, NormalMap and the other "
                + "texture kinds come from the importer, Texture2D means every texture. Example: \"Sprite\""),
            new("Type Name", "The value behind the popup. Pick Custom to type your own. "
                + "Empty means every asset. Example: \"UnityEngine.Texture2D\""),
            new("Path Contains", "Only check assets whose path contains this text. "
                + "Empty checks everywhere. Example: \"/Art/\""),
            new("Casing", "How the name must be cased. Example: \"PascalCase\""),
            new("Prefixes", "The name must start with one of these, comma separated. The first one is "
                + "used when a fix is suggested. Example: \"P_, SM_\""),
            new("Suffixes", "The name may end with one of these, comma separated. The first one is "
                + "used when a fix is suggested. Example: \"_D, _N, _R\""),
            new("Opt", "If on, a suffix is allowed but not demanded. Turn it off when every asset of "
                + "this kind has to carry one."),
            new("Strip", "Text that has to go, comma separated. Dropped from the front or the back "
                + "before the name is checked. Example: \"Icon_, _Old\""),
            new("Pattern", "Advanced. A regular expression the whole name must match. Only needed for "
                + "shapes the other columns cannot express, like a word count or a length limit. "
                + "If set, nothing else is checked. "
                + "Example: \"^[A-Z][a-z]+_[A-Z][a-z]+$\" allows exactly two words, so Kitchen_Lamp "
                + "passes but Kitchen_Lamp_Small does not"),
            new("Digits", "Length of the number at the end. 0 allows any length. Example: \"2\" means _01")
        };

        private static readonly GUIContent AddFragmentContent = new("Add Fragment",
            "Add another path text to skip. Example: \"/TextMesh Pro/\"");

        private static readonly GUIContent AddRuleContent = new("Add Rule", "Add an empty rule to the table");

        private static readonly GUIContent PackagesContent = new("Scan packages",
            "Also check assets in the Packages folder");

        private static readonly GUIContent RemoveContent = new("x", "Remove this entry");

        private static readonly GUIContent ScriptsContent = new("Scan scripts",
            "Also check .cs files. Careful: renaming a script breaks its class name.");

        private static readonly string[] TypeValues = BuildTypeValues();
        private static readonly string[] TypeLabels = BuildTypeLabels();

        /// <summary>
        /// Draws the scan options. Fragment rows change the control count, so additions and
        /// removals are reported back and applied by the window after the layout pass.
        /// </summary>
        internal static bool DrawOptions(AssetNamingRuleSet ruleSet, bool isFragmentsExpanded,
            out bool isFragmentAddRequested, out int fragmentRemovalIndex)
        {
            isFragmentAddRequested = false;
            fragmentRemovalIndex = NoIndex;

            EditorGUI.BeginChangeCheck();

            ruleSet.IncludePackages = EditorGUILayout.ToggleLeft(PackagesContent, ruleSet.IncludePackages);
            ruleSet.IncludeScripts = EditorGUILayout.ToggleLeft(ScriptsContent, ruleSet.IncludeScripts);

            EditorGUILayout.Space(FragmentGap);

            GUIContent fragmentsLabel = new($"Ignored Path Fragments ({ruleSet.IgnoredPathFragments.Count})",
                "Assets whose path contains one of these are skipped by every rule. "
                + "Example: \"/TextMesh Pro/\"");

            isFragmentsExpanded = EditorGUILayout.Foldout(isFragmentsExpanded, fragmentsLabel, true);

            if (isFragmentsExpanded)
            {
                fragmentRemovalIndex = DrawFragments(ruleSet);
                isFragmentAddRequested = GUILayout.Button(AddFragmentContent, GUILayout.Width(110f));
            }

            if (EditorGUI.EndChangeCheck())
                ruleSet.Persist();

            return isFragmentsExpanded;
        }

        /// <summary>Draws the rule table and returns the index the user asked to remove.</summary>
        internal static int DrawRules(AssetNamingRuleSet ruleSet)
        {
            int removalIndex = NoIndex;
            float width = Columns.TotalWidth + RemoveWidth + AssetNamingGui.Padding;

            Rect header = GUILayoutUtility.GetRect(width, AssetNamingGui.RowHeight, GUILayout.MinWidth(width));

            AssetNamingGui.DrawHeaderBackground(header);
            Columns.DrawHeader(header, Headers);

            EditorGUI.BeginChangeCheck();

            Rect area = GUILayoutUtility.GetRect(width, ruleSet.Rules.Count * AssetNamingGui.RowHeight,
                GUILayout.MinWidth(width));

            for (int index = 0; index < ruleSet.Rules.Count; index++)
            {
                Rect row = new(area.x, area.y + index * AssetNamingGui.RowHeight, width, AssetNamingGui.RowHeight);

                AssetNamingGui.DrawRowBackground(row, index);
                DrawEditedMarker(row, ruleSet.Rules[index]);

                if (DrawRule(row, ruleSet.Rules[index]))
                    removalIndex = index;
            }

            if (EditorGUI.EndChangeCheck())
                ruleSet.Persist();

            return removalIndex;
        }

        /// <summary>True when the user asked for another rule.</summary>
        internal static bool DrawAddButton() => GUILayout.Button(AddRuleContent, GUILayout.Width(110f));

        private static string[] BuildTypeValues() => new[]
        {
            string.Empty,
            AssetKindResolver.PrefabKind,
            AssetKindResolver.ModelKind,
            AssetKindResolver.SpriteKind,
            AssetKindResolver.NormalMapKind,
            AssetKindResolver.LightmapKind,
            AssetKindResolver.CursorKind,
            AssetKindResolver.CookieKind,
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

        /// <summary>
        /// Draws the ignore list as a striped table like the rules, and returns the index the user
        /// asked to remove.
        /// </summary>
        private static int DrawFragments(AssetNamingRuleSet ruleSet)
        {
            int removalIndex = NoIndex;
            int count = ruleSet.IgnoredPathFragments.Count;
            Rect area = GUILayoutUtility.GetRect(0f, count * AssetNamingGui.RowHeight, GUILayout.ExpandWidth(true));

            for (int index = 0; index < count; index++)
            {
                Rect row = new(area.x, area.y + index * AssetNamingGui.RowHeight, area.width,
                    AssetNamingGui.RowHeight);

                AssetNamingGui.DrawRowBackground(row, index);

                if (DrawFragment(ruleSet, row, index))
                    removalIndex = index;
            }

            return removalIndex;
        }

        private static bool DrawFragment(AssetNamingRuleSet ruleSet, Rect row, int index)
        {
            float padding = AssetNamingGui.Padding;
            float width = Mathf.Max(padding, row.width - RemoveWidth - padding * 3f);
            Rect fieldRect = new(row.x + padding, row.y + 2f, width, row.height - 4f);
            Rect removeRect = new(fieldRect.xMax + padding, row.y + 2f, RemoveWidth, row.height - 4f);

            string edited = EditorGUI.DelayedTextField(fieldRect, ruleSet.IgnoredPathFragments[index]);

            if (edited != ruleSet.IgnoredPathFragments[index])
                ruleSet.SetIgnoredFragmentAt(index, edited);

            return GUI.Button(removeRect, RemoveContent, EditorStyles.miniButton);
        }

        /// <summary>Draws one rule row and returns true when the user asked to remove it.</summary>
        private static bool DrawRule(Rect row, AssetNamingRule rule)
        {
            bool enabled = EditorGUI.Toggle(Columns.Field(row, 0), rule.Enabled);

            if (enabled != rule.Enabled)
            {
                rule.Enabled = enabled;
                rule.MarkEdited(EAssetNamingField.Enabled);
            }

            using (new EditorGUI.DisabledScope(!rule.Enabled))
            {
                NamingRule naming = rule.Naming;

                DrawText(Columns.Field(row, 1), rule, EAssetNamingField.Label, rule.Label,
                    apply: value => rule.Label = value);

                DrawTypePopup(Columns.Field(row, 2), rule);

                DrawText(Columns.Field(row, 3), rule, EAssetNamingField.TypeName, rule.TypeName,
                    apply: value => rule.TypeName = value);

                DrawText(Columns.Field(row, 4), rule, EAssetNamingField.PathFilter, rule.PathFilter,
                    apply: value => rule.PathFilter = value);

                ENamingStyle style = (ENamingStyle)EditorGUI.Popup(Columns.Field(row, 5), (int)naming.Style,
                    AssetNamingGui.StyleLabels);

                if (style != naming.Style)
                {
                    naming.Style = style;
                    rule.MarkEdited(EAssetNamingField.Style);
                }

                DrawList(Columns.Field(row, 6), rule, EAssetNamingField.Prefixes, naming.Prefixes);
                DrawList(Columns.Field(row, 7), rule, EAssetNamingField.Suffixes, naming.Suffixes);

                bool optional = EditorGUI.Toggle(Columns.Field(row, 8), naming.SuffixOptional);

                if (optional != naming.SuffixOptional)
                {
                    naming.SuffixOptional = optional;
                    rule.MarkEdited(EAssetNamingField.SuffixOptional);
                }

                DrawList(Columns.Field(row, 9), rule, EAssetNamingField.Stripped, naming.Stripped);

                DrawText(Columns.Field(row, 10), rule, EAssetNamingField.Pattern, naming.Pattern,
                    apply: value => naming.Pattern = value);

                int digits = Mathf.Clamp(EditorGUI.DelayedIntField(Columns.Field(row, 11), rule.EnumerationDigits),
                    0, MaxEnumerationDigits);

                if (digits != rule.EnumerationDigits)
                {
                    rule.EnumerationDigits = digits;
                    rule.MarkEdited(EAssetNamingField.Digits);
                }
            }

            // The remove button sits right behind its row instead of at the window edge, so it
            // visually belongs to the rule it deletes.
            Rect removeRect = new(row.x + Columns.TotalWidth, row.y + 3f, RemoveWidth,
                AssetNamingGui.RowHeight - 6f);

            return GUI.Button(removeRect, RemoveContent, EditorStyles.miniButton);
        }

        /// <summary>Marks the rule with a bar on the left when it carries changes made by hand.</summary>
        private static void DrawEditedMarker(Rect row, AssetNamingRule rule)
        {
            if (!rule.HasUserEdits)
                return;

            if (Event.current.type != EventType.Repaint)
                return;

            EditorGUI.DrawRect(new Rect(row.x, row.y + 2f, EditedMarkerWidth, row.height - 4f),
                AssetNamingGui.RulesAccent);
        }

        private static void DrawTypePopup(Rect rect, AssetNamingRule rule)
        {
            int current = IndexOfType(rule.TypeName);
            int selected = EditorGUI.Popup(rect, current, TypeLabels);

            if (selected == current)
                return;

            // The custom entry has no value behind it and only keeps the text field in charge.
            if (selected >= TypeValues.Length)
                return;

            rule.TypeName = TypeValues[selected];
            rule.MarkEdited(EAssetNamingField.TypeName);
        }

        /// <summary>Draws a delayed text field that records the edit when the value changes.</summary>
        private static void DrawText(Rect rect, AssetNamingRule rule, EAssetNamingField field, string current,
            Action<string> apply)
        {
            string edited = EditorGUI.DelayedTextField(rect, current);

            if (edited == current)
                return;

            apply(edited);
            rule.MarkEdited(field);
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

        private static void DrawList(Rect rect, AssetNamingRule rule, EAssetNamingField field, List<string> entries)
        {
            string joined = string.Join(Separator, entries);
            string edited = EditorGUI.DelayedTextField(rect, joined);

            if (edited == joined)
                return;

            rule.MarkEdited(field);
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