namespace Base.SaveSystemPackage.Encryption
{
    /// <summary>
    /// When to encrypt saves on WRITE. Reading always auto-detects, so any of these
    /// can still read both plain and encrypted saves.
    /// </summary>
    public enum EEncryption : byte
    {
        Auto = 0,
        On = 1,
        Off = 2
    }
}