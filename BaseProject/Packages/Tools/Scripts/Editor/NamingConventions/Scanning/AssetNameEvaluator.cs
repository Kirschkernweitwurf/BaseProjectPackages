using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Base.ToolPackage.Editor.NamingConventions.Data;

namespace Base.ToolPackage.Editor.NamingConventions.Scanning
{
    /// <summary>
    /// Checks asset file names against a rule. A trailing number is split off first, with or
    /// without an underscore, then the remaining name goes through the shared
    /// <see cref="NamingRuleEvaluator"/>. So "P_Street_Lamp01" validates its casing on
    /// "P_Street_Lamp" and the suggestion becomes "P_StreetLamp_01", normalized underscore
    /// included. A required suffix from the asset importer, for example _N for a normal map, is
    /// passed straight through.
    /// </summary>
    public static class AssetNameEvaluator
    {
        private const string DigitFormatPrefix = "D";
        private const char EnumerationSeparator = '_';

        // Lazy core plus an optional underscore, so both "Lamp_01" and "Lamp01" split into
        // "Lamp" and "01".
        private static readonly Regex EnumerationPattern = new("^(.+?)_?([0-9]+)$", RegexOptions.Compiled);

        /// <summary>True when the file name satisfies the rule.</summary>
        public static bool IsValid(AssetNamingRule rule, string fileName, string requiredSuffix)
        {
            string core = SplitEnumeration(fileName, requiredSuffix, out string number, out string tail);

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
            string core = SplitEnumeration(fileName, requiredSuffix, out string number, out string tail);

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
            string core = SplitEnumeration(fileName, requiredSuffix, out string number, out string tail);

            if (number.Length == 0)
                return NamingRuleEvaluator.Suggest(rule.Naming, core + tail, requiredSuffix);

            // The number keeps its place in front of the suffix, so "T_Rock1_N" becomes
            // "T_Rock_01_N" instead of losing its sub type marker.
            string body = NamingRuleEvaluator.Suggest(rule.Naming, core + tail, requiredSuffix);
            string suffix = FindTrailingSuffix(body, tail, requiredSuffix);

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
        /// Splits the number off the name. When a suffix is required the name is cut in front of
        /// it first, so "T_Rock_01_N" is read as the core "T_Rock", the number "01" and the tail
        /// "_N".
        /// </summary>
        private static string SplitEnumeration(string fileName, string requiredSuffix, out string number,
            out string tail)
        {
            number = string.Empty;
            tail = string.Empty;

            string head = fileName;

            if (requiredSuffix.Length > 0
                && fileName.EndsWith(requiredSuffix, StringComparison.Ordinal))
            {
                head = fileName[..^requiredSuffix.Length];
                tail = requiredSuffix;
            }

            return TrySplitEnumeration(head, out string core, out number)
                ? core
                : head;
        }

        private static string FindTrailingSuffix(string body, string tail, string requiredSuffix)
        {
            if (tail.Length > 0
                && body.EndsWith(tail, StringComparison.Ordinal))
                return tail;

            if (requiredSuffix.Length > 0
                && body.EndsWith(requiredSuffix, StringComparison.Ordinal))
                return requiredSuffix;

            return string.Empty;
        }

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
