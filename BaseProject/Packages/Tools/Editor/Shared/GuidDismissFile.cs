using System;
using System.Collections.Generic;

namespace Base.ToolsPackage.Editor.Shared
{
    /// <summary>
    /// The on disk shape of a <see cref="GuidDismissStore"/> file.
    /// </summary>
    /// <remarks>
    /// A public field rather than a property, because JsonUtility maps public fields only. The name
    /// is the JSON key and matches the files the three previous stores already wrote, so an existing
    /// dismissal list keeps loading after the switch.
    /// </remarks>
    [Serializable]
    internal sealed class GuidDismissFile
    {
        /// <summary>The dismissed GUIDs.</summary>
        public List<string> guids = new();
    }
}