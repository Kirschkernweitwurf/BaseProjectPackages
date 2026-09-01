using Base.EditorUiPackage;
using Base.ToolPackage.Editor.TodoOverview.Model;

namespace Base.ToolPackage.Editor.TodoOverview.Settings
{
    /// <summary>
    /// What a fresh <see cref="TodoSettings"/> starts out with. These are only the starting values;
    /// every one of them can be edited per project afterwards.
    /// </summary>
    internal static class TodoDefaults
    {
        /// <summary>How far an item reaches past its own line before anyone changes it.</summary>
        internal const ETodoContinuation Continuation = ETodoContinuation.Indented;

        /// <summary>
        /// Which notation dates are shown in before anyone changes it. The project's own, so a list
        /// reads the same for everyone working on it rather than following each machine's region.
        /// </summary>
        internal const ETodoDateDisplay DateDisplay = ETodoDateDisplay.Project;

        /// <summary>
        /// Formats a date in an item is read with, tried in this order before the invariant culture
        /// gets a last try. Two digit years resolve into the current century.
        /// </summary>
        /// <returns>The default date formats.</returns>
        internal static string[] CreateDateFormats() => new[]
        {
            "dd.MM.yy",
            "d.M.yy",
            "dd.MM.yyyy",
            "d.M.yyyy",
            "yyyy-MM-dd",
            "yyyy/MM/dd",
            "dd-MM-yyyy",
            "MM/dd/yyyy"
        };

        /// <summary>The file types that are read. Anything else in the project is skipped.</summary>
        /// <returns>The default file extensions, lower case and with their dot.</returns>
        internal static string[] CreateExtensions() => new[]
        {
            ".cs",
            ".shader",
            ".cginc",
            ".hlsl",
            ".compute",
            ".uss"
        };

        /// <summary>
        /// Patterns that pull the responsible person and the date out of an item's text. Each one is a
        /// regular expression that may carry an <c>owner</c> group, a <c>date</c> group or both;
        /// whatever a pattern matches is cut out of the message. They are tried in order and a later
        /// pattern only fills in what an earlier one left empty, so the most specific ones come first.
        /// </summary>
        /// <returns>The default metadata patterns.</returns>
        internal static string[] CreateMetadataPatterns() => new[]
        {
            @"\(\s*(?<owner>[^,()]{1,32}?)\s*,\s*(?<date>[0-9][0-9.\-/]{4,9})\s*\)",
            @"\[\s*(?<owner>[^\[\],]{1,32}?)\s*,\s*(?<date>[0-9][0-9.\-/]{4,9})\s*\]",
            @"\(\s*(?<date>[0-9][0-9.\-/]{4,9})\s*\)",
            @"^\(\s*(?<owner>[A-Za-z][\w.\-]{0,31})\s*\)",
            @"@(?<owner>[A-Za-z][\w.\-]{0,31})",
            @"(?<date>\d{4}-\d{1,2}-\d{1,2})",
            @"(?<date>\d{1,2}\.\d{1,2}\.\d{2,4})"
        };

        /// <summary>The keywords that are looked for, with the color each one is drawn in.</summary>
        /// <remarks>
        /// Taken from <see cref="EditorSwatches"/> rather than written out here, so a fresh project
        /// starts on colors tuned for the editor theme it was seeded under, and so the swatch offered
        /// in the settings page is the same color the keyword already has.
        /// </remarks>
        /// <returns>The default keyword tags.</returns>
        internal static TodoTag[] CreateTags() => new[]
        {
            new TodoTag("TODO", EditorSwatches.Blue, true),
            new TodoTag("FIXME", EditorSwatches.Orange, true),
            new TodoTag("BUG", EditorSwatches.Red, true),
            new TodoTag("HACK", EditorSwatches.Amber, true),
            new TodoTag("OPTIMIZE", EditorSwatches.Violet, true),
            new TodoTag("REVIEW", EditorSwatches.Teal, true),
            new TodoTag("NOTE", EditorSwatches.Green, false)
        };
    }
}