using Base.CorePackage.Services;

namespace Base.SaveSystemPackage.Unity.Playtime
{
    /// <summary>
    /// Optional: anything that knows total play time can expose it here so the save button can stamp it
    /// into metadata. Keeps play-time tracking out of the save system.
    /// </summary>
    public interface IPlaytimeProvider : IGameService
    {
        /// <summary>Total play time in seconds, including the session that is running right now.</summary>
        double TotalSeconds { get; }
    }
}