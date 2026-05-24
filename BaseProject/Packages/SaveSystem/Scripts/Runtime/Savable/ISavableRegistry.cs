using System.Collections.Generic;

namespace Base.SaveSystemPackage.Savable
{
    /// <summary>
    /// Where savables register. Injected into the save system instead of being a global static,
    /// so the system has no hidden dependencies and can be unit-tested, and so two independent
    /// save contexts can coexist (e.g. main game and a separate profile).
    /// </summary>
    public interface ISavableRegistry
    {
        /// <summary>Register a savable. Ignored if already present or if its SaveId is taken.</summary>
        void Register(ISavable savable);

        /// <summary>Remove a savable, e.g. when it is destroyed.</summary>
        void Deregister(ISavable savable);

        /// <summary>All savables, highest priority first, ties keeping registration order.</summary>
        IReadOnlyList<ISavable> GetOrdered();
    }
}