using System;
using Base.SaveSystemPackage.Encryption;
using UnityEngine;

namespace Base.SaveSystemPackage
{
    /// <summary>
    /// All save settings, shaped for the inspector. Lives on the SaveManager component,
    /// so designers/you can change it without code.
    ///
    /// Note on the encryption choice: a single bool cannot mean "off in editor, on in
    /// build" because it is one stored value. The 3-way enum makes that intent explicit
    /// while still being easy to force either way.
    /// </summary>
    [Serializable]
    public sealed class SaveSystemSettings
    {
        public const string DefaultSubFolder = "Saves";

        [field: Tooltip("Encryption mode on write. Reading always auto-detects.")]
        [field: SerializeField] public EEncryption Encryption { get; private set; } = EEncryption.Auto;

        [field: Tooltip("Secret used for AES. Change it for your project. Keep it stable across versions.")]
        [field: SerializeField] public string EncryptionPassphrase { get; private set; } = "ChangeThis";

        [field: Tooltip("Salt used for AES key derivation. Change it for your project. Keep it stable across versions.")]
        [field: SerializeField] public string Salt { get; private set; } = "ChangeThis";

        [field: Tooltip("Indent JSON so it is easy to read. Handy while developing.")]
        [field: SerializeField] public bool PrettyPrint { get; private set; } = true;

        [field: Tooltip("Schema version. Bump it when your save data layout changes.")]
        [field: SerializeField] public int SaveVersion { get; private set; } = 1;

        /// <summary>
        /// Resolves the enum into a concrete yes/no for the current context.
        /// </summary>
        public bool ShouldEncryptOnWrite() => Encryption switch
        {
            EEncryption.On => true,
            EEncryption.Off => false,
            _ => !Application.isEditor // Auto
        };
    }
}