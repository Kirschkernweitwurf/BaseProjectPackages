using System;
using Base.ToolPackage.Editor.Shared;

namespace Base.ToolPackage.Editor.TodoOverview.Model
{
    /// <summary>
    /// One comment item found by the scan, with everything the list needs to draw it and everything
    /// the navigator needs to jump to it. Immutable, so a running query can never change a result.
    /// </summary>
    internal sealed class TodoEntry
    {
        private const string LineSeparator = ":";
        private const string Space = " ";

        /// <summary>The keyword that marked this item, in the casing the tag is configured with.</summary>
        internal string Keyword { get; }

        /// <summary>The text on the keyword's own line, with the metadata already cut out.</summary>
        internal string Message { get; }

        /// <summary>The continuation lines joined by line breaks, empty for a single line item.</summary>
        internal string Details { get; }

        /// <summary>The responsible person, or an empty string when none was named.</summary>
        internal string Owner { get; }

        /// <summary>The parsed date, or null when the item carries none.</summary>
        internal DateTime? Date { get; }

        /// <summary>The date exactly as it was written, shown when it could not be parsed.</summary>
        internal string RawDate { get; }

        /// <summary>Project relative path of the file the item sits in.</summary>
        internal string AssetPath { get; }

        /// <summary>File name of <see cref="AssetPath"/>, which is what the row shows.</summary>
        internal string FileName { get; }

        /// <summary>One based line number of the keyword.</summary>
        internal int Line { get; }

        /// <summary>Zero based column of the keyword, so the editor lands on the word itself.</summary>
        internal int Column { get; }

        /// <summary>How many source lines the item spans, including the keyword's own line.</summary>
        internal int LineCount { get; }

        /// <summary>Where the file the item sits in comes from.</summary>
        internal EAssetOrigin Origin { get; }

        /// <summary>File name and line, precomputed because it is drawn on every repaint.</summary>
        internal string Location { get; }

        /// <summary>Everything searchable about the item, lower cased once so filtering stays cheap.</summary>
        internal string SearchText { get; }

        /// <summary>Creates a single found item.</summary>
        /// <param name="keyword">The keyword that marked it.</param>
        /// <param name="message">The text on the keyword's line.</param>
        /// <param name="details">The continuation lines.</param>
        /// <param name="metadata">The owner and date read out of the text.</param>
        /// <param name="assetPath">Project relative path of the file.</param>
        /// <param name="fileName">File name of the path.</param>
        /// <param name="line">One based line number of the keyword.</param>
        /// <param name="column">Zero based column of the keyword.</param>
        /// <param name="lineCount">How many source lines the item spans.</param>
        /// <param name="origin">Where the file comes from.</param>
        internal TodoEntry(string keyword, string message, string details, TodoMetadata metadata, string assetPath,
            string fileName, int line, int column, int lineCount, EAssetOrigin origin)
        {
            Keyword = keyword;
            Message = message;
            Details = details;
            Owner = metadata.Owner;
            Date = metadata.Date;
            RawDate = metadata.RawDate;
            AssetPath = assetPath;
            FileName = fileName;
            Line = line;
            Column = column;
            LineCount = lineCount;
            Origin = origin;
            Location = fileName + LineSeparator + line;

            SearchText = string.Concat(keyword, Space, message, Space, details, Space, metadata.Owner, Space,
                    assetPath)
                .ToLowerInvariant();
        }
    }
}