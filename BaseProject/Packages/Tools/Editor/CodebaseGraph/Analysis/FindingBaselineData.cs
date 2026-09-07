using System;
using System.Collections.Generic;

namespace Base.ToolsPackage.Editor.CodebaseGraph.Analysis
{
    /// <summary>Serialization shape of the baseline file. One list, so the format stays obvious.</summary>
    [Serializable]
    internal sealed class FindingBaselineData
    {
        // JsonUtility writes a field's own name as the key, so this one is the file format. Renaming
        // it to match the serialized field convention would silently stop every baseline written
        // before the rename from loading.
        /// <summary>Ids of every finding the previous scan raised.</summary>
        public List<string> Ids = new();
    }
}