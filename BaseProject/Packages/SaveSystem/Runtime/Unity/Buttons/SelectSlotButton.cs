using System.Threading;
using Base.SaveSystemPackage.Core;
using Base.SaveSystemPackage.Slots;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.SaveSystemPackage.Unity.Buttons
{
    /// <summary>
    /// Sets the active <see cref="SaveSlotSelection"/> so the save, load and delete buttons act on this
    /// slot. A menu assigns the runtime id via <see cref="SetSlotId"/> when building a row; for a fixed
    /// slot layout the slot index can be authored directly instead.
    /// </summary>
    public sealed class SelectSlotButton : SaveSlotButtonBase
    {
        private const int UseRuntimeId = -1;

        [Tooltip("Fixed-slot index to select. Ignored once a runtime id is set via "
            + nameof(SetSlotId)
            + ".")]
        [SerializeField] private int fixedSlotIndex = UseRuntimeId;

        private string _slotId;

        /// <summary>Binds this button to a runtime slot id, for example when a menu populates a row.</summary>
        /// <param name="slotId">The slot this button should select.</param>
        public void SetSlotId(string slotId) => _slotId = slotId;

        protected override Awaitable OnClickAsync(CancellationToken ct)
        {
            string slotId = ResolveSlotId();
            if (string.IsNullOrEmpty(slotId))
            {
                CustomLogger.LogWarning("Select button has no slot id to select.", this);
                return AwaitableUtility.Completed();
            }

            Selection.Select(slotId);
            return AwaitableUtility.Completed();
        }

        private string ResolveSlotId()
        {
            if (!string.IsNullOrEmpty(_slotId))
                return _slotId;

            return Slots.Model == ESlotModel.Fixed && fixedSlotIndex > UseRuntimeId
                ? FixedSlotProvider.SlotId(fixedSlotIndex)
                : null;
        }
    }
}