using System;
using System.Collections.Generic;

namespace Base.ToolPackage.Editor.CodebaseGraph.Analysis
{
    /// <summary>Serialization shape of the dismissal file. Two lists, so the format stays obvious.</summary>
    [Serializable]
    public sealed class DismissalData
    {
        /// <summary>Ids whose own findings are hidden.</summary>
        public List<string> Own = new();

        /// <summary>Ids whose own findings and everything inside them are hidden.</summary>
        public List<string> Tree = new();
    }
}
