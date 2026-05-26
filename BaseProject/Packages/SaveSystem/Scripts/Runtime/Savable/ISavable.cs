using Base.SystemsCorePackage.Tracking;

namespace Base.SaveSystemPackage.Savable
{
    /// <summary>
    /// Implemented by small, decoupled classes that own a piece of save data.
    /// </summary>
    public interface ISavable
    {
        /// <summary>
        /// Stable, unique key inside the save file. Never change it once shipped.
        /// </summary>
        SaveId SaveId { get; }

        /// <summary>
        /// Higher priority runs first on both save and load.
        /// </summary>
        EPriority Priority { get; }

        /// <summary>
        /// Return this object's state as a string.
        /// </summary>
        string Serialize();

        /// <summary>
        /// Receive the state saved earlier. Gets <c>null</c> if this slot had no data
        /// for this SaveId (e.g. a brand new savable).
        /// </summary>
        void Deserialize(string state);
    }
}