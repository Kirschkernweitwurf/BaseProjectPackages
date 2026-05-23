using System;
using System.Collections.Generic;
using System.Threading;
using Base.SaveSystemPackage.Encryption;
using Base.SaveSystemPackage.Savable;
using Base.SaveSystemPackage.Serialization;
using Base.SaveSystemPackage.Storage;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.SaveSystemPackage
{
    /// <summary>
    /// The default <see cref="ISaveSystem"/>. It uses an <see cref="ISaveStorage"/> for disk access,
    /// an <see cref="ISaveSerializer"/> for serialization and an <see cref="ISaveEncryptor"/> for encryption.
    /// Each save corresponds to three files in storage: one for the data, one for the metadata
    /// and one for the screenshot. The metadata contains timestamps, play time and other info
    /// that you might want to show in a save/load menu without having to load the full save data.
    /// The screenshot is optional, but if provided it will be saved
    /// as a PNG and can be loaded later for display in the UI.
    /// </summary>
    public sealed class SaveSystem : ISaveSystem
    {
        private const string DataSuffix = "/Save.dat";
        private const string MetaSuffix = "/Meta.dat";
        private const string ShotSuffix = "/Screenshot.png";

        private readonly int _saveVersion;
        private readonly ISaveCodec _codec;
        private readonly ISaveStorage _storage;

        public SaveSystem(ISaveStorage storage, ISaveCodec codec, int saveVersion = 1)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _codec = codec ?? throw new ArgumentNullException(nameof(codec));
            _saveVersion = saveVersion;
        }

        public async Awaitable SaveAsync(string slotId, string displayName = null, Texture2D screenshot = null,
            double? totalPlaySeconds = null, CancellationToken ct = default)
        {
            // Save data
            SaveBlob blob = new();
            foreach (ISavable s in SaveRegistry.GetOrdered())
                blob.entries.Add(new SaveEntry { id = s.SaveId, state = s.Serialize() ?? string.Empty });

            // Metadata
            DateTime nowUtc = DateTime.UtcNow;
            SaveMetadata meta = await LoadMetadataAsync(slotId, ct) ?? new SaveMetadata();
            meta.slotId = slotId;
            meta.saveVersion = _saveVersion;
            meta.appVersion = Application.version;
            meta.lastSavedUtcTicks = nowUtc.Ticks;

            if (meta.createdUtcTicks == 0)
                meta.createdUtcTicks = nowUtc.Ticks;

            if (displayName != null)
                meta.displayName = displayName;

            if (totalPlaySeconds.HasValue)
                meta.totalPlaySeconds = totalPlaySeconds.Value;

            // Screenshot
            byte[] shotBytes = null;
            if (screenshot != null)
            {
                shotBytes = screenshot.EncodeToPNG();
                meta.hasScreenshot = true;
                meta.screenshotWidth = screenshot.width;
                meta.screenshotHeight = screenshot.height;
            }

            byte[] dataBytes = _codec.Encode(blob);
            byte[] metaBytes = _codec.Encode(meta);

            await _storage.WriteAsync(DataKey(slotId), dataBytes, ct);
            await _storage.WriteAsync(MetaKey(slotId), metaBytes, ct);

            if (shotBytes != null)
                await _storage.WriteAsync(ShotKey(slotId), shotBytes, ct);
        }

        public async Awaitable<bool> LoadAsync(string slotId, CancellationToken ct = default)
        {
            byte[] bytes = await _storage.ReadAsync(DataKey(slotId), ct);
            if (bytes == null)
            {
                CustomLogger.LogWarning($"No save data found for slot '{slotId}'.", null);
                return false;
            }

            SaveBlob blob = _codec.Decode<SaveBlob>(bytes);

            // Hand state back, same priority order.
            // A savable with no stored entry gets null so it can reset itself.
            foreach (ISavable s in SaveRegistry.GetOrdered())
                s.Deserialize(blob.Find(s.SaveId));

            return true;
        }

        public Awaitable<bool> ExistsAsync(string slotId, CancellationToken ct = default)
        {
            return _storage.ExistsAsync(DataKey(slotId), ct);
        }

        public async Awaitable DeleteAsync(string slotId, CancellationToken ct = default)
        {
            await _storage.DeleteAsync(DataKey(slotId), ct);
            await _storage.DeleteAsync(MetaKey(slotId), ct);
            await _storage.DeleteAsync(ShotKey(slotId), ct);
        }

        public async Awaitable<SaveMetadata> LoadMetadataAsync(string slotId, CancellationToken ct = default)
        {
            byte[] bytes = await _storage.ReadAsync(MetaKey(slotId), ct);
            return bytes == null
                ? null
                : _codec.Decode<SaveMetadata>(bytes);
        }

        public async Awaitable<Texture2D> LoadScreenshotAsync(string slotId, CancellationToken ct = default)
        {
            byte[] bytes = await _storage.ReadAsync(ShotKey(slotId), ct);
            if (bytes == null)
                return null;

            Texture2D tex = new(2, 2);
            tex.LoadImage(bytes);
            return tex;
        }

        public async Awaitable<IReadOnlyList<SaveMetadata>> ListSavesAsync(CancellationToken ct = default)
        {
            IReadOnlyList<string> keys = await _storage.ListKeysAsync(null, ct);

            List<SaveMetadata> result = new();
            foreach (string key in keys)
            {
                if (!key.EndsWith(MetaSuffix))
                    continue;

                byte[] bytes = await _storage.ReadAsync(key, ct);
                if (bytes == null)
                    continue;

                try
                {
                    result.Add(_codec.Decode<SaveMetadata>(bytes));
                }
                catch
                {
                    CustomLogger.LogWarning($"Failed to decode save metadata for key '{key}'.", null);
                }
            }

            return result;
        }

        private static string DataKey(string slotId) => slotId + DataSuffix;

        private static string MetaKey(string slotId) => slotId + MetaSuffix;

        private static string ShotKey(string slotId) => slotId + ShotSuffix;
    }
}