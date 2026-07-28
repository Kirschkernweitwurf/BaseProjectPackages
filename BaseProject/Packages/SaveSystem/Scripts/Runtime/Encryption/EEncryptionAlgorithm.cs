namespace Base.SaveSystemPackage.Encryption
{
    /// <summary>
    /// Which algorithm wrote a save. The byte value goes into the save header, so the loader can pick
    /// the matching <see cref="ISaveEncryptor"/> even if the current settings have changed since.
    /// </summary>
    public enum EEncryptionAlgorithm : byte
    {
        /// <summary>Not encrypted. Plain, hand-editable JSON.</summary>
        None = 0,

        /// <summary>AES-256 with a fresh random IV per save.</summary>
        Aes = 1
    }
}