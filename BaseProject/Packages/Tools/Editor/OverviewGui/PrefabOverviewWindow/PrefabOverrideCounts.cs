namespace Base.ToolPackage.Editor.OverviewGui.PrefabOverviewWindow
{
    /// <summary>
    /// Amount of overrides a prefab variant carries on top of its base prefab.
    /// </summary>
    internal readonly struct PrefabOverrideCounts
    {
        /// <summary>Number of modified serialized properties, without the name of the variant root.</summary>
        public int ModifiedProperties { get; }

        /// <summary>Number of components added on top of the base.</summary>
        public int AddedComponents { get; }

        /// <summary>Number of components removed from the base.</summary>
        public int RemovedComponents { get; }

        /// <summary>Number of GameObjects added on top of the base.</summary>
        public int AddedGameObjects { get; }

        /// <summary>Sum of all override kinds.</summary>
        public int Total => ModifiedProperties + AddedComponents + RemovedComponents + AddedGameObjects;

        /// <summary>True when the variant changes nothing about its base.</summary>
        public bool IsEmpty => Total == 0;

        /// <summary>Creates a set of counts.</summary>
        /// <param name="modifiedProperties">Number of modified serialized properties.</param>
        /// <param name="addedComponents">Number of added components.</param>
        /// <param name="removedComponents">Number of removed components.</param>
        /// <param name="addedGameObjects">Number of added GameObjects.</param>
        public PrefabOverrideCounts(int modifiedProperties,
            int addedComponents,
            int removedComponents,
            int addedGameObjects)
        {
            ModifiedProperties = modifiedProperties;
            AddedComponents = addedComponents;
            RemovedComponents = removedComponents;
            AddedGameObjects = addedGameObjects;
        }
    }
}