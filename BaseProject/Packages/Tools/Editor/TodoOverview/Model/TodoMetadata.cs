using System;

namespace Base.ToolsPackage.Editor.TodoOverview.Model
{
    /// <summary>
    /// What a metadata pattern pulled out of an item's text: who it belongs to, when it is due, and
    /// the message that is left once both were cut out.
    /// </summary>
    internal readonly struct TodoMetadata
    {
        /// <summary>The message without the metadata that was recognized in it.</summary>
        internal string Message { get; }

        /// <summary>The responsible person, or an empty string when none was named.</summary>
        internal string Owner { get; }

        /// <summary>The date exactly as it was written, kept for display when it cannot be parsed.</summary>
        internal string RawDate { get; }

        /// <summary>The parsed date, or null when there was none or it did not match any format.</summary>
        internal DateTime? Date { get; }

        /// <summary>Creates the result of reading one item's metadata.</summary>
        /// <param name="message">The message without the metadata.</param>
        /// <param name="owner">The responsible person.</param>
        /// <param name="rawDate">The date as it was written.</param>
        /// <param name="date">The parsed date.</param>
        internal TodoMetadata(string message, string owner, string rawDate, DateTime? date)
        {
            Message = message;
            Owner = owner;
            RawDate = rawDate;
            Date = date;
        }
    }
}