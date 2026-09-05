using System;
using System.Collections.Generic;
using Base.ToolsPackage.Editor.TodoOverview.Model;
using Base.ToolsPackage.Editor.TodoOverview.Settings;

namespace Base.ToolsPackage.Editor.TodoOverview.Scanning
{
    /// <summary>
    /// Everything <see cref="TodoPatterns"/> needs, as plain values. The settings live in a
    /// <c>ScriptableSingleton</c> backed by a file in <c>ProjectSettings</c>, and reading one to
    /// compile a pattern tied the compiler to the whole project's state.
    /// </summary>
    /// <remarks>
    /// This is also what makes the compiler reachable at all: anything wanting to compile a set of
    /// keywords without being the project settings, a test above all, hands over the four values it
    /// actually reads instead of a singleton it would have to write to first.
    /// </remarks>
    internal readonly struct TodoPatternInput
    {
        /// <summary>The keywords to look for, in the order they are listed in.</summary>
        internal IReadOnlyList<TodoTag> Tags { get; }

        /// <summary>The patterns that read the responsible person and the date out of an item.</summary>
        internal IReadOnlyList<string> MetadataPatterns { get; }

        /// <summary>The formats a date in an item is read with, in the order they are tried.</summary>
        internal IReadOnlyList<string> DateFormats { get; }

        /// <summary>How far an item reaches past the line its keyword sits on.</summary>
        internal ETodoContinuation Continuation { get; }

        /// <summary>Whether a keyword only counts in the exact casing it is configured with.</summary>
        internal bool CaseSensitive { get; }

        /// <summary>Creates the input a set of patterns is compiled from.</summary>
        /// <param name="tags">The keywords to look for.</param>
        /// <param name="metadataPatterns">The patterns that read owner and date.</param>
        /// <param name="dateFormats">The formats a date is read with.</param>
        /// <param name="continuation">How far an item reaches past its own line.</param>
        /// <param name="caseSensitive">Whether casing has to match.</param>
        /// <exception cref="ArgumentNullException">When any of the three lists is null.</exception>
        internal TodoPatternInput(IReadOnlyList<TodoTag> tags, IReadOnlyList<string> metadataPatterns,
            IReadOnlyList<string> dateFormats, ETodoContinuation continuation, bool caseSensitive)
        {
            Tags = tags ?? throw new ArgumentNullException(nameof(tags));
            MetadataPatterns = metadataPatterns ?? throw new ArgumentNullException(nameof(metadataPatterns));
            DateFormats = dateFormats ?? throw new ArgumentNullException(nameof(dateFormats));
            Continuation = continuation;
            CaseSensitive = caseSensitive;
        }

        /// <summary>Reads the values a scan needs out of the project settings.</summary>
        /// <param name="settings">The settings to read.</param>
        /// <returns>The input a set of patterns is compiled from.</returns>
        /// <exception cref="ArgumentNullException">When the settings are null.</exception>
        internal static TodoPatternInput From(TodoSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            return new TodoPatternInput(settings.Tags, settings.MetadataPatterns, settings.DateFormats,
                settings.Continuation, settings.CaseSensitive);
        }
    }
}