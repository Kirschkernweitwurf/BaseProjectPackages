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
        internal readonly List<FieldHit> Fields = new();
        internal readonly Dictionary<string, string> StaticMethods = new();
        internal readonly List<string> ResetBodies = new();

        internal string Cleaned;
        internal int[] LineStarts;
        internal ScanOptions Opt;
    }
}