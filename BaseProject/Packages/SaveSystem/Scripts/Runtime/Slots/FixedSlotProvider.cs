using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Base.SaveSystemPackage.System;
using UnityEngine;

namespace Base.SaveSystemPackage.Slots
{
    /// <summary>
    /// A fixed number of numbered slots (slot_0 … slot_{count-1}). A save overwrites a selected
    /// slot in place; new slots cannot be minted. Empty slots still appear so a menu can show them.
    /// </summary>
    public sealed class FixedSlotProvider : ISaveSlotProvider
    {
        public ESlotModel Model => ESlotModel.Fixed;

        public bool SupportsNewSlots => false;

        private readonly ISaveReader _reader;
        private readonly HashSet<string> _ids;
        private readonly IReadOnlyList<string> _orderedIds;

        public FixedSlotProvider(ISaveReader reader, int count)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            if (count < 1)
                throw new ArgumentOutOfRangeException(nameof(count), "A fixed slot model needs at least one slot.");

            string[] ids = new string[count];
            for (int i = 0; i < count; i++)
                ids[i] = SlotId(i);

            _orderedIds = ids;
            _ids = new HashSet<string>(ids);
        }

        public async Awaitable<IReadOnlyList<SlotInfo>> ListSlotsAsync(CancellationToken ct = default)
        {
            List<SlotInfo> slots = new(_orderedIds.Count);
            foreach (string id in _orderedIds)
                slots.Add(new SlotInfo(id, await _reader.LoadMetadataAsync(id, ct)));

            return slots;
        }

        public bool TryResolveSaveTarget(string selectedSlotId, out string slotId)
        {
            slotId = selectedSlotId;
            return !string.IsNullOrEmpty(selectedSlotId) && _ids.Contains(selectedSlotId);
        }

        public async Awaitable EnforcePolicyAsync(string savedSlotId, CancellationToken ct = default)
            => await Task.CompletedTask;

        public static string SlotId(int index) => $"slot_{index}";
    }
}