using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Base.SaveSystemPackage.Slots
{
    /// <summary>
    /// Owns slot bookkeeping for one slot model: which slots exist, where the next save is written,
    /// and any post-save policy such as pruning. Callers express intent (a selected slot, or none)
    /// and the provider resolves it into a concrete id, so switching the model changes save
    /// behaviour without touching the caller.
    /// </summary>
    public interface ISaveSlotProvider
    {
        /// <summary>The model this provider implements.</summary>
        ESlotModel Model { get; }

        /// <summary>Whether this model can create slots beyond a fixed set.</summary>
        bool SupportsNewSlots { get; }

        /// <summary>The slots to show, in display order.</summary>
        Awaitable<IReadOnlyList<SlotInfo>> ListSlotsAsync(CancellationToken ct = default);

        /// <summary>
        /// Resolves the id the next save should write to. <paramref name="selectedSlotId"/> is the
        /// slot the player currently has selected, or <c>null</c> for "no selection / new save".
        /// Returns <c>false</c> when the model cannot satisfy the request.
        /// </summary>
        bool TryResolveSaveTarget(string selectedSlotId, out string slotId);

        /// <summary>Runs any post-save policy for this model, e.g. deleting the oldest saves.</summary>
        Awaitable EnforcePolicyAsync(string savedSlotId, CancellationToken ct = default);
    }
}