using System.Collections.Generic;
using System.Threading;
using Base.SaveSystemPackage.Savable;
using Base.SaveSystemPackage.SaveSlot;
using UnityEngine;

namespace Base.SaveSystemPackage
{
    /// <summary>
    /// High-level save API. The ONLY thing gameplay/UI code talks to.
    /// Swap the concrete implementation (e.g. for a console) without touching callers.
    ///
    /// Data is NOT passed in. Instead, every object that wants to be saved implements
    /// <see cref="ISavable"/> and registers with <see cref="SaveRegistry"/>. On save,
    /// the system collects their state; on load, it hands the state back to them.
    /// </summary>
    public interface ISaveSystem
    {
        /// <summary>
        /// Collect state from all registered savables and write it.
        /// </summary>
        Awaitable SaveAsync(string slotId, string displayName = null, Texture2D screenshot = null,
            double? totalPlaySeconds = null, CancellationToken ct = default);

        /// <summary>
        /// Read a save and hand the state back to all registered savables.
        /// Returns <c>false</c> if the slot does not exist.
        /// </summary>
        Awaitable<bool> LoadAsync(string slotId, CancellationToken ct = default);

        /// <summary>
        /// Like <see cref="LoadAsync"/>, but also returns the deserialized game data.
        /// Handy if you want to show some of it in the UI before loading (e.g. in a load menu).
        /// Returns <c>null</c> if the slot does not exist.
        /// </summary>
        Awaitable<bool> ExistsAsync(string slotId, CancellationToken ct = default);

        /// <summary>
        /// Delete all files related to this slot. Does nothing if the slot doesn't exist.
        /// </summary>
        Awaitable DeleteAsync(string slotId, CancellationToken ct = default);

        /// <summary>
        /// Read just the metadata (fast). Returns null if missing.
        /// </summary>
        Awaitable<SaveMetadata> LoadMetadataAsync(string slotId, CancellationToken ct = default);

        /// <summary>
        /// Load the saved screenshot. Returns <c>null</c> if none.
        /// </summary>
        Awaitable<Texture2D> LoadScreenshotAsync(string slotId, CancellationToken ct = default);

        /// <summary>
        /// Metadata of every existing save. Handy for a load/continue menu.
        /// </summary>
        Awaitable<IReadOnlyList<SaveMetadata>> ListSavesAsync(CancellationToken ct = default);
    }
}