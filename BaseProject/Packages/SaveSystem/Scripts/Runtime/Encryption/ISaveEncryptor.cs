namespace Base.SaveSystemPackage.Encryption
{
    /// <summary>
    /// Optional encryption layer. Use <see cref="NoOpEncryptor"/> while developing so files stay as
    /// readable JSON, and <see cref="AesEncryptor"/> for shipped builds.
    /// </summary>
    public interface ISaveEncryptor
    {
        /// <summary>
        /// Identifies the algorithm this encryptor implements. It is written to the save file header,
        /// so the system can read old saves even if the current settings have changed.
        /// </summary>
        EEncryptionAlgorithm Algorithm { get; }

        /// <summary>
        /// Encrypts the given bytes. The output can be a different length than the input.
        /// </summary>
        /// <param name="plain">The raw bytes to encrypt, as produced by the serializer.</param>
        /// <returns>The encrypted bytes to write to disk.</returns>
        byte[] Encrypt(byte[] plain);

        /// <summary>
        /// Decrypts the given bytes. The output can be a different length than the input.
        /// </summary>
        /// <param name="cipher">The encrypted bytes read from disk, as produced by <see cref="Encrypt"/>.</param>
        /// <returns>The decrypted bytes to pass to the deserializer.</returns>
        byte[] Decrypt(byte[] cipher);
    }
}