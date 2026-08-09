namespace Base.ToolPackage.Editor.OverviewGui.PrefabOverviewWindow
{
    /// <summary>
    /// Which prefabs the overview window keeps visible.
    /// </summary>
    public enum EPrefabViewFilter : byte
    {
        /// <summary>Every scanned prefab.</summary>
        All = 0,

        /// <summary>Only prefab variants, with their bases kept as context.</summary>
        Variants = 1,

        /// <summary>Only prefabs that were flagged with an issue, with their bases kept as context.</summary>
        Issues = 2
    }
}