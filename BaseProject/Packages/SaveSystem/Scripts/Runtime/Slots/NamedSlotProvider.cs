using System;
using System.Collections.Generic;
using System.Threading;
using Base.SaveSystemPackage.Core;
using UnityEngine;

namespace Base.SaveSystemPackage.Slots
{
    /// <summary>
    /// Unlimited named slots. With no selection a save mints a fresh id; with a selection it
    /// overwrites that slot.
    /// </summary>
    public sealed class NamedSlotProvider : ISaveSlotProvider
    {
        private const string GuidFormat = "N";

        private readonly ISaveReader _reader;

        /// <inheritdoc/>
        public ESlotModel Model => ESlotModel.Named;

        /// <inheritdoc/>
        public bool SupportsNewSlots => true;

        /// <param name="reader">Used to list the saves that currently exist.</param>
        /// <exception cref="ArgumentNullException">When the reader is null.</exception>
        public NamedSlotProvider(ISaveReader reader)
            => _reader = reader ?? throw new ArgumentNullException(nameof(reader));

        /// <inheritdoc/>
        public async Awaitable<IReadOnlyList<SlotInfo>> ListSlotsAsync(CancellationToken ct = default)
            => await SlotListing.ListNewestFirstAsync(_reader, ct);

        /// <inheritdoc/>
        public bool TryResolveSaveTarget(string selectedSlotId, out string slotId)
        {
            slotId = string.IsNullOrEmpty(selectedSlotId)
                ? CreateNewSlotId()
                : selectedSlotId;

            return true;
        }

        /// <inheritdoc/>
        public Awaitable EnforcePolicyAsync(string savedSlotId, CancellationToken ct = default)
            => AwaitableUtility.Completed();

        private static string CreateNewSlotId() => Guid.NewGuid().ToString(GuidFormat);
    }
}