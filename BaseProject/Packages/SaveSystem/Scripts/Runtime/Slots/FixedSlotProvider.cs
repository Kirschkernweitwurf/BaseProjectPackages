using System;
using System.Collections.Generic;
using System.Threading;
using Base.SaveSystemPackage.Core;
using UnityEngine;

namespace Base.SaveSystemPackage.Slots
{
    /// <summary>
    /// A fixed number of numbered slots. A save overwrites a selected slot in place and new slots
    /// cannot be minted. Empty slots still appear so a menu can show them.
    /// </summary>
    public sealed class FixedSlotProvider : ISaveSlotProvider
    {
        private const int MinSlotCount = 1;
        private const string SlotIdPrefix = "slot_";

        private readonly ISaveReader _reader;
        private readonly HashSet<string> _ids;
        private readonly IReadOnlyList<string> _orderedIds;

        /// <inheritdoc/>
        public ESlotModel Model => ESlotModel.Fixed;

        /// <inheritdoc/>
        public bool SupportsNewSlots => false;

        /// <param name="reader">Used to read the metadata of each numbered slot.</param>
        /// <param name="count">How many slots exist. At least one.</param>
        /// <exception cref="ArgumentNullException">When the reader is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">When the count is below one.</exception>
        public FixedSlotProvider(ISaveReader reader, int count)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));

            if (count < MinSlotCount)
                throw new ArgumentOutOfRangeException(nameof(count), "A fixed slot model needs at least one slot.");

            string[] ids = new string[count];
            for (int i = 0; i < count; i++)
                ids[i] = SlotId(i);

            _orderedIds = ids;
            _ids = new HashSet<string>(ids);
        }

        /// <summary>
        /// The id of the numbered slot at the given index, so a menu or a button can target one
        /// without knowing how ids are spelled.
        /// </summary>
        /// <param name="index">Zero-based slot index.</param>
        /// <returns>The slot id.</returns>
        public static string SlotId(int index) => $"{SlotIdPrefix}{index}";

        /// <inheritdoc/>
        public async Awaitable<IReadOnlyList<SlotInfo>> ListSlotsAsync(CancellationToken ct = default)
        {
            List<SlotInfo> slots = new(_orderedIds.Count);
            foreach (string id in _orderedIds)
                slots.Add(new SlotInfo(id, await _reader.LoadMetadataAsync(id, ct)));

            return slots;
        }

        /// <inheritdoc/>
        public bool TryResolveSaveTarget(string selectedSlotId, out string slotId)
        {
            slotId = selectedSlotId;
            return !string.IsNullOrEmpty(selectedSlotId) && _ids.Contains(selectedSlotId);
        }

        /// <inheritdoc/>
        public Awaitable EnforcePolicyAsync(string savedSlotId, CancellationToken ct = default)
            => AwaitableUtility.Completed();
    }
}