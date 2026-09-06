using System;
using System.Text.RegularExpressions;
using Base.ToolsPackage.Editor.TodoOverview.Model;

namespace Base.ToolsPackage.Editor.TodoOverview.Scanning
{
    /// <summary>
    /// Reads the responsible person and the date out of an item's text and hands back the message that
    /// is left. Each configured pattern is tried in turn and only counts when it fills something that
    /// is still missing, so a general pattern can complete what a more specific one did not carry.
    /// <para>
    /// A pattern says what its date means by which group it puts it in, so an item can override the
    /// project's reading of a bare date without any new syntax being baked in here.
    /// </para>
    /// </summary>
    internal static class TodoMetadataParser
    {
        private const string DoubleSpace = "  ";
        private const string SingleSpace = " ";

        private static readonly char[] Separators =
        {
            ':',
            '-',
            '>',
            ',',
            ' ',
            '\t'
        };

        /// <summary>Splits an item's text into message, owner and date.</summary>
        /// <param name="message">The text that follows the keyword.</param>
        /// <param name="patterns">The compiled patterns of this scan.</param>
        /// <returns>What was recognized, together with the remaining message.</returns>
        internal static TodoMetadata Parse(string message, TodoPatterns patterns)
        {
            string remaining = message;
            string owner = string.Empty;
            string rawDate = string.Empty;
            ETodoDateMeaning? meaning = null;

            foreach (Regex pattern in patterns.Metadata)
            {
                if (owner.Length > 0
                    && rawDate.Length > 0)
                    break;

                Match match = TryMatch(pattern, remaining);

                if (match == null || !match.Success)
                    continue;

                string matchedOwner = Read(match, TodoGroupNames.Owner);
                string matchedDate = ReadDate(match, out ETodoDateMeaning? matchedMeaning);

                bool addsOwner = owner.Length == 0 && matchedOwner.Length > 0;
                bool addsDate = rawDate.Length == 0 && matchedDate.Length > 0;

                if (!addsOwner && !addsDate)
                    continue;

                if (addsOwner)
                    owner = matchedOwner;

                if (addsDate)
                {
                    rawDate = matchedDate;
                    meaning = matchedMeaning;
                }

                remaining = remaining.Remove(match.Index, match.Length);
            }

            DateTime? date = TodoDateParser.TryParse(rawDate, patterns.DateFormats, out DateTime parsed)
                ? parsed
                : null;

            return new TodoMetadata(Clean(remaining), owner, rawDate, date, meaning);
        }

        private static Match TryMatch(Regex pattern, string text)
        {
            try
            {
                return pattern.Match(text);
            }
            catch (RegexMatchTimeoutException)
            {
                return null;
            }
        }

        // A marked group is read before the plain one, so a pattern that carries both by accident
        // still says something definite rather than depending on which group name sorts first.
        private static string ReadDate(Match match, out ETodoDateMeaning? meaning)
        {
            string due = Read(match, TodoGroupNames.Due);

            if (due.Length > 0)
            {
                meaning = ETodoDateMeaning.Due;
                return due;
            }

            string written = Read(match, TodoGroupNames.Written);

            if (written.Length > 0)
            {
                meaning = ETodoDateMeaning.Written;
                return written;
            }

            meaning = null;
            return Read(match, TodoGroupNames.Date);
        }

        private static string Read(Match match, string group)
        {
            Group value = match.Groups[group];

            return value.Success
                ? value.Value.Trim()
                : string.Empty;
        }

        // What is left after cutting the metadata out still carries the punctuation that separated it
        // from the message, and a gap where it used to sit.
        private static string Clean(string message)
        {
            string trimmed = message.Trim(Separators);

            while (trimmed.Contains(DoubleSpace, StringComparison.Ordinal))
                trimmed = trimmed.Replace(DoubleSpace, SingleSpace, StringComparison.Ordinal);

            return trimmed;
        }
    }
}