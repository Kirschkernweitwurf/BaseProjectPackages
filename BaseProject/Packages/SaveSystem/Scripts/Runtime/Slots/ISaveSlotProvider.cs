using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Base.SaveSystemPackage.Slots
{
    /// <summary>
    /// Owns slot bookkeeping: what slots exist, how a new id is minted, and what happens after a
    /// save (e.g. pruning). This is the single seam that makes the same save core behave like
    /// fixed slots, append or unlimited named slots. Writing to a known id is
    /// just <c>ISaveWriter.SaveAsync</c>; the provider is only needed to enumerate, allocate and
    /// enforce policy.
    /// </summary>
    public interface ISaveSlotProvider
    {
        /// <summary>The slots to show, in display order.</summary>
        Awaitable<IReadOnlyList<SlotInfo>> ListSlotsAsync(CancellationToken ct = default);

        /// <summary>True if this model can mint brand-new slots (append, named) vs a fixed set.</summary>
        bool SupportsNewSlots { get; }

        /// <summary>Mint an id for a brand-new save. Throws if <see cref="SupportsNewSlots"/> is false.</summary>
        string CreateNewSlotId();

        /// <summary>Run any post-save policy for this model, e.g. delete the oldest auto-saves.</summary>
        Awaitable EnforcePolicyAsync(string savedSlotId, CancellationToken ct = default);
    }
}
