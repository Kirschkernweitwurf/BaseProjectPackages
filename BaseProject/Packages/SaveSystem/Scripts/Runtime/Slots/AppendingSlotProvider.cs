using System;
using System.Collections.Generic;
using System.Threading;
using Base.SaveSystemPackage.Core;
using UnityEngine;

namespace Base.SaveSystemPackage.Slots
{
    /// <summary>
    /// Every save creates a new entry. Ids lead with a timestamp so listing newest first is trivial.
    /// Optionally caps the number of saves and prunes the oldest beyond the cap.
    /// </summary>
    public sealed class AppendingSlotProvider : ISaveSlotProvider
    {
        private const string GuidFormat = "N";
        private const string SlotIdPrefix = "save_";
        private const int SlotIdSuffixLength = 8;
        private const string TicksFormat = "D19";
        private const int UnlimitedSaves = 0;

        /// <inheritdoc/>
        public ESlotModel Model => ESlotModel.Appending;

        /// <inheritdoc/>
        public bool SupportsNewSlots => true;

        private readonly ISaveReader _reader;
        private readonly ISaveWriter _writer;
        private readonly int _maxSaves;

        /// <param name="reader">Used to list the saves that currently exist.</param>
        /// <param name="writer">Used to prune saves beyond the cap.</param>
        /// <param name="maxSaves">How many saves to keep. Zero means unlimited.</param>
        /// <exception cref="ArgumentNullException">When the reader or the writer is null.</exception>
        public AppendingSlotProvider(ISaveReader reader, ISaveWriter writer, int maxSaves = UnlimitedSaves)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _maxSaves = Mathf.Max(UnlimitedSaves, maxSaves);
        }

        /// <inheritdoc/>
        public async Awaitable<IReadOnlyList<SlotInfo>> ListSlotsAsync(CancellationToken ct = default)
            => await SlotListing.ListNewestFirstAsync(_reader, ct);

        /// <inheritdoc/>
        public bool TryResolveSaveTarget(string selectedSlotId, out string slotId)
        {
            // This model never overwrites, so the selection is deliberately ignored.
            slotId = CreateNewSlotId();
            return true;
        }

        /// <inheritdoc/>
        public async Awaitable EnforcePolicyAsync(string savedSlotId, CancellationToken ct = default)
        {
            if (_maxSaves <= UnlimitedSaves)
                return;

            IReadOnlyList<SlotInfo> slots = await ListSlotsAsync(ct);
            for (int i = _maxSaves; i < slots.Count; i++)
                await _writer.DeleteAsync(slots[i].Id, ct);
        }

        private static string CreateNewSlotId()
        {
            string uniqueSuffix = Guid.NewGuid().ToString(GuidFormat)[..SlotIdSuffixLength];
            return $"{SlotIdPrefix}{DateTime.UtcNow.Ticks.ToString(TicksFormat)}_{uniqueSuffix}";
        }
    }
}