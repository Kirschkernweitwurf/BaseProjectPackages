using System.Collections.Generic;
using System.Text;

namespace Base.DataHubPackage.Editor.Sheets
{
    /// <summary>
    /// Writes and reads the plain text a spreadsheet exchanges: comma separated for a file, tab separated for
    /// the clipboard. Fields with the separator, a quote or a line break are quoted, as every spreadsheet
    /// expects, so a description with a comma survives the round trip.
    /// </summary>
    internal static class DataSheetFormat
    {
        /// <summary>The separator of a CSV file.</summary>
        internal const char Comma = ',';

        /// <summary>The separator spreadsheets use on the clipboard.</summary>
        internal const char Tab = '\t';

        /// <summary>The separator a German Excel writes into a CSV file, where the comma is the decimal mark.</summary>
        internal const char Semicolon = ';';

        private const char CarriageReturn = '\r';
        private const char LineFeed = '\n';
        private const char Quote = '"';
        private const string RowEnd = "\r\n";

        /// <summary>Writes rows of fields as text.</summary>
        /// <param name="rows">The rows, the first one usually the titles.</param>
        /// <param name="separator">The field separator.</param>
        /// <returns>The text.</returns>
        internal static string Write(IEnumerable<IReadOnlyList<string>> rows, char separator)
        {
            StringBuilder builder = new();

            foreach (IReadOnlyList<string> row in rows)
            {
                for (int i = 0; i < row.Count; i++)
                {
                    if (i > 0)
                        builder.Append(separator);

                    AppendField(builder, row[i] ?? string.Empty, separator);
                }

                builder.Append(RowEnd);
            }

            return builder.ToString();
        }

        /// <summary>Reads text into rows of fields. Blank lines are skipped.</summary>
        /// <param name="text">The text.</param>
        /// <param name="separator">The field separator.</param>
        /// <returns>The rows.</returns>
        internal static List<List<string>> Read(string text, char separator)
        {
            List<List<string>> rows = new();
            List<string> row = new();
            StringBuilder field = new();
            bool isQuoted = false;

            for (int i = 0; i < text.Length; i++)
            {
                char current = text[i];

                if (isQuoted)
                {
                    if (current != Quote)
                    {
                        field.Append(current);
                        continue;
                    }

                    // A doubled quote inside a quoted field is one quote character.
                    if (i + 1 < text.Length
                        && text[i + 1] == Quote)
                    {
                        field.Append(Quote);
                        i++;

                        continue;
                    }

                    isQuoted = false;
                    continue;
                }

                if (current == Quote
                    && field.Length == 0)
                {
                    isQuoted = true;
                    continue;
                }

                if (current == separator)
                {
                    row.Add(field.ToString());
                    field.Clear();

                    continue;
                }

                if (current == CarriageReturn)
                    continue;

                if (current == LineFeed)
                {
                    EndRow(rows, row, field);
                    row = new List<string>();

                    continue;
                }

                field.Append(current);
            }

            EndRow(rows, row, field);

            return rows;
        }

        /// <summary>
        /// Guesses the separator of pasted or opened text from its first line: tab when it has one, then
        /// semicolon, comma otherwise.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The separator.</returns>
        internal static char Detect(string text)
        {
            int lineEnd = text.IndexOf(LineFeed);

            string firstLine = lineEnd < 0
                ? text
                : text.Substring(0, lineEnd);

            if (firstLine.IndexOf(Tab) >= 0)
                return Tab;

            return firstLine.IndexOf(Semicolon) >= 0
                ? Semicolon
                : Comma;
        }

        private static void AppendField(StringBuilder builder, string value, char separator)
        {
            bool needsQuotes = value.IndexOf(separator) >= 0
                || value.IndexOf(Quote) >= 0
                || value.IndexOf(LineFeed) >= 0
                || value.IndexOf(CarriageReturn) >= 0;

            if (!needsQuotes)
            {
                builder.Append(value);
                return;
            }

            builder.Append(Quote);
            builder.Append(value.Replace(Quote.ToString(), new string(Quote, 2)));
            builder.Append(Quote);
        }

        private static void EndRow(List<List<string>> rows, List<string> row, StringBuilder field)
        {
            row.Add(field.ToString());
            field.Clear();

            // A blank line, or the empty line after the last row break, is no data.
            if (row.Count == 1
                && row[0].Length == 0)
                return;

            rows.Add(row);
        }
    }
}