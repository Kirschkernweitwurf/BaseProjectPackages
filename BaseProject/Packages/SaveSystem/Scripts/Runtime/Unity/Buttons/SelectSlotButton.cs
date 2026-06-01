using System.Threading;
using Base.SaveSystemPackage.Slots;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.SaveSystemPackage.Unity.Buttons
{
    /// <summary>
    /// Sets the active <see cref="SaveSlotSelection"/> so the save, load and delete buttons act on
    /// this slot. A menu assigns the runtime id via <see cref="SetSlotId"/> when building a row;
    /// for a fixed-slot layout the slot index can be authored directly instead.
    /// </summary>
    public sealed class SelectSlotButton : SaveSlotButtonBase
    {
        [Tooltip("Fixed-slot index to select. Ignored once a runtime id is set via SetSlotId.")]
        [SerializeField] private int fixedSlotIndex = -1;

        private string _slotId;

        /// <summary>Binds this button to a runtime slot id, e.g. when a menu populates a row.</summary>
        public void SetSlotId(string slotId) => _slotId = slotId;

        protected override Awaitable OnClickAsync(CancellationToken ct)
        {
            string slotId = ResolveSlotId();
            if (string.IsNullOrEmpty(slotId))
            {
                CustomLogger.LogWarning("Select button has no slot id to select.", this);
                return Awaitable.EndOfFrameAsync(ct);
            }

            Selection.Select(slotId);
            return Awaitable.EndOfFrameAsync(ct);
        }

        private string ResolveSlotId()
        {
            if (!string.IsNullOrEmpty(_slotId))
                return _slotId;

            return Slots.Model == ESlotModel.Fixed && fixedSlotIndex >= 0
                ? FixedSlotProvider.SlotId(fixedSlotIndex)
                : null;
        }
    }
}