using System.Collections.Generic;
using System.Threading;
using Base.SaveSystemPackage.Core;
using Base.SaveSystemPackage.Model;
using UnityEngine;

namespace Base.SaveSystemPackage.Slots
{
    /// <summary>
    /// Shared listing logic for the slot models that simply show whatever saves happen to exist, so
    /// <see cref="AppendingSlotProvider"/> and <see cref="NamedSlotProvider"/> do not repeat it.
    /// </summary>
    internal static class SlotListing
    {
        /// <summary>
        /// Lists every existing save, most recently written first.
        /// </summary>
        /// <param name="reader">The reader to pull metadata from.</param>
        /// <param name="ct">Cancels the underlying storage reads.</param>
        /// <returns>One entry per existing save, newest first.</returns>
        public static async Awaitable<IReadOnlyList<SlotInfo>> ListNewestFirstAsync(ISaveReader reader,
            CancellationToken ct)
        {
            IReadOnlyList<SaveMetadata> saves = await reader.ListSavesAsync(ct);

            List<SaveMetadata> sorted = new(saves);
            sorted.Sort(comparison: (first, second) => second.LastSavedUtc.CompareTo(first.LastSavedUtc));

            List<SlotInfo> slots = new(sorted.Count);
            foreach (SaveMetadata metadata in sorted)
                slots.Add(new SlotInfo(metadata.SlotId, metadata));

            return slots;
        }
    }
}