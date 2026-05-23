using Base.SaveSystemPackage.Storage;

namespace Base.SaveSystemPackage.Encryption
{
    /// <summary>
    /// Optional encryption layer. Use NoOpEncryptor while developing
    /// (files stay as readable JSON), and AesEncryptor for shipped builds.
    /// </summary>
    public interface ISaveEncryptor
    {
        /// <summary>
        /// Identifies the encryption strategy used by this encryptor.
        /// This is written to the save file header,
        /// so the system can read old saves even if the current settings have changed.
        /// </summary>
        ESaveEncryption Mode { get; }

        /// <summary>
        /// Encrypts the given byte array. The output can be a different length than the input.
        /// </summary>
        /// <param name="plain">
        /// The raw bytes to encrypt. This is the output of the serializer, so it can be as long as you want.
        /// </param>
        /// <returns>
        /// The encrypted bytes to write to disk.
        /// </returns>
        byte[] Encrypt(byte[] plain);

        /// <summary>
        /// Decrypts the given byte array. The output can be a different length than the input.
        /// </summary>
        /// <param name="cipher">
        /// The encrypted bytes read from disk.
        /// This is the output of the Encrypt method, so it can be as long as you want.
        /// </param>
        /// <returns>
        /// The decrypted bytes to pass to the deserializer.
        /// </returns>
        byte[] Decrypt(byte[] cipher);
    }
}