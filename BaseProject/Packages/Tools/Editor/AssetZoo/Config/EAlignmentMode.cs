namespace Base.ToolPackage.Editor.AssetZoo.Config
{
    /// <summary>
    /// How to align prefabs when placing them in the zoo.
    /// This only affects the editor display, not the prefab assets themselves.
    /// </summary>
    public enum EAlignmentMode : byte
    {
        /// <summary>
        /// Place the prefab using its original pivot.
        /// </summary>
        Pivot = 0,

        /// <summary>
        /// Push the prefab up so its lowest point sits on the slot (y=0).
        /// </summary>
        Ground = 1,

        /// <summary>
        /// Center the prefab's bounding box on the slot.
        /// </summary>
        Center = 2
    }
}