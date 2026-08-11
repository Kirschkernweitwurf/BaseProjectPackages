using System.Collections.Generic;
using Base.ToolPackage.Editor.NamingConventions.Data;

namespace Base.ToolPackage.Editor.NamingConventions.Window
{
    /// <summary>
    /// One collapsible group of scan results, for example a folder or a rule. A group with an
    /// empty key is drawn without a header, which is what the ungrouped list uses.
    /// </summary>
    internal sealed class AssetNamingGroup
    {
        /// <summary>Header of the group. Empty when the list is not grouped.</summary>
        public string Key { get; }

        /// <summary>Violations inside this group.</summary>
        public List<AssetNamingViolation> Violations { get; } = new();

        /// <summary>Creates an empty group.</summary>
        public AssetNamingGroup(string key) => Key = key;
    }
}