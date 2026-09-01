using System;
using System.Collections.Generic;

namespace Base.ToolPackage.Editor.AssetZoo.Generation
{
    /// <summary>
    /// Decides which leading name parts are naming prefixes ("SM", "P", "VFX") and which are real group
    /// names. Prefixes the user typed always win. Everything else has to look like a prefix and be
    /// shared by more than one asset before it is stripped.
    /// </summary>
    internal static class PrefixDetector
    {
        /// <summary>
        /// Resolves the prefixes for one scan.
        /// </summary>
        /// <param name="knownPrefixes">Prefixes the user typed. Always treated as prefixes.</param>
        /// <param name="firstTokens">First name part of every scanned asset, duplicates included.</param>
        /// <param name="autoDetect">False keeps the known prefixes and nothing else.</param>
        /// <param name="minOccurrences">How many assets have to share a token before it counts.</param>
        /// <param name="maxLength">Longest a token may be to still count as a prefix.</param>
        /// <param name="comparer">Decides whether prefix matching is case sensitive.</param>
        public static PrefixSet Build(IReadOnlyList<string> knownPrefixes, IReadOnlyList<string> firstTokens,
            bool autoDetect, int minOccurrences, int maxLength, StringComparer comparer)
        {
            Dictionary<string, int> orders = new(comparer);
            List<string> detected = new();
            List<string> suspects = new();

            foreach (string prefix in knownPrefixes)
            {
                string trimmed = prefix.Trim();
                if (trimmed.Length == 0
                    || orders.ContainsKey(trimmed))
                    continue;

                orders.Add(trimmed, orders.Count);
            }

            if (!autoDetect)
                return new PrefixSet(orders, detected, suspects);

            // Only the first part of a name is ever a candidate, so a short group name further along
            // in a name like "SM_Bar_Stool_01" can never be mistaken for a prefix.
            Dictionary<string, int> counts = new(comparer);
            foreach (string token in firstTokens)
            {
                if (orders.ContainsKey(token)
                    || !IsPrefixCandidate(token, maxLength))
                    continue;

                counts.TryGetValue(token, out int count);
                counts[token] = count + 1;
            }

            foreach (KeyValuePair<string, int> entry in counts)
            {
                if (entry.Value >= minOccurrences)
                    detected.Add(entry.Key);
                else
                    suspects.Add(entry.Key);
            }

            detected.Sort(StringComparer.OrdinalIgnoreCase);
            suspects.Sort(StringComparer.OrdinalIgnoreCase);

            foreach (string prefix in detected)
                orders.Add(prefix, orders.Count);

            return new PrefixSet(orders, detected, suspects);
        }

        private static bool IsPrefixCandidate(string token, int maxLength)
        {
            if (token.Length > maxLength)
                return false;

            // Naming prefixes are uppercase abbreviations. Demanding that keeps a short group name
            // like "Rock" out, which would otherwise turn "Rock_01, Rock_02" into the groups "01, 02".
            bool hasUppercase = false;

            foreach (char character in token)
            {
                if (char.IsLower(character))
                    return false;

                hasUppercase = hasUppercase || char.IsUpper(character);
            }

            return hasUppercase;
        }
    }
}