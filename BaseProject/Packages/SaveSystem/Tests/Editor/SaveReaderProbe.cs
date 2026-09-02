using System.Collections.Generic;
using System.Threading;
using Base.SaveSystemPackage.Core;
using Base.SaveSystemPackage.Model;
using Base.UtilityPackage.Async;
using UnityEngine;

namespace Base.SaveSystemPackage.Tests
{
    /// <summary>
    /// A reader that answers from a list handed in rather than from disk. The slot provider tests only
    /// exercise the synchronous half of a provider, so every member here answers immediately.
    /// </summary>
    internal sealed class SaveReaderProbe : ISaveReader
    {
        private readonly Dictionary<string, SaveMetadata> _saves = new();

        /// <summary>Files metadata under a slot, so the reader reports that slot as taken.</summary>
        /// <param name="metadata">The metadata to hand out for its own slot.</param>
        internal void Add(SaveMetadata metadata) => _saves[metadata.SlotId] = metadata;

        /// <inheritdoc/>
        public Awaitable<ESaveLoadResult> LoadAsync(string slotId, CancellationToken ct = default)
            => AwaitableUtility.FromResult(ESaveLoadResult.Success);

        /// <inheritdoc/>
        public Awaitable<bool> ExistsAsync(string slotId, CancellationToken ct = default)
            => AwaitableUtility.FromResult(_saves.ContainsKey(slotId));

        /// <inheritdoc/>
        public Awaitable<SaveMetadata> LoadMetadataAsync(string slotId, CancellationToken ct = default)
            => AwaitableUtility.FromResult(_saves.GetValueOrDefault(slotId));

        /// <inheritdoc/>
        public Awaitable<byte[]> LoadScreenshotPngAsync(string slotId, CancellationToken ct = default)
            => AwaitableUtility.FromResult<byte[]>(null);

        /// <inheritdoc/>
        public Awaitable<IReadOnlyList<SaveMetadata>> ListSavesAsync(CancellationToken ct = default)
            => AwaitableUtility.FromResult<IReadOnlyList<SaveMetadata>>(new List<SaveMetadata>(_saves.Values));
    }
}