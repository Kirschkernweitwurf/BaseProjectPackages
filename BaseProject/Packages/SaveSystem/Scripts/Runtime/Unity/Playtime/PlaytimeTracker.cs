using Base.CorePackage.Services;
using UnityEngine;

namespace Base.SaveSystemPackage.Unity.Playtime
{
    /// <summary>
    /// Tracks total play time correctly across sessions.
    /// <example>
    /// <code>
    ///   SaveMetadata meta = await saves.LoadMetadataAsync(slotId);
    ///   playtimeTracker.Initialize(meta?.TotalPlayTime.TotalSeconds ?? 0);
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

#region Unity Callbacks
        private void Awake() => ServiceLocator.Register<IPlaytimeProvider>(this);

        private void Start() => Resume();

        private void OnDestroy() => ServiceLocator.Deregister<IPlaytimeProvider>();
#endregion

        /// <summary>
        /// Seeds the tracker with the play time from a loaded save. Call after loading a slot.
        /// </summary>
        public void Initialize(double previousTotalSeconds)
            => _accumulated = previousTotalSeconds;

        /// <summary>
        /// Call when gameplay resumes after a pause. Started automatically on scene start.
        /// </summary>
        public void Resume()
        {
            if (_running)
                return;

            _sessionStart = Time.realtimeSinceStartup;
            _running = true;
        }

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
