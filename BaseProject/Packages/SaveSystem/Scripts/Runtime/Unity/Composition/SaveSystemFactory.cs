using System.Collections.Generic;
using System.IO;
using System.Text;
using Base.SaveSystemPackage.Encryption;
using Base.SaveSystemPackage.Savable;
using Base.SaveSystemPackage.Serialization;
using Base.SaveSystemPackage.Slots;
using Base.SaveSystemPackage.Storage;
using Base.SaveSystemPackage.System;
using Base.SaveSystemPackage.Unity.Composition;
using UnityEngine;

namespace Base.SaveSystemPackage
{
    /// <summary>
    /// Builds a ready-to-use save system. The caller does NOT decide which storage to use; the
    /// factory picks the right one for the current platform. Add console branches here and nothing
    /// else in the game has to change.
    /// </summary>
    public static class SaveSystemFactory
    {
        public static Bundle Create(SaveSystemSettings settings, IReadOnlyList<ISaveMigration> migrations = null)
        {
            settings ??= new SaveSystemSettings();

            string root = Path.Combine(Application.persistentDataPath, SaveSystemSettings.DefaultSubFolder);
            ISaveStorage storage = new FileSaveStorage(root);
            ISaveSerializer serializer = new JsonUtilitySerializer(settings.PrettyPrint);
            ISaveCodec codec = BuildCodec(settings, serializer);
            ISavableRegistry registry = new SavableRegistry();

            ISaveSystem system = new SaveSystem(storage, codec, registry, settings.SaveVersion, migrations);
            ISaveSlotProvider slots = BuildSlotProvider(settings, system);
            SaveSlotSelection selection = new();

            return new Bundle(system, registry, slots, selection);
        }

        private static ISaveSlotProvider BuildSlotProvider(SaveSystemSettings settings, ISaveSystem system)
        {
            return settings.SlotModel switch
            {
                ESlotModel.Fixed => new FixedSlotProvider(system, settings.FixedSlotCount),
                ESlotModel.Appending => new AppendingSlotProvider(system, system, settings.MaxAppendingSaves),
                _ => new NamedSlotProvider(system)
            };
        }

        private static ISaveCodec BuildCodec(SaveSystemSettings settings, ISaveSerializer serializer)
        {
            NoOpEncryptor noop = new();
            List<ISaveEncryptor> readers = new() { noop };

            ISaveEncryptor aes = null;
            if (!string.IsNullOrEmpty(settings.EncryptionPassphrase))
            {
                byte[] saltBytes = string.IsNullOrEmpty(settings.Salt)
                    ? null
                    : Encoding.UTF8.GetBytes(settings.Salt);
                aes = new AesEncryptor(settings.EncryptionPassphrase, saltBytes);
                readers.Add(aes);
            }

            bool encrypt = settings.ShouldEncryptOnWrite();
            ISaveEncryptor writer = encrypt && aes != null ? aes : noop;

            return new SaveCodec(serializer, writer, readers);
        }
    }
}