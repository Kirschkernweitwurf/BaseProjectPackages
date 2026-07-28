using Base.SaveSystemPackage.Model;

namespace Base.SaveSystemPackage.Slots
{
    /// <summary>
    /// One row in a save or load menu: a slot id, whether it currently holds a save, and its metadata
    /// if so. Immutable.
    /// </summary>
    public readonly struct SlotInfo
    {
        /// <summary>The slot id this row stands for.</summary>
        public string Id { get; }

        /// <summary>Whether the slot currently holds a completed save.</summary>
        public bool Exists { get; }

        /// <summary>Metadata if the slot holds a save, otherwise <c>null</c>.</summary>
        public SaveMetadata Metadata { get; }

        /// <param name="id">The slot id.</param>
        /// <param name="metadata">The slot's metadata, or <c>null</c> when the slot is empty.</param>
        public SlotInfo(string id, SaveMetadata metadata)
        {
            Id = id;
            Metadata = metadata;
            Exists = metadata != null;
        }
    }
}