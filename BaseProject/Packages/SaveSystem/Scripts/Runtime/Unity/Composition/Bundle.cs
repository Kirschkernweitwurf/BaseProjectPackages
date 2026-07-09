using Base.SaveSystemPackage.Savable;
using Base.SaveSystemPackage.Slots;

namespace Base.SaveSystemPackage.Unity.Composition
{
    /// <summary>
    /// The bundle the <see cref="SaveManager"/> hands to the rest of the game: the system, the
    /// registry savables register with, the slot provider, and the runtime slot selection.
    /// </summary>
    public sealed class Bundle
    {
        public ISaveSystem System { get; }

        public ISavableRegistry Registry { get; }

        public ISaveSlotProvider Slots { get; }

        public SaveSlotSelection Selection { get; }

        public Bundle(ISaveSystem system, ISavableRegistry registry, ISaveSlotProvider slots,
            SaveSlotSelection selection)
        {
            System = system;
            Registry = registry;
            Slots = slots;
            Selection = selection;
        }
    }
}