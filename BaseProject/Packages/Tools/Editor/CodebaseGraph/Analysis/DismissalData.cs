using System;
using System.Collections.Generic;

namespace Base.ToolsPackage.Editor.CodebaseGraph.Analysis
{
    /// <summary>Serialization shape of the dismissal file. Two lists, so the format stays obvious.</summary>
    [Serializable]
    internal sealed class DismissalData
    {
        /// <summary>
        /// Format the file was written with. It exists for one reason: an id written before ids could
        /// name a finding is byte identical to a deliberate entry wide one, so without this the older
        /// file comes back from a rollback or a fresh clone silently broadened.
        /// </summary>
        public int version;

        /// <summary>Ids whose own findings are hidden.</summary>
        public List<string> own = new();

        /// <summary>Ids whose own findings and everything inside them are hidden.</summary>
        public List<string> tree = new();
    }
}