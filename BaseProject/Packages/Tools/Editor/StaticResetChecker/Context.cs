using System.Collections.Generic;

namespace Base.ToolPackage.Editor.StaticResetChecker
{
    /// <summary>
    /// Context object to hold data during the static reset check process.
    /// <br/><br/>
    /// This includes the list of fields that are hit, the static methods found
    /// and the bodies of any reset methods encountered.
    /// <br/><br/>
    /// It also holds the cleaned code and line start indices for reference during analysis,
    /// as well as the options used for scanning.
    /// </summary>
    internal class Context
    {
        /// <summary>Every static field found in the file, before any of them is cleared.</summary>
        internal readonly List<FieldHit> Fields = new();

        /// <summary>
        /// The static methods declared in the file, keyed by name. Used to follow a reset method into
        /// the helpers it calls, so clearing done one level down still counts.
        /// </summary>
        internal readonly Dictionary<string, string> StaticMethods = new();

        /// <summary>
        /// The bodies of every method that resets statics. A field named in one of these is considered
        /// handled and drops out of the findings.
        /// </summary>
        internal readonly List<string> ResetBodies = new();

        /// <summary>
        /// The file with comments and string literals blanked out, so a field name inside a comment
        /// never reads as a reset.
        /// </summary>
        internal string Cleaned;

        /// <summary>
        /// Character offset of each line start, which is how a match position becomes a line number
        /// without counting newlines again for every hit.
        /// </summary>
        internal int[] LineStarts;

        /// <summary>The options this scan runs under.</summary>
        internal ScanOptions Opt;
    }
}