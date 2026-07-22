using System;
using Base.AttributePackage;
using Base.SaveSystemPackage.Encryption;
using Base.SaveSystemPackage.Slots;
using UnityEngine;

namespace Base.SaveSystemPackage.Unity.Composition
{
    /// <summary>
    /// All save settings, shaped for the inspector. Lives on the SaveManager component, so the
    /// slot model, encryption and versioning can be changed without code.
    /// On the encryption choice: a single bool cannot mean "off in editor, on in build" because it
    /// is one stored value. The 3-way enum makes that intent explicit while still being forceable.
    /// </summary>
    [Serializable]
    public sealed class SaveSystemSettings
    {
        public const string DefaultSubFolder = "Saves";

        [field: Title("Slot Model")]
        [field: Tooltip("Fixed: numbered slots. Appending: a new save each time. Named: unlimited named slots.")]
        [field: EnumToggleButtons]
        [field: SerializeField] public ESlotModel SlotModel { get; private set; } = ESlotModel.Named;

        [field: Tooltip("How many slots when SlotModel is Fixed.")]
        [field: ShowIfEnum(nameof(SlotModel), ESlotModel.Fixed)]
        [field: SerializeField] public int FixedSlotCount { get; private set; } = 3;

        [field: Tooltip("Max kept saves when SlotModel is Appending. 0 = unlimited.")]
        [field: ShowIfEnum(nameof(SlotModel), ESlotModel.Appending)]
        [field: SerializeField] public int MaxAppendingSaves { get; private set; } = 20;

        [field: Title("Encryption")]
        [field: Tooltip("Encryption mode on write. Reading always auto-detects.")]
        [field: EnumToggleButtons]
        [field: SerializeField] public EEncryption Encryption { get; private set; } = EEncryption.Auto;

        [field: Tooltip("Secret used for AES. Change it for your project. Keep it stable across versions.")]
        [field: ShowIfEnum(nameof(Encryption), EEncryption.Auto, EEncryption.On)]
        [field: SerializeField] public string EncryptionPassphrase { get; private set; } = "ChangeThis";

        [field:
            Tooltip("Salt used for AES key derivation. Change it for your project. Keep it stable across versions.")]
        [field: ShowIfEnum(nameof(Encryption), EEncryption.Auto, EEncryption.On)]
        [field: SerializeField] public string Salt { get; private set; } = "ChangeThis";

        [field: Title("Serialization")]
        [field: Tooltip("Indent JSON so it is easy to read. Handy while developing.")]
        [field: SerializeField] public bool PrettyPrint { get; private set; } = true;

        [field: Tooltip("Schema version. Bump it when your save data layout changes, and add a migration.")]
        [field: NotZero]
        [field: SerializeField] public int SaveVersion { get; private set; } = 1;

        /// <summary>Resolves the encryption enum into a concrete yes/no for the current context.</summary>
        public bool ShouldEncryptOnWrite() => Encryption switch
        {
            EEncryption.On => true,
            EEncryption.Off => false,
            _ => !Application.isEditor // Auto
        };
    }
}