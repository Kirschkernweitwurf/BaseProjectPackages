namespace Base.SaveSystemPackage.Encryption
{
    /// <summary>
    /// When to encrypt saves on write. Reading always auto-detects, so every mode can still read both
    /// plain and encrypted saves.
    /// </summary>
    public enum EEncryptionMode : byte
    {
        /// <summary>Plain in the editor so saves stay readable, encrypted in a build.</summary>
        Auto = 0,

        /// <summary>Always encrypt, including in the editor.</summary>
        On = 1,

        /// <summary>Never encrypt.</summary>
        Off = 2
    }
}