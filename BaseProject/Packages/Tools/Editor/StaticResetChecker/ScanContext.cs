using System.Collections.Generic;

namespace Base.ToolsPackage.Editor.StaticResetChecker
{
    /// <summary>
    /// Everything one file's scan accumulates: the statics found in it, the static methods it
    /// declares, the bodies of the methods that reset statics, and the cleaned text those were all
    /// read out of.
    /// </summary>
    /// <remarks>
    /// One instance per file. It is filled in one pass and read in the next, which is why the fields
    /// are writable rather than set through a constructor.
    /// </remarks>
    internal sealed class ScanContext
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
        internal ScanOptions Options;
    }
}