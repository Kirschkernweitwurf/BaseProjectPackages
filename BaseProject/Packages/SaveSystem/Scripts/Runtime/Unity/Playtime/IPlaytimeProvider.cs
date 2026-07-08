using Base.CorePackage.Services;

namespace Base.SaveSystemPackage.Unity.Playtime
{
    /// <summary>
    /// Optional: anything that knows total play time can expose it here so the save
    /// button can stamp it into metadata. Keeps play-time tracking out of the save system.
    /// </summary>
    public interface IPlaytimeProvider : IGameService
    {
        double TotalSeconds { get; }
    }
}