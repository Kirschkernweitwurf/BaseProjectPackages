using Base.CorePackage.Services;
using UnityEngine;

namespace Base.SaveSystemPackage.Unity.Playtime
{
    /// <summary>
    /// Tracks total play time correctly across sessions.
    /// <example>
    /// <code>
    ///   var meta = await saves.LoadMetadataAsync(slot);
    ///   _playtime = new PlaytimeTracker(meta?.totalPlaySeconds ?? 0);
    ///   _playtime.Start();
    ///   ...
    ///   await saves.SaveAsync(slot, data, screenshot, _playtime.TotalSeconds);
    /// </code>
    /// </example>
    /// </summary>
    public sealed class PlaytimeTracker : MonoBehaviour, IPlaytimeProvider
    {
        /// <summary>
        /// Total play time so far, including the current running session.
        /// </summary>
        public double TotalSeconds => _running
            ? _accumulated + (Time.realtimeSinceStartup - _sessionStart)
            : _accumulated;

        private double _accumulated;
        private float _sessionStart;
        private bool _running;

        public PlaytimeTracker(double previousTotalSeconds = 0) => _accumulated = previousTotalSeconds;

#region Unity Callbacks
        private void Awake() => ServiceLocator.Register<IPlaytimeProvider>(this);

        public void Start()
        {
            _sessionStart = Time.realtimeSinceStartup;
            _running = true;
        }

        private void OnDestroy() => ServiceLocator.Deregister<IPlaytimeProvider>();
#endregion

        /// <summary>
        /// Call when the game is paused or the player returns to a menu.
        /// </summary>
        public void Pause()
        {
            if (!_running)
                return;

            _accumulated += Time.realtimeSinceStartup - _sessionStart;
            _running = false;
        }
    }
}