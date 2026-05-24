using Base.SaveSystemPackage.Savable;
using Base.SaveSystemPackage.Slots;

namespace Base.SaveSystemPackage.Unity.Composition
{
    /// <summary>
    /// The bundle the <see cref="SaveManager"/> hands to the rest of the game. The system, the registry
    /// savables register with, and the slot provider the menu uses.
    /// </summary>
    public sealed class Bundle
    {
        public ISaveSystem System { get; }
        public ISavableRegistry Registry { get; }
        public ISaveSlotProvider Slots { get; }

        public Bundle(ISaveSystem system, ISavableRegistry registry, ISaveSlotProvider slots)
        {
            System = system;
            Registry = registry;
            Slots = slots;
        }
    }
}