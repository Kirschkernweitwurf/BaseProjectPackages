namespace Base.SaveSystemPackage.Encryption
{
    /// <summary>
    /// Does nothing. Use this while developing so save files stay as plain,
    /// readable JSON that you can open and edit by hand.
    /// </summary>
    public sealed class NoOpEncryptor : ISaveEncryptor
    {
        public ESaveEncryption Mode => ESaveEncryption.None;

        /// <summary>
        /// Returns the input bytes unchanged.
        /// </summary>
        public byte[] Encrypt(byte[] plain) => plain;

        /// <summary>
        /// Returns the input bytes unchanged.
        /// </summary>
        public byte[] Decrypt(byte[] cipher) => cipher;
    }
}