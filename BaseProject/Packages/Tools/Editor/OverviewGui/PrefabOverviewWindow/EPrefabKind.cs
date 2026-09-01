namespace Base.ToolPackage.Editor.OverviewGui.PrefabOverviewWindow
{
    /// <summary>
    /// What kind of prefab asset an overview entry points at.
    /// </summary>
    internal enum EPrefabKind : byte
    {
        /// <summary>A normal prefab that is not derived from another prefab.</summary>
        Regular = 0,

        /// <summary>A prefab variant derived from a base prefab.</summary>
        Variant = 1,

        /// <summary>An imported model prefab.</summary>
        Model = 2,

        /// <summary>The asset could not be classified, for example because its base prefab is gone.</summary>
        Broken = 3
    }
}