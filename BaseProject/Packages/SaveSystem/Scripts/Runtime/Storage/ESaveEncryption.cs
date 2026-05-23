namespace Base.SaveSystemPackage.Storage
{
    /// <summary>
    /// Encryption modes. The byte value is written into the save header,
    /// so the loader can detect which encryptor to use.
    /// </summary>
    public enum ESaveEncryption : byte
    {
        None = 0,
        Aes = 1
    }
}