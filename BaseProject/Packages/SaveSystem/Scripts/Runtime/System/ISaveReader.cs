using System.Collections.Generic;
using System.Threading;
using Base.SaveSystemPackage.Model;
using UnityEngine;

namespace Base.SaveSystemPackage.System
{
    /// <summary>
    /// Read-only side of the save system. A load/continue menu only needs this, so it can depend
    /// on the narrow surface instead of the whole system (interface segregation).
    /// State is not returned; on load it is handed back to the registered savables.
    /// </summary>
    public interface ISaveReader
    {
        /// <summary>Read a save and hand state back to all registered savables.</summary>
        Awaitable<ESaveLoadResult> LoadAsync(string slotId, CancellationToken ct = default);

        /// <summary>True if a completed save exists in this slot.</summary>
        Awaitable<bool> ExistsAsync(string slotId, CancellationToken ct = default);

        /// <summary>Read just the metadata (fast). Returns <c>null</c> if the slot is empty.</summary>
        Awaitable<SaveMetadata> LoadMetadataAsync(string slotId, CancellationToken ct = default);

        /// <summary>
        /// Load the saved screenshot as raw PNG bytes, or <c>null</c> if none. The core stays
        /// rendering-agnostic; turn the bytes into a Texture2D in your UI with <c>tex.LoadImage</c>.
        /// </summary>
        Awaitable<byte[]> LoadScreenshotPngAsync(string slotId, CancellationToken ct = default);

        /// <summary>Metadata of every existing save, for a load/continue menu.</summary>
        Awaitable<IReadOnlyList<SaveMetadata>> ListSavesAsync(CancellationToken ct = default);
    }
}