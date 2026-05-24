using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Base.SaveSystemPackage.Model;
using Base.SaveSystemPackage.System;
using UnityEngine;

namespace Base.SaveSystemPackage.Slots
{
    /// <summary>
    /// Unlimited named slots. "Save as new" mints a fresh id; saving to an existing id overwrites it.
    /// </summary>
    public sealed class NamedSlotProvider : ISaveSlotProvider
    {
        private readonly ISaveReader _reader;

        public bool SupportsNewSlots => true;

        public NamedSlotProvider(ISaveReader reader) =>
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));

        public async Awaitable<IReadOnlyList<SlotInfo>> ListSlotsAsync(CancellationToken ct = default)
        {
            IReadOnlyList<SaveMetadata> all = await _reader.ListSavesAsync(ct);
            List<SaveMetadata> sorted = new(all);
            sorted.Sort((a, b) => b.LastSavedUtc.CompareTo(a.LastSavedUtc));

            List<SlotInfo> slots = new(sorted.Count);
            foreach (SaveMetadata m in sorted)
                slots.Add(new SlotInfo(m.SlotId, m));

            return slots;
        }

        public string CreateNewSlotId() => Guid.NewGuid().ToString("N");

        public async Awaitable EnforcePolicyAsync(string savedSlotId, CancellationToken ct = default) =>
            await Task.CompletedTask;
    }
}