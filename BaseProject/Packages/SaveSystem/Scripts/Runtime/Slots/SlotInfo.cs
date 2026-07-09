using Base.SaveSystemPackage.Model;

namespace Base.SaveSystemPackage.Slots
{
    /// <summary>
    /// One row in a save/load menu: a slot id, whether it currently holds a save, and its
    /// metadata if so. Immutable.
    /// </summary>
    public readonly struct SlotInfo
    {
        public string Id { get; }

        public bool Exists { get; }

        /// <summary>Metadata if the slot holds a save, otherwise <c>null</c>.</summary>
        public SaveMetadata Metadata { get; }

        public SlotInfo(string id, SaveMetadata metadata)
        {
            Id = id;
            Metadata = metadata;
            Exists = metadata != null;
        }
    }
}