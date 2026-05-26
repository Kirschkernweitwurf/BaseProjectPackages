using System;

namespace Base.SaveSystemPackage.Slots
{
    /// <summary>
    /// Runtime holder for the slot the player currently has selected. A menu sets it when a row is
    /// chosen; save, load and delete actions read it. Decouples slot identity from authored assets,
    /// so identity is established at runtime from the slots that actually exist.
    /// </summary>
    public sealed class SaveSlotSelection
    {
        /// <summary>The selected slot id, or <c>null</c> when nothing is selected.</summary>
        public string SelectedSlotId { get; private set; }

        /// <summary>Raised whenever the selection changes, including when cleared.</summary>
        public event Action<string> Changed;

        public void Select(string slotId)
        {
            if (SelectedSlotId == slotId)
                return;

            SelectedSlotId = slotId;
            Changed?.Invoke(slotId);
        }

        public void Clear() => Select(null);
    }
}