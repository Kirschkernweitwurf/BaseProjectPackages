using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Base.ToolPackage.Editor.NamingConventions.Data;
using UnityEditor;

namespace Base.ToolPackage.Editor.NamingConventions.Scanning
{
    /// <summary>
    /// Checks names against a rule and builds the replacement name for a fix. A required suffix
    /// can be handed in for assets whose sub type is known, for example a normal map that has to
    /// end in _N.
    /// </summary>
    internal static class NamingRuleEvaluator
    {
        private const string Wildcard = "*";

        private static readonly char[] ForbiddenSeparators =
        {
            ' ',
            '-'
        };

        /// <summary>True when the name satisfies the rule or is on its ignore list.</summary>
        public static bool IsValid(NamingRule rule, string name, string requiredSuffix)
        {
            if (IsIgnored(rule, name))
                return true;

            if (!string.IsNullOrEmpty(rule.Pattern))
                return IsPatternMatch(rule.Pattern, name);

            if (HasForbiddenSeparator(name))
                return false;

            if (FindStripped(rule, name).Length > 0)
                return false;

            if (!HasAffix(rule.Prefixes, name, true))
                return false;

            if (!HasSuffix(rule, name, requiredSuffix))
                return false;

            return NameStyleUtility.Matches(Core(rule, name, requiredSuffix), rule.Style);
        }

        /// <summary>Short explanation of why the name was rejected.</summary>
        public static string Reason(NamingRule rule, string name, string requiredSuffix)
        {
            if (!string.IsNullOrEmpty(rule.Pattern))
                return $"Does not match the pattern {rule.Pattern}";

            if (HasForbiddenSeparator(name))
                return "Contains a space or dash";

            string stripped = FindStripped(rule, name);

            if (stripped.Length > 0)
                return $"Should not contain {stripped}";

            if (!HasAffix(rule.Prefixes, name, true))
                return $"Missing prefix {string.Join(" or ", rule.Prefixes)}";

            if (requiredSuffix.Length > 0
                && !name.EndsWith(requiredSuffix, StringComparison.Ordinal))
                return $"Should end with {requiredSuffix}";

            if (!HasSuffix(rule, name, requiredSuffix))
                return $"Missing suffix {string.Join(" or ", rule.Suffixes)}";

            return $"Expected {ObjectNames.NicifyVariableName(rule.Style.ToString())}";
        }

        /// <summary>
        /// Name that would satisfy the rule, derived from the current one. A prefix or suffix the
        /// name already carries is kept when the rule allows it, so "SM_Kitchen01" with the
        /// prefixes P_, S_ and SM_ suggests "SM_Kitchen_01" instead of switching to P_.
        /// </summary>
        public static string Suggest(NamingRule rule, string name, string requiredSuffix)
        {
            string core = NameStyleUtility.Convert(Core(rule, name, requiredSuffix), rule.Style);

            if (core.Length == 0)
                return name;

            string prefix = FindMatchedAffix(rule.Prefixes, name, true);

            if (prefix.Length == 0)
                prefix = rule.PrimaryPrefix;

            return prefix + core + SuggestSuffix(rule, name, requiredSuffix);
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

        /// <summary>The first entry of the strip list the name carries, at either end.</summary>
        private static string FindStripped(NamingRule rule, string name)
        {
            foreach (string entry in rule.Stripped)
            {
                if (string.IsNullOrWhiteSpace(entry))
                    continue;

                if (name.StartsWith(entry, StringComparison.Ordinal)
                    || name.EndsWith(entry, StringComparison.Ordinal))
                    return entry;
            }

            return string.Empty;
        }

        private static string SuggestSuffix(NamingRule rule, string name, string requiredSuffix)
        {
            if (requiredSuffix.Length > 0)
                return requiredSuffix;

            string matched = FindMatchedAffix(rule.Suffixes, name, false);

            if (matched.Length > 0)
                return matched;

            // An optional suffix is only added when the name already had one, so assets without a
            // sub type keep their plain name instead of being pushed into the first entry.
            return rule.SuffixOptional
                ? string.Empty
                : rule.PrimarySuffix;
        }

        private static bool HasSuffix(NamingRule rule, string name, string requiredSuffix)
        {
            if (requiredSuffix.Length > 0)
                return name.EndsWith(requiredSuffix, StringComparison.Ordinal);

            if (rule.Suffixes.Count == 0)
                return true;

            if (FindMatchedAffix(rule.Suffixes, name, false).Length > 0)
                return true;

            return rule.SuffixOptional;
        }

        /// <summary>Spaces and dashes are never allowed in an asset name.</summary>
        private static bool HasForbiddenSeparator(string name) => name.IndexOfAny(ForbiddenSeparators) >= 0;

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

        private static string Core(NamingRule rule, string name, string requiredSuffix)
        {
            string core = StripAll(rule, name);

            core = Strip(rule.Prefixes, core, true);
            core = Strip(rule.Suffixes, core, false);

            if (requiredSuffix.Length > 0
                && core.EndsWith(requiredSuffix, StringComparison.Ordinal))
                core = core[..^requiredSuffix.Length];

            return core.Trim('_');
        }

        /// <summary>Removes every entry of the strip list, from the front and from the back.</summary>
        private static string StripAll(NamingRule rule, string name)
        {
            string core = name;
            string found = FindStripped(rule, core);

            while (found.Length > 0
                   && core.Length > found.Length)
            {
                core = core.StartsWith(found, StringComparison.Ordinal)
                    ? core[found.Length..]
                    : core[..^found.Length];

                found = FindStripped(rule, core);
            }

            return core;
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