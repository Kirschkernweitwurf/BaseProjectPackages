using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Base.SaveSystemPackage.System;
using UnityEngine;

namespace Base.SaveSystemPackage.Slots
{
    /// <summary>
    /// A fixed number of numbered slots (slot_0 … slot_{count-1}). The player overwrites a chosen
    /// slot in place; new slots cannot be minted. Empty slots still appear in the list so a menu
    /// can show "Empty".
    /// </summary>
    public sealed class FixedSlotProvider : ISaveSlotProvider
    {
        private readonly ISaveReader _reader;
        private readonly IReadOnlyList<string> _ids;

        public bool SupportsNewSlots => false;

        public FixedSlotProvider(ISaveReader reader, int count)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            if (count < 1)
                throw new ArgumentOutOfRangeException(nameof(count), "A fixed slot model needs at least one slot.");

            string[] ids = new string[count];
            for (int i = 0; i < count; i++)
                ids[i] = SlotId(i);
            _ids = ids;
        }

        /// <summary>The id for slot index <paramref name="index"/>; use it as the SaveRequest slot id.</summary>
        public static string SlotId(int index) => $"slot_{index}";

        public async Awaitable<IReadOnlyList<SlotInfo>> ListSlotsAsync(CancellationToken ct = default)
        {
            List<SlotInfo> slots = new(_ids.Count);
            foreach (string id in _ids)
                slots.Add(new SlotInfo(id, await _reader.LoadMetadataAsync(id, ct)));

            return slots;
        }

        public string CreateNewSlotId() =>
            throw new InvalidOperationException("The fixed slot model has a set number of slots; pick one to overwrite.");

        public async Awaitable EnforcePolicyAsync(string savedSlotId, CancellationToken ct = default) =>
            await Task.CompletedTask;
    }
}