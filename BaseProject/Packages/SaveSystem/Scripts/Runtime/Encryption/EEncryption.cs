using UnityEngine;

namespace Base.SaveSystemPackage.Encryption
{
    /// <summary>
    /// When to encrypt saves on WRITE. Reading always auto-detects, so any of these
    /// can still read both plain and encrypted saves.
    /// </summary>
    public enum EEncryption
    {
        [Tooltip("Off in the editor, on in real builds. Recommended.")]
        Auto = 0,

        [Tooltip("Always encrypt, even in the editor.")]
        On = 1,

        [Tooltip("Never encrypt. Saves stay readable JSON.")]
        Off = 2
    }
}