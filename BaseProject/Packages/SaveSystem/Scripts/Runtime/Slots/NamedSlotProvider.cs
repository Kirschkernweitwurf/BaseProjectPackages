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
    /// Unlimited named slots. With no selection a save mints a fresh id; with a selection it
    /// overwrites that slot.
    /// </summary>
    public sealed class NamedSlotProvider : ISaveSlotProvider
    {
        private readonly ISaveReader _reader;

        public ESlotModel Model => ESlotModel.Named;
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

        public bool TryResolveSaveTarget(string selectedSlotId, out string slotId)
        {
            slotId = string.IsNullOrEmpty(selectedSlotId) ? CreateNewSlotId() : selectedSlotId;
            return true;
        }

        public async Awaitable EnforcePolicyAsync(string savedSlotId, CancellationToken ct = default) =>
            await Task.CompletedTask;

        private static string CreateNewSlotId() => Guid.NewGuid().ToString("N");
    }
}