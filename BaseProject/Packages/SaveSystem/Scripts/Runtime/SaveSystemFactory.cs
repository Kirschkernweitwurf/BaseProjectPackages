using System.Collections.Generic;
using System.IO;
using Base.SaveSystemPackage.Encryption;
using Base.SaveSystemPackage.Serialization;
using Base.SaveSystemPackage.Storage;
using UnityEngine;

namespace Base.SaveSystemPackage
{
    /// <summary>
    /// Builds a ready-to-use ISaveSystem. The caller does NOT decide which storage to
    /// use; the factory picks the right one for the current platform. Add console
    /// branches here and nothing else in the game has to change.
    /// </summary>
    public static class SaveSystemFactory
    {
        /// <summary>
        /// Creates a save system with the given settings.
        /// </summary>
        /// <param name="settings">The settings to use for the save system. If <c>null</c>, defaults will be used.</param>
        /// <returns>The created save system.</returns>
        public static ISaveSystem Create(SaveSystemSettings settings)
        {
            settings ??= new SaveSystemSettings();
            return CreateFileBased(settings);
        }

        private static ISaveSystem CreateFileBased(SaveSystemSettings settings)
        {
            string root = Path.Combine(Application.persistentDataPath, SaveSystemSettings.DefaultSubFolder);
            ISaveStorage storage = new FileSaveStorage(root);
            ISaveSerializer serializer = new JsonUtilitySerializer(settings.PrettyPrint);
            ISaveCodec codec = BuildCodec(settings, serializer);

            return new SaveSystem(storage, codec, settings.SaveVersion);
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
                    : System.Text.Encoding.UTF8.GetBytes(settings.Salt);
                aes = new AesEncryptor(settings.EncryptionPassphrase, saltBytes);
                readers.Add(aes);
            }

            bool encrypt = settings.ShouldEncryptOnWrite();
            ISaveEncryptor writer = encrypt && aes != null ? aes : noop;

            return new SaveCodec(serializer, writer, readers);
        }
    }
}