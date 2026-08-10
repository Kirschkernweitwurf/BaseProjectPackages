using System;
using System.Collections.Generic;

namespace Base.ToolPackage.Editor.CodebaseGraph.Analysis
{
    /// <summary>Serialization shape of the baseline file. One list, so the format stays obvious.</summary>
    [Serializable]
    internal sealed class FindingBaselineData
    {
        /// <summary>Ids of every finding the previous scan raised.</summary>
        public List<string> Ids = new();
    }
}