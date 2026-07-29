using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.SaveSystemPackage.Storage
{
    /// <summary>
    /// Stores bytes as files under <see cref="Application.persistentDataPath"/>. All disk work runs on
    /// a background thread and every call returns on the main thread.
    /// Writes go to a temp file that is then moved into place, so a crash mid-write cannot corrupt an
    /// existing save.
    /// </summary>
    public sealed class FileSaveStorage : ISaveStorage
    {
        /// <summary>Folder under the persistent data path that holds all save slots.</summary>
        public const string DefaultSubFolder = "Saves";

        private const string TempSuffix = ".tmp";

        private readonly string _root;

        /// <param name="root">
        /// Absolute folder to store saves in. Defaults to <see cref="DefaultSubFolder"/> under the
        /// persistent data path.
        /// </param>
        public FileSaveStorage(string root = null)
            => _root = root ?? Path.Combine(Application.persistentDataPath, DefaultSubFolder);

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

                string tempPath = path + TempSuffix;
                await File.WriteAllBytesAsync(tempPath, bytes, ct);

                // File.Replace swaps atomically, so there is no window where the file is gone.
                if (File.Exists(path))
                    File.Replace(tempPath, path, null);
                else
                    File.Move(tempPath, path);
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
                if (!File.Exists(path))
                    return;

                File.Delete(path);
                DeleteParentIfEmpty(path);
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
                foreach (string file in Directory.EnumerateFiles(_root, "*", SearchOption.AllDirectories))
                {
                    ct.ThrowIfCancellationRequested();

                    string relative = Path.GetRelativePath(_root, file)
                        .Replace(Path.DirectorySeparatorChar, '/');

                    // Half-written temp files are not keys anyone can read.
                    if (relative.EndsWith(TempSuffix, StringComparison.Ordinal))
                        continue;

                    if (prefix == null || relative.StartsWith(prefix, StringComparison.Ordinal))
                        result.Add(relative);
                }

                return result;
            }
            finally
            {
                await Awaitable.MainThreadAsync();
            }
        }

        private void DeleteParentIfEmpty(string path)
        {
            string parent = Path.GetDirectoryName(path);

            if (string.IsNullOrEmpty(parent)
                || parent == _root
                || !Directory.Exists(parent)
                || Directory.GetFileSystemEntries(parent).Length > 0)
                return;

            try
            {
                Directory.Delete(parent);
            }
            catch (IOException)
            {
                // Folder is busy. An empty folder left behind is harmless.
            }
            catch (UnauthorizedAccessException)
            {
                // Same: not worth failing the delete over.
            }
        }

        private bool TryGetPathForKey(string key, out string path)
        {
            path = string.Empty;

            if (string.IsNullOrWhiteSpace(key))
            {
                CustomLogger.LogWarning("Storage key is null or whitespace.", null);
                return false;
            }

            string safe = key.Replace('\\', '/');
            if (safe.Contains(".."))
            {
                CustomLogger.LogWarning($"Storage key '{key}' contains '..', which would escape the save "
                    + "folder. Rejecting it.", null);

                return false;
            }

            path = Path.Combine(_root, safe.Replace('/', Path.DirectorySeparatorChar));
            return true;
        }
    }
}