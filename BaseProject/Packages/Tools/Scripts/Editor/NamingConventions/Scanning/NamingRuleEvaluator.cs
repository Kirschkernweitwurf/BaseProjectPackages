using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Base.ToolPackage.Editor.NamingConventions.Data;
using UnityEditor;

namespace Base.ToolPackage.Editor.NamingConventions.Scanning
{
    /// <summary>Checks names against a rule and builds the replacement name for a fix.</summary>
    public static class NamingRuleEvaluator
    {
        private const string Wildcard = "*";

        /// <summary>True when the name satisfies the rule or is on its ignore list.</summary>
        public static bool IsValid(NamingRule rule, string name)
        {
            if (IsIgnored(rule, name))
                return true;

            if (!string.IsNullOrEmpty(rule.Pattern))
                return IsPatternMatch(rule.Pattern, name);

            if (!HasAffix(rule.Prefixes, name, isPrefix: true))
                return false;

            if (!HasAffix(rule.Suffixes, name, isPrefix: false))
                return false;

            return NameStyleUtility.Matches(Core(rule, name), rule.Style);
        }

        /// <summary>Short explanation of why the name was rejected.</summary>
        public static string Reason(NamingRule rule, string name)
        {
            if (!string.IsNullOrEmpty(rule.Pattern))
                return $"Does not match the pattern {rule.Pattern}";

            if (!HasAffix(rule.Prefixes, name, isPrefix: true))
                return $"Missing prefix {string.Join(" or ", rule.Prefixes)}";

            if (!HasAffix(rule.Suffixes, name, isPrefix: false))
                return $"Missing suffix {string.Join(" or ", rule.Suffixes)}";

            return $"Expected {ObjectNames.NicifyVariableName(rule.Style.ToString())}";
        }

        /// <summary>
        /// Name that would satisfy the rule, derived from the current one. A prefix or suffix the
        /// name already carries is kept when the rule allows it, so "SM_Kitchen01" with the
        /// prefixes P_, S_ and SM_ suggests "SM_Kitchen_01" instead of switching to P_.
        /// </summary>
        public static string Suggest(NamingRule rule, string name)
        {
            string core = NameStyleUtility.Convert(Core(rule, name), rule.Style);

            if (core.Length == 0)
                return name;

            string prefix = FindMatchedAffix(rule.Prefixes, name, isPrefix: true);

            if (prefix.Length == 0)
                prefix = rule.PrimaryPrefix;

            string suffix = FindMatchedAffix(rule.Suffixes, name, isPrefix: false);

            if (suffix.Length == 0)
                suffix = rule.PrimarySuffix;

            return prefix + core + suffix;
        }

        /// <summary>True when the rule skips this name.</summary>
        public static bool IsIgnored(NamingRule rule, string name)
        {
            foreach (string entry in rule.IgnoredNames)
            {
                if (IsWildcardMatch(entry, name))
                    return true;
            }

            return false;
        }

        private static bool IsPatternMatch(string pattern, string name)
        {
            try
            {
                return Regex.IsMatch(name, pattern);
            }
            catch (ArgumentException)
            {
                // A broken expression is a rule authoring problem, not a naming problem. Letting the
                // name pass keeps the window usable while the pattern is being written.
                return true;
            }
        }

        private static bool IsWildcardMatch(string pattern, string name)
        {
            if (string.IsNullOrEmpty(pattern))
                return false;

            if (!pattern.Contains(Wildcard, StringComparison.Ordinal))
                return pattern == name;

            string expression = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";

            return Regex.IsMatch(name, expression);
        }

        private static string Core(NamingRule rule, string name)
        {
            string core = Strip(rule.Prefixes, name, isPrefix: true);
            core = Strip(rule.Suffixes, core, isPrefix: false);

            return core.Trim('_');
        }

        private static string Strip(List<string> affixes, string name, bool isPrefix)
        {
            string matched = FindMatchedAffix(affixes, name, isPrefix);

            if (matched.Length == 0)
                return name;

            return isPrefix
                ? name[matched.Length..]
                : name[..^matched.Length];
        }

        /// <summary>The longest affix the name carries, or an empty string when none matches.</summary>
        private static string FindMatchedAffix(List<string> affixes, string name, bool isPrefix)
        {
            string longest = string.Empty;

            foreach (string affix in affixes)
            {
                if (affix.Length <= longest.Length)
                    continue;

                if (IsAffixMatch(affix, name, isPrefix))
                    longest = affix;
            }

            return longest;
        }

        private static bool HasAffix(List<string> affixes, string name, bool isPrefix)
        {
            if (affixes.Count == 0)
                return true;

            return FindMatchedAffix(affixes, name, isPrefix).Length > 0;
        }

        private static bool IsAffixMatch(string affix, string name, bool isPrefix)
        {
            if (string.IsNullOrEmpty(affix))
                return false;

            if (name.Length <= affix.Length)
                return false;

            return isPrefix
                ? IsPrefixMatch(affix, name)
                : name.EndsWith(affix, StringComparison.Ordinal);
        }

        private static bool IsPrefixMatch(string affix, string name)
        {
            if (!name.StartsWith(affix, StringComparison.Ordinal))
                return false;

            // A letter prefix only counts when the rest starts a new word, so a texture called
            // "Trees" is reported as missing its T_ prefix instead of being read as "T" plus "rees".
            return !char.IsLetter(affix[^1])
                || char.IsUpper(name[affix.Length]);
        }
    }
}
