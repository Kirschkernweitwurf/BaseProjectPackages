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
    /// included.
    /// </summary>
    public static class AssetNameEvaluator
    {
        private const string DigitFormatPrefix = "D";
        private const char EnumerationSeparator = '_';

        // Lazy core plus an optional underscore, so both "Lamp_01" and "Lamp01" split into
        // "Lamp" and "01".
        private static readonly Regex EnumerationPattern = new("^(.+?)_?([0-9]+)$", RegexOptions.Compiled);

        /// <summary>True when the file name satisfies the rule.</summary>
        public static bool IsValid(AssetNamingRule rule, string fileName)
        {
            string core = SplitEnumeration(fileName, out string number);

            if (!HasValidEnumerationLength(rule, number))
                return false;

            // A number without its underscore is a violation on its own, so "Lamp01" gets fixed
            // to "Lamp_01" even when everything else is already correct.
            if (number.Length > 0
                && !HasEnumerationSeparator(fileName, number))
                return false;

            return NamingRuleEvaluator.IsValid(rule.Naming, core);
        }

        /// <summary>Short explanation of why the file name was rejected.</summary>
        public static string Reason(AssetNamingRule rule, string fileName)
        {
            string core = SplitEnumeration(fileName, out string number);

            if (!HasValidEnumerationLength(rule, number))
                return $"Number should have {rule.EnumerationDigits} digits";

            if (number.Length > 0
                && !HasEnumerationSeparator(fileName, number))
                return "Missing _ before number";

            return NamingRuleEvaluator.Reason(rule.Naming, core);
        }

        /// <summary>File name that would satisfy the rule, derived from the current one.</summary>
        public static string Suggest(AssetNamingRule rule, string fileName)
        {
            string core = SplitEnumeration(fileName, out string number);
            string suggestion = NamingRuleEvaluator.Suggest(rule.Naming, core);

            if (number.Length == 0)
                return suggestion;

            return suggestion + EnumerationSeparator + FormatNumber(rule, number);
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

        private static string SplitEnumeration(string fileName, out string number)
        {
            return TrySplitEnumeration(fileName, out string core, out number)
                ? core
                : fileName;
        }

        private static bool HasValidEnumerationLength(AssetNamingRule rule, string number)
        {
            if (number.Length == 0)
                return true;

            if (rule.EnumerationDigits <= 0)
                return true;

            return number.Length == rule.EnumerationDigits;
        }

        private static bool HasEnumerationSeparator(string fileName, string number)
        {
            int separatorIndex = fileName.Length - number.Length - 1;

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
