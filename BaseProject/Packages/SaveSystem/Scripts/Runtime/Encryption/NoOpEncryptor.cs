namespace Base.SaveSystemPackage.Encryption
{
    /// <summary>
    /// Does nothing. Use this while developing so save files stay as plain, readable JSON that you can
    /// open and edit by hand.
    /// </summary>
    public sealed class NoOpEncryptor : ISaveEncryptor
    {
        /// <inheritdoc/>
        public EEncryptionAlgorithm Algorithm => EEncryptionAlgorithm.None;

        /// <inheritdoc/>
        public byte[] Encrypt(byte[] plain) => plain;

        /// <inheritdoc/>
        public byte[] Decrypt(byte[] cipher) => cipher;
    }
}