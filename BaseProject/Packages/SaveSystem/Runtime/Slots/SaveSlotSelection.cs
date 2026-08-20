using System;

namespace Base.SaveSystemPackage.Slots
{
    /// <summary>
    /// Runtime holder for the slot the player currently has selected. A menu sets it when a row is
    /// chosen; save, load and delete actions read it. Decouples slot identity from authored assets, so
    /// identity is established at runtime from the slots that actually exist.
    /// </summary>
    public sealed class SaveSlotSelection
    {
        /// <summary>Raised whenever the selection changes, including when it is cleared.</summary>
        public event Action<string> Changed;

        /// <summary>The selected slot id, or <c>null</c> when nothing is selected.</summary>
        public string SelectedSlotId { get; private set; }

        /// <summary>
        /// Selects a slot and raises <see cref="Changed"/>. Selecting the current slot does nothing.
        /// </summary>
        /// <param name="slotId">The slot to select, or <c>null</c> to clear the selection.</param>
        public void Select(string slotId)
        {
            if (SelectedSlotId == slotId)
                return;

            SelectedSlotId = slotId;
            Changed?.Invoke(slotId);
        }

        /// <summary>Clears the selection, so the next save goes to a new slot where the model allows it.</summary>
        public void Clear() => Select(null);
    }
}