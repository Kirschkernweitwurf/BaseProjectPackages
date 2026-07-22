using Base.SaveSystemPackage.Unity.Composition;

namespace Base.SaveSystemPackage.Slots
{
    /// <summary>
    /// Which slot layout the game uses. Picked in the <see cref="SaveManager"/> settings; the factory builds
    /// the matching <see cref="ISaveSlotProvider"/>.
    /// </summary>
    public enum ESlotModel : byte
    {
        /// <summary>A fixed number of numbered slots the player overwrites in place.</summary>
        Fixed = 0,

        /// <summary>Every save creates a new entry, optionally capped with auto-prune.</summary>
        Appending = 1,

        /// <summary>Unlimited named slots; saving overwrites the chosen one.</summary>
        Named = 2
    }
}