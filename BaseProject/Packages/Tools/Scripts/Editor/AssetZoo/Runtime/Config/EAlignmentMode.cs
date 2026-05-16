namespace Base.ToolPackage.Editor.AssetZoo.Runtime.Config
{
    /// <summary>
    /// How to align prefabs when placing them in the zoo.
    /// This only affects the editor display, not the prefab assets themselves.
    /// </summary>
    public enum EAlignmentMode
    {
        /// <summary>
        /// Place the prefab using its original pivot.
        /// </summary>
        Pivot,

        /// <summary>
        /// Push the prefab up so its lowest point sits on the slot (y=0).
        /// </summary>
        Ground,

        /// <summary>
        /// Center the prefab's bounding box on the slot.
        /// </summary>
        Center
    }
}