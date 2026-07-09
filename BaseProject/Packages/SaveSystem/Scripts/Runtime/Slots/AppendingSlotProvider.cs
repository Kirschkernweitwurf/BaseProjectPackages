using System;
using System.Collections.Generic;
using System.Threading;
using Base.SaveSystemPackage.Model;
using Base.SaveSystemPackage.System;
using UnityEngine;

namespace Base.SaveSystemPackage.Slots
{
    /// <summary>
    /// Every save creates a new entry. Ids are time-ordered so listing newest-first is trivial.
    /// Optionally caps the number of saves and prunes the oldest beyond the cap.
    /// </summary>
    public sealed class AppendingSlotProvider : ISaveSlotProvider
    {
        public ESlotModel Model => ESlotModel.Appending;

        public bool SupportsNewSlots => true;

        private readonly ISaveReader _reader;
        private readonly ISaveWriter _writer;
        private readonly int _maxSaves;

        public AppendingSlotProvider(ISaveReader reader, ISaveWriter writer, int maxSaves = 0)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _maxSaves = Mathf.Max(0, maxSaves);
        }

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
            slotId = CreateNewSlotId();
            return true;
        }

        public async Awaitable EnforcePolicyAsync(string savedSlotId, CancellationToken ct = default)
        {
            if (_maxSaves <= 0)
                return;

            IReadOnlyList<SlotInfo> slots = await ListSlotsAsync(ct);
            for (int i = _maxSaves; i < slots.Count; i++)
                await _writer.DeleteAsync(slots[i].Id, ct);
        }

        private static string CreateNewSlotId() => $"save_{DateTime.UtcNow.Ticks:D19}_{Guid.NewGuid():N}"[..33];
    }
}