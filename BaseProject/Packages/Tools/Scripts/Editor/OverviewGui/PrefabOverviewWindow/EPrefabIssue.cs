using System;

namespace Base.ToolPackage.Editor.OverviewGui.PrefabOverviewWindow
{
    /// <summary>
    /// Problems a prefab entry can be flagged with after a scan.
    /// </summary>
    [Flags]
    public enum EPrefabIssue : byte
    {
        /// <summary>Nothing to report.</summary>
        None = 0,

        /// <summary>A variant that does not change anything about its base and can be replaced by it.</summary>
        RedundantVariant = 1,

        /// <summary>A variant that overrides so much that it barely shares anything with its base.</summary>
        HeavyOverrides = 2,

        /// <summary>A variant that sits far down a chain of variants, which makes changes hard to predict.</summary>
        DeepChain = 4,

        /// <summary>A variant whose base prefab could not be resolved.</summary>
        MissingBase = 8
    }
}