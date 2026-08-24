using System;
using System.Collections.Generic;
using System.Text;
using Base.SaveSystemPackage.Backup;
using Base.SaveSystemPackage.Core;
using Base.SaveSystemPackage.Encryption;
using Base.SaveSystemPackage.Savable;
using Base.SaveSystemPackage.Serialization;
using Base.SaveSystemPackage.Slots;
using Base.SaveSystemPackage.Storage;
using Base.UtilityPackage.Logging;

namespace Base.SaveSystemPackage.Unity.Composition
{
    /// <summary>
    /// Builds a ready-to-use save system. The caller does not decide which storage to use; the factory
    /// picks the right one for the current platform. Add console branches here and nothing else in the
    /// game has to change.
    /// </summary>
    public static class SaveSystemFactory
    {
        /// <summary>
        /// Wires up storage, codec, serializer, registry, backups and slot provider from the given
        /// settings.
        /// </summary>
        /// <param name="settings">The authored settings, or <c>null</c> to use the defaults.</param>
        /// <param name="migrations">
        /// Steps that upgrade older saves on load. <c>null</c> falls back to whatever the settings say,
        /// which by default is every migration found in the project.
        /// </param>
        /// <returns>Every part the game needs, ready to use.</returns>
        public static SaveSystemBundle Create(SaveSystemSettings settings,
            IReadOnlyList<ISaveMigration> migrations = null)
        {
            settings ??= new SaveSystemSettings();

            ISaveStorage storage = new FileSaveStorage();
            ISaveSerializer serializer = new JsonUtilitySerializer(settings.PrettyPrint);
            ISaveCodec codec = BuildCodec(settings, serializer);
            ISavableRegistry registry = new SavableRegistry();
            ISaveBackups backups = new SaveBackups(storage, settings.KeptBackups);

            ISaveSystem system = new SaveSystem(storage, codec, registry, settings.SaveVersion,
                migrations ?? ResolveMigrations(settings), backups);

            ISaveSlotProvider slots = BuildSlotProvider(settings, system);

            return new SaveSystemBundle(system, registry, slots, new SaveSlotSelection(), backups);
        }

        private static IReadOnlyList<ISaveMigration> ResolveMigrations(SaveSystemSettings settings)
            => settings.AutoDiscoverMigrations
                ? SaveMigrationDiscovery.Discover()
                : Array.Empty<ISaveMigration>();

        private static ISaveSlotProvider BuildSlotProvider(SaveSystemSettings settings, ISaveSystem system)
            => settings.SlotModel switch
            {
                ESlotModel.Fixed => new FixedSlotProvider(system, settings.FixedSlotCount),
                ESlotModel.Appending => new AppendingSlotProvider(system, system, settings.MaxAppendingSaves),
                _ => new NamedSlotProvider(system)
            };

        private static ISaveCodec BuildCodec(SaveSystemSettings settings, ISaveSerializer serializer)
        {
            NoOpEncryptor noOpEncryptor = new();
            List<ISaveEncryptor> readEncryptors = new()
            {
                noOpEncryptor
            };

            AesEncryptor aesEncryptor = null;
            if (!string.IsNullOrEmpty(settings.EncryptionPassphrase))
            {
                byte[] salt = string.IsNullOrEmpty(settings.Salt)
                    ? null
                    : Encoding.UTF8.GetBytes(settings.Salt);

                aesEncryptor = new AesEncryptor(settings.EncryptionPassphrase, salt);
                readEncryptors.Add(aesEncryptor);
            }

            bool shouldEncrypt = settings.ShouldEncryptOnWrite();

            // Falling back quietly would ship plain saves from a project that asked for encryption.
            if (shouldEncrypt && aesEncryptor == null)
                CustomLogger.LogError($"Encryption is set to '{settings.Encryption}' but no passphrase is "
                    + "configured, so saves are written unencrypted.", null);

            ISaveEncryptor writeEncryptor = shouldEncrypt && aesEncryptor != null
                ? aesEncryptor
                : noOpEncryptor;

            return new SaveCodec(serializer, writeEncryptor, readEncryptors);
        }
    }
}