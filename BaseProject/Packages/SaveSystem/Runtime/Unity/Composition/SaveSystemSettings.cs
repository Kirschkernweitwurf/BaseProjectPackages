using System;
using Base.AttributePackage;
using Base.SaveSystemPackage.Encryption;
using Base.SaveSystemPackage.Serialization;
using Base.SaveSystemPackage.Slots;
using UnityEngine;

namespace Base.SaveSystemPackage.Unity.Composition
{
    /// <summary>
    /// All save settings, shaped for the inspector. Lives on the <see cref="SaveManager"/> component,
    /// so the slot model, encryption and versioning can be changed without code.
    /// On the encryption choice: a single bool cannot mean "off in editor, on in build" because it is
    /// one stored value. The three-way enum makes that intent explicit while still being forceable.
    /// </summary>
    [Serializable]
    public sealed class SaveSystemSettings
    {
        [field: Title("Slot Model")]
        [field: Tooltip("Fixed: numbered slots. Appending: a new save each time. Named: unlimited named slots.")]
        [field: EnumToggleButtons]
        [field: SerializeField] public ESlotModel SlotModel { get; private set; } = ESlotModel.Named;

        [field: Tooltip("How many slots when " + nameof(SlotModel) + " is " + nameof(ESlotModel.Fixed) + ".")]
        [field: ShowIfEnum(nameof(SlotModel), ESlotModel.Fixed)]
        [field: SerializeField] public int FixedSlotCount { get; private set; } = 3;

        [field: Tooltip("Max kept saves when "
            + nameof(SlotModel)
            + " is "
            + nameof(ESlotModel.Appending)
            + ". 0 means unlimited.")]
        [field: ShowIfEnum(nameof(SlotModel), ESlotModel.Appending)]
        [field: SerializeField] public int MaxAppendingSaves { get; private set; } = 20;

        [field: Title("Encryption")]
        [field: Tooltip("Encryption mode on write. Reading always auto-detects.")]
        [field: EnumToggleButtons]
        [field: SerializeField] public EEncryptionMode Encryption { get; private set; } = EEncryptionMode.Auto;

        [field: Tooltip("Secret used for AES. Set your own per project and keep it stable across versions.")]
        [field: ShowIfEnum(nameof(Encryption), EEncryptionMode.Auto, EEncryptionMode.On)]
        [field: NotNullOrEmpty]
        [field: SerializeField] public string EncryptionPassphrase { get; private set; } = string.Empty;

        [field: Tooltip("Salt used for AES key derivation. Set your own per project and keep it stable.")]
        [field: ShowIfEnum(nameof(Encryption), EEncryptionMode.Auto, EEncryptionMode.On)]
        [field: NotNullOrEmpty]
        [field: SerializeField] public string Salt { get; private set; } = string.Empty;

        [field: Title("Serialization")]
        [field: Tooltip("Indent JSON so it is easy to read. Handy while developing.")]
        [field: SerializeField] public bool PrettyPrint { get; private set; } = true;

        [field: Tooltip("Schema version. Bump it when your save data layout changes, and add a migration.")]
        [field: NotZero]
        [field: SerializeField] public int SaveVersion { get; private set; } = 1;

        [field: Tooltip("Find every "
            + nameof(ISaveMigration)
            + " in the project and register it automatically. "
            + "Turn off to hand the steps to the factory yourself.")]
        [field: SerializeField] public bool AutoDiscoverMigrations { get; private set; } = true;

        [field: Title("Backups")]
        [field: Tooltip("How many previous saves to keep per slot, used to recover a damaged save. 0 turns "
            + "backups off. Each save copies the previous one aside once, whatever the count.")]
        [field: Min(0)]
        [field: SerializeField] public int KeptBackups { get; private set; } = 2;

        /// <summary>Resolves the encryption mode into a concrete yes or no for the current context.</summary>
        /// <returns><c>true</c> when the next write should be encrypted.</returns>
        public bool ShouldEncryptOnWrite() => Encryption switch
        {
            EEncryptionMode.On => true,
            EEncryptionMode.Off => false,
            _ => !Application.isEditor
        };
    }
}