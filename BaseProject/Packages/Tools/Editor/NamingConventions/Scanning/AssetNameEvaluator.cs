using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Base.ToolsPackage.Editor.NamingConventions.Data;

namespace Base.ToolsPackage.Editor.NamingConventions.Scanning
{
    /// <summary>
    /// Checks asset file names against a rule. The suffix is split off first, then the number, so
    /// a name always reads Prefix_Base_Number_Suffix and "T_PhoneScreen_N_2" is fixed to
    /// "T_PhoneScreen_02_N". The rest of the name goes through the shared
    /// <see cref="NamingRuleEvaluator"/>.
    /// </summary>
    internal static class AssetNameEvaluator
    {
        private const string DigitFormatPrefix = "D";
        private const char EnumerationSeparator = '_';

        // Lazy core plus an optional underscore, so both "Lamp_01" and "Lamp01" split into
        // "Lamp" and "01".
        private static readonly Regex EnumerationPattern = new("^(.+?)_?([0-9]+)$", RegexOptions.Compiled);

        /// <summary>True when the file name satisfies the rule.</summary>
        public static bool IsValid(AssetNamingRule rule, string fileName, string requiredSuffix)
        {
            string core = Split(rule, fileName, requiredSuffix, out string number, out string tail);

            if (!HasValidEnumerationLength(rule, number))
                return false;

            // A number without its underscore is a violation on its own, so "Lamp01" gets fixed
            // to "Lamp_01" even when everything else is already correct.
            if (number.Length > 0
                && !HasEnumerationSeparator(fileName, number, tail))
                return false;

            return NamingRuleEvaluator.IsValid(rule.Naming, core + tail, requiredSuffix);
        }

        /// <summary>Short explanation of why the file name was rejected.</summary>
        public static string Reason(AssetNamingRule rule, string fileName, string requiredSuffix)
        {
            string core = Split(rule, fileName, requiredSuffix, out string number, out string tail);

            if (!HasValidEnumerationLength(rule, number))
                return $"Number should have {rule.EnumerationDigits} digits";

            if (number.Length > 0
                && !HasEnumerationSeparator(fileName, number, tail))
                return "Missing _ before number";

            return NamingRuleEvaluator.Reason(rule.Naming, core + tail, requiredSuffix);
        }

        /// <summary>File name that would satisfy the rule, derived from the current one.</summary>
        public static string Suggest(AssetNamingRule rule, string fileName, string requiredSuffix)
        {
            string core = Split(rule, fileName, requiredSuffix, out string number, out string tail);
            string body = NamingRuleEvaluator.Suggest(rule.Naming, core + tail, requiredSuffix);

            if (number.Length == 0)
                return body;

            // The number belongs in front of the suffix, so the name reads
            // Prefix_Base_Number_Suffix like the convention asks for.
            string suffix = FindSuffix(rule, body, requiredSuffix);

            return body[..^suffix.Length] + EnumerationSeparator + FormatNumber(rule, number) + suffix;
        }

        /// <summary>Splits a trailing number off a name. Returns false when there is none.</summary>
        public static bool TrySplitEnumeration(string fileName, out string core, out string number)
        {
            core = fileName;
            number = string.Empty;

            if (string.IsNullOrEmpty(fileName))
                return false;

            Match match = EnumerationPattern.Match(fileName);

            if (!match.Success)
                return false;

            // A name that is nothing but digits has no core to check, so it stays as it is.
            if (IsAllDigits(match.Groups[1].Value))
                return false;

            core = match.Groups[1].Value;
            number = match.Groups[2].Value;

            return true;
        }

        /// <summary>
        /// Cuts the name into the part before the number, the number itself and the suffix behind
        /// it. So "T_Rock_01_N" reads as the core "T_Rock", the number "01" and the tail "_N".
        /// </summary>
        private static string Split(AssetNamingRule rule, string fileName, string requiredSuffix, out string number,
            out string tail)
        {
            number = string.Empty;
            tail = FindSuffix(rule, fileName, requiredSuffix);

            string head = tail.Length > 0
                ? fileName[..^tail.Length]
                : fileName;

            return TrySplitEnumeration(head, out string core, out number)
                ? core
                : head;
        }

        /// <summary>Longest suffix the name carries, from the rule or from the asset importer.</summary>
        private static string FindSuffix(AssetNamingRule rule, string name, string requiredSuffix)
        {
            string longest = string.Empty;

            if (requiredSuffix.Length > 0
                && EndsWithSuffix(name, requiredSuffix))
                longest = requiredSuffix;

            foreach (string suffix in rule.Naming.Suffixes)
            {
                if (suffix.Length <= longest.Length)
                    continue;

                if (EndsWithSuffix(name, suffix))
                    longest = suffix;
            }

            return longest;
        }

        private static bool EndsWithSuffix(string name, string suffix) => suffix.Length > 0
            && name.Length > suffix.Length
            && name.EndsWith(suffix, StringComparison.Ordinal);

        private static bool HasValidEnumerationLength(AssetNamingRule rule, string number)
        {
            if (number.Length == 0)
                return true;

            if (rule.EnumerationDigits <= 0)
                return true;

            return number.Length == rule.EnumerationDigits;
        }

        private static bool HasEnumerationSeparator(string fileName, string number, string tail)
        {
            int separatorIndex = fileName.Length - tail.Length - number.Length - 1;

            return separatorIndex >= 0
                && fileName[separatorIndex] == EnumerationSeparator;
        }

        private static bool IsAllDigits(string value)
        {
            foreach (char symbol in value)
            {
                if (!char.IsDigit(symbol))
                    return false;
            }

            return true;
        }

        private static string FormatNumber(AssetNamingRule rule, string number)
        {
            if (rule.EnumerationDigits <= 0)
                return number;

            if (!int.TryParse(number, NumberStyles.None, CultureInfo.InvariantCulture, out int value))
                return number;

            return value.ToString(DigitFormatPrefix + rule.EnumerationDigits, CultureInfo.InvariantCulture);
        }
    }
}