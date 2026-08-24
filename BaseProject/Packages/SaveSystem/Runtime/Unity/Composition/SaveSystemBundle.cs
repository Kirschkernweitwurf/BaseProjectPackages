using Base.SaveSystemPackage.Backup;
using Base.SaveSystemPackage.Core;
using Base.SaveSystemPackage.Savable;
using Base.SaveSystemPackage.Slots;

namespace Base.SaveSystemPackage.Unity.Composition
{
    /// <summary>
    /// Everything <see cref="SaveSystemFactory"/> wires up, in one piece, so <see cref="SaveManager"/>
    /// can hand the parts to the rest of the game without knowing how they were built.
    /// </summary>
    public readonly struct SaveSystemBundle
    {
        /// <summary>The read and write API.</summary>
        public ISaveSystem System { get; }

        /// <summary>The registry savables register with. Same instance the system collects from.</summary>
        public ISavableRegistry Registry { get; }

        /// <summary>The provider that owns slot bookkeeping for the configured model.</summary>
        public ISaveSlotProvider Slots { get; }

        /// <summary>The runtime holder for the slot the player has selected.</summary>
        public SaveSlotSelection Selection { get; }

        /// <summary>The kept previous versions of each slot. Same instance the system recovers from.</summary>
        public ISaveBackups Backups { get; }

        /// <param name="system">The read and write API.</param>
        /// <param name="registry">The registry savables register with.</param>
        /// <param name="slots">The provider for the configured slot model.</param>
        /// <param name="selection">The runtime slot selection.</param>
        /// <param name="backups">The kept previous versions of each slot.</param>
        public SaveSystemBundle(ISaveSystem system, ISavableRegistry registry, ISaveSlotProvider slots,
            SaveSlotSelection selection, ISaveBackups backups)
        {
            System = system;
            Registry = registry;
            Slots = slots;
            Selection = selection;
            Backups = backups;
        }
    }
}