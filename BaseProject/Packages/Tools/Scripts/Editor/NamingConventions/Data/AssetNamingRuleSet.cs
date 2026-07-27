using System;
using System.Collections.Generic;
using System.IO;
using Base.ToolPackage.MenuManagerWindow;
using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.NamingConventions.Data
{
    /// <summary>
    /// The asset naming conventions of a project. Stored as a plain asset next to the code rule
    /// set, so the conventions are versioned with the project and every machine scans against the
    /// same rules. New sets start empty because asset prefixes differ from project to project, the
    /// auto detection fills them from the assets that already exist.
    /// </summary>
    [DynamicCreateAssetMenu("Scriptable Objects/Base/Naming Conventions/New Asset Rule Set", "ANRS_AssetNamingRuleSet")]
    public sealed class AssetNamingRuleSet : ScriptableObject
    {
        private const string AssetFilter = "t:" + nameof(AssetNamingRuleSet);
        private const string DefaultAssetName = "ANRS_AssetNamingRuleSet.asset";
        private const string DefaultFolder = "Assets/Editor/NamingConventions";
        private const string FolderSeparator = "/";
        private const int DefaultsVersion = 2;
        private const string ScriptExtension = ".cs";

        private static readonly string[] DefaultIgnoredPaths =
        {
            "/Plugins/",
            "/ThirdParty/",
            "/Generated/",
            "/TextMesh Pro/",
            "/Fonts & Materials/",
            "/AddressableAssetsData/"
        };

        [Tooltip("One rule per asset group. The first rule that applies to an asset wins.")]
        [SerializeField] private List<AssetNamingRule> rules = new();

        [Tooltip("If true, assets inside the Packages folder are scanned as well.")]
        [SerializeField] private bool includePackages;

        [Tooltip("If true, scripts are scanned too. Renaming a script breaks its class name.")]
        [SerializeField] private bool includeScripts;

        [Tooltip("Assets whose path contains one of these fragments are skipped, for example /Fonts & Materials/.")]
        [SerializeField] private List<string> ignoredPathFragments = new();

        [Tooltip("Set of defaults this asset was last filled with. Raised when new ones ship.")]
        [SerializeField] private int defaultsVersion;

        /// <summary>Rules applied while scanning.</summary>
        public IReadOnlyList<AssetNamingRule> Rules => rules;

        /// <summary>Path fragments that exclude an asset from the scan.</summary>
        public IReadOnlyList<string> IgnoredPathFragments => ignoredPathFragments;

        /// <summary>If true, assets inside the Packages folder are scanned as well.</summary>
        public bool IncludePackages
        {
            get => includePackages;
            set => includePackages = value;
        }

        /// <summary>If true, scripts are scanned too. Renaming a script breaks its class name.</summary>
        public bool IncludeScripts
        {
            get => includeScripts;
            set => includeScripts = value;
        }

        /// <summary>Returns the first asset rule set found in the project, or null when there is none.</summary>
        public static AssetNamingRuleSet Load()
        {
            foreach (string guid in AssetDatabase.FindAssets(AssetFilter))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AssetNamingRuleSet found = AssetDatabase.LoadAssetAtPath<AssetNamingRuleSet>(path);

                if (found == null)
                    continue;

                found.EnsureDefaults();
                return found;
            }

            return null;
        }

        /// <summary>Creates an empty rule set asset that is ready for the auto detection.</summary>
        public static AssetNamingRuleSet Create()
        {
            Directory.CreateDirectory(DefaultFolder);
            AssetDatabase.Refresh();

            AssetNamingRuleSet instance = CreateInstance<AssetNamingRuleSet>();
            instance.EnsureDefaults();

            string path = AssetDatabase.GenerateUniqueAssetPath(DefaultFolder + FolderSeparator + DefaultAssetName);

            AssetDatabase.CreateAsset(instance, path);
            AssetDatabase.SaveAssets();
            CustomLogger.Log($"Created an asset naming rule set at {path}", instance);

            return instance;
        }

        /// <summary>
        /// Fills the ignore list on first use and tops it up when the tool ships new defaults.
        /// Only missing entries are added, so a fragment deleted on purpose stays gone until the
        /// next set of defaults arrives.
        /// </summary>
        public void EnsureDefaults()
        {
            if (defaultsVersion >= DefaultsVersion)
                return;

            bool isChanged = false;

            foreach (string fragment in DefaultIgnoredPaths)
            {
                if (ignoredPathFragments.Contains(fragment))
                    continue;

                ignoredPathFragments.Add(fragment);
                isChanged = true;
            }

            defaultsVersion = DefaultsVersion;
            Persist();

            if (isChanged)
                CustomLogger.Log("Added the new default path fragments to the asset naming rules.", this);
        }

        /// <summary>Appends a rule to the end of the list.</summary>
        public void AddRule(AssetNamingRule rule)
        {
            if (rule == null)
            {
                CustomLogger.LogError($"Cannot add an empty {nameof(AssetNamingRule)}.", this);
                return;
            }

            rules.Add(rule);
        }

        /// <summary>Removes the rule at the given index.</summary>
        public void RemoveRuleAt(int index)
        {
            if (!IsInRange(index, rules.Count))
            {
                CustomLogger.LogError($"Rule index {index} is out of range.", this);
                return;
            }

            rules.RemoveAt(index);
        }

        /// <summary>Drops all rules and takes over the given ones.</summary>
        public void ReplaceRules(IEnumerable<AssetNamingRule> newRules)
        {
            if (newRules == null)
            {
                CustomLogger.LogError("Cannot replace the rules with an empty list.", this);
                return;
            }

            rules.Clear();
            rules.AddRange(newRules);
        }

        /// <summary>Appends an empty entry to the ignore list.</summary>
        public void AddIgnoredFragment(string fragment) => ignoredPathFragments.Add(fragment ?? string.Empty);

        /// <summary>Overwrites the ignore entry at the given index.</summary>
        public void SetIgnoredFragmentAt(int index, string fragment)
        {
            if (!IsInRange(index, ignoredPathFragments.Count))
            {
                CustomLogger.LogError($"Ignore index {index} is out of range.", this);
                return;
            }

            ignoredPathFragments[index] = fragment ?? string.Empty;
        }

        /// <summary>Removes the ignore entry at the given index.</summary>
        public void RemoveIgnoredFragmentAt(int index)
        {
            if (!IsInRange(index, ignoredPathFragments.Count))
            {
                CustomLogger.LogError($"Ignore index {index} is out of range.", this);
                return;
            }

            ignoredPathFragments.RemoveAt(index);
        }

        /// <summary>True when the asset is excluded from the scan.</summary>
        public bool IsIgnoredPath(string path)
        {
            if (!includeScripts
                && path.EndsWith(ScriptExtension, StringComparison.OrdinalIgnoreCase))
                return true;

            foreach (string fragment in ignoredPathFragments)
            {
                if (string.IsNullOrWhiteSpace(fragment))
                    continue;

                if (path.Contains(fragment, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        /// <summary>Writes the asset to disk so the rules end up in source control.</summary>
        public void Persist()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }

        private static bool IsInRange(int index, int count) => index >= 0 && index < count;
    }
}
