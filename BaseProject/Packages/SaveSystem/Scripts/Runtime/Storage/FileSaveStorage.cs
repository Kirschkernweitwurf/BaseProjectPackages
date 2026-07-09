using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.SaveSystemPackage.Storage
{
    /// <summary>
    /// Stores bytes as files under <c>Application.persistentDataPath</c>.
    /// All disk work runs on a background thread and the call returns on the main thread.
    /// Writes are atomic, so we write to a temp file first, then move it, so a crash mid-save can't corrupt your save.
    /// </summary>
    public sealed class FileSaveStorage : ISaveStorage
    {
        private readonly string _root;

        public FileSaveStorage(string root = null)
            => _root = root ?? Path.Combine(Application.persistentDataPath, "Saves");

        /// <inheritdoc/>
        public async Awaitable WriteAsync(string key, byte[] bytes, CancellationToken ct = default)
        {
            if (!TryGetPathForKey(key, out string path))
                return;

            await Awaitable.BackgroundThreadAsync();
            try
            {
                ct.ThrowIfCancellationRequested();
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);

                string tmp = path + ".tmp";
                await File.WriteAllBytesAsync(tmp, bytes, ct);

                if (File.Exists(path))
                    File.Delete(path);

                File.Move(tmp, path);
            }
            finally
            {
                await Awaitable.MainThreadAsync();
            }
        }

        /// <inheritdoc/>
        public async Awaitable<byte[]> ReadAsync(string key, CancellationToken ct = default)
        {
            if (!TryGetPathForKey(key, out string path))
                return null;

            await Awaitable.BackgroundThreadAsync();
            try
            {
                ct.ThrowIfCancellationRequested();
                return File.Exists(path)
                    ? await File.ReadAllBytesAsync(path, ct)
                    : null;
            }
            finally
            {
                await Awaitable.MainThreadAsync();
            }
        }

        /// <inheritdoc/>
        public async Awaitable<bool> ExistsAsync(string key, CancellationToken ct = default)
        {
            if (!TryGetPathForKey(key, out string path))
                return false;

            await Awaitable.BackgroundThreadAsync();
            try
            {
                return File.Exists(path);
            }
            finally
            {
                await Awaitable.MainThreadAsync();
            }
        }

        /// <inheritdoc/>
        public async Awaitable DeleteAsync(string key, CancellationToken ct = default)
        {
            if (!TryGetPathForKey(key, out string path))
                return;

            await Awaitable.BackgroundThreadAsync();
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);

                    // Remove the parent folder if it's empty now
                    string parent = Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(parent)
                        && parent != _root
                        && Directory.Exists(parent)
                        && Directory.GetFileSystemEntries(parent).Length == 0)
                        try
                        {
                            Directory.Delete(parent);
                        }
                        catch
                        {
                            /* folder busy or already gone; not a problem */
                        }
                }
            }
            finally
            {
                await Awaitable.MainThreadAsync();
            }
        }

        /// <inheritdoc/>
        public async Awaitable<IReadOnlyList<string>> ListKeysAsync(string prefix = null,
            CancellationToken ct = default)
        {
            await Awaitable.BackgroundThreadAsync();
            try
            {
                if (!Directory.Exists(_root))
                    return Array.Empty<string>();

                List<string> result = new();
                foreach (string file in Directory.GetFiles(_root, "*", SearchOption.AllDirectories))
                {
                    ct.ThrowIfCancellationRequested();

                    string rel = Path.GetRelativePath(_root, file)
                        .Replace(Path.DirectorySeparatorChar, '/');

                    if (rel.EndsWith(".tmp"))
                        continue; // skip half-written temp files

                    if (prefix == null || rel.StartsWith(prefix))
                        result.Add(rel);
                }

                return result;
            }
            finally
            {
                await Awaitable.MainThreadAsync();
            }
        }

        private bool TryGetPathForKey(string key, out string path)
        {
            path = string.Empty;

            if (string.IsNullOrWhiteSpace(key))
            {
                CustomLogger.LogWarning("Key is null or whitespace. This is not valid!", null);
                return false;
            }

            string safe = key.Replace('\\', '/');
            if (safe.Contains(".."))
            {
                CustomLogger.LogWarning("Key contains '..', which is not allowed for security reasons!", null);
                return false;
            }

            path = Path.Combine(_root, safe.Replace('/', Path.DirectorySeparatorChar));
            return true;
        }
    }
}