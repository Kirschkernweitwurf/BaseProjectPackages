using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Base.SaveSystemPackage.Storage
{
    /// <summary>
    /// Raw byte storage. This is the layer you swap for a console (Switch /
    /// PlayStation / Xbox) that uses its own save API instead of files.
    /// </summary>
    public interface ISaveStorage
    {
        /// <summary>
        /// Writes the bytes to the given key. Overwrites if the key already exists.
        /// </summary>
        Awaitable WriteAsync(string key, byte[] bytes, CancellationToken ct = default);

        /// <summary>
        /// Returns the bytes or <c>null</c> if the key does not exist.
        /// </summary>
        Awaitable<byte[]> ReadAsync(string key, CancellationToken ct = default);

        /// <summary>
        /// Returns true if the key exists. Handy to check for a save slot's existence without reading its data.
        /// </summary>
        Awaitable<bool> ExistsAsync(string key, CancellationToken ct = default);

        /// <summary>
        /// Deletes the key and its data. Does nothing if the key doesn't exist.
        /// </summary>
        Awaitable DeleteAsync(string key, CancellationToken ct = default);

        /// <summary>
        /// All keys, optionally filtered by a prefix.
        /// </summary>
        Awaitable<IReadOnlyList<string>> ListKeysAsync(string prefix = null, CancellationToken ct = default);
    }
}