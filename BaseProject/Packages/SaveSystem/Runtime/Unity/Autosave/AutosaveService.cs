using System;
using System.Threading;
using Base.AttributesPackage;
using Base.SaveSystemPackage.Core;
using Base.SaveSystemPackage.Model;
using Base.SaveSystemPackage.Slots;
using Base.SaveSystemPackage.Unity.Composition;
using Base.ServicesPackage;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.SaveSystemPackage.Unity.Autosave
{
    /// <summary>
    /// Saves on a timer and on request, never more often than its cooldown allows. Drop it into the
    /// scene next to the <see cref="SaveManager"/> and gameplay code only ever has to say that it
    /// reached a checkpoint.
    /// <para>
    /// The interval decides how often a save is offered, the cooldown decides how often one is
    /// allowed. Both matter: a timer alone cannot stop a run of checkpoints from writing five saves in
    /// as many seconds, and a cooldown alone never saves at all in a quiet stretch of play.
    /// </para>
    /// <para>
    /// Starts from the defaults in its <see cref="AutosaveConfig"/>, which the autosave setting
    /// components then override with the player's choice.
    /// </para>
    /// </summary>
    public sealed class AutosaveService : GameServiceBehaviour
    {
        /// <summary>Raised after an autosave was written. Carries the slot it went to.</summary>
        public event Action<string> Saved;

        /// <summary>Raised when an autosave was attempted but did not finish.</summary>
        public event Action<string> Failed;

        [Title("Autosave")]
        [Tooltip("Defaults this service starts from. Share the asset with the autosave setting components.")]
        [Required]
        [SerializeField] private AutosaveConfig config;

        /// <summary>Whether the timer and any request write anything at all.</summary>
        [ShowNativeProperty]
        public bool IsAutosaveEnabled { get; private set; }

        /// <summary>Seconds between timed autosaves. Zero leaves only requests.</summary>
        [ShowNativeProperty]
        public float IntervalSeconds { get; private set; }

        /// <summary>Shortest gap between two autosaves.</summary>
        [ShowNativeProperty]
        public float CooldownSeconds { get; private set; }

        /// <summary>True while autosaving is paused by <see cref="Suspend"/>.</summary>
        public bool IsSuspended { get; private set; }

        /// <summary>True while a save is in flight.</summary>
        public bool IsSaving { get; private set; }

        /// <summary>Seconds until the next timed autosave, or zero when the timer is off.</summary>
        public float SecondsUntilNextSave => HasTimer
            ? Mathf.Max(0f, _nextSaveTime - Time.unscaledTime)
            : 0f;

        private bool HasTimer => IntervalSeconds > 0f;

        private ISaveSystem _saves;
        private SaveSlotSelection _selection;
        private CancellationTokenSource _cts;
        private float _nextSaveTime;
        private float _cooldownEndTime;
        private bool _pending;
        private bool _resolved;
        private bool _unavailable;

#region Unity Callbacks
        protected override void Awake()
        {
            base.Awake();

            _cts = new CancellationTokenSource();

            IsAutosaveEnabled = config.AutosaveEnabled;
            IntervalSeconds = config.IntervalSeconds;
            CooldownSeconds = config.CooldownSeconds;
        }

        private void Start() => RestartTimer();

        private void Update()
        {
            if (!IsAutosaveEnabled || IsSuspended || IsSaving)
                return;

            if (HasTimer && Time.unscaledTime >= _nextSaveTime)
            {
                _pending = true;
                RestartTimer();
            }

            // The request is only consumed once it can actually be acted on, so one made while no
            // slot is selected waits for one instead of disappearing.
            if (!_pending
                || Time.unscaledTime < _cooldownEndTime
                || !TryResolveServices()
                || !TryResolveSlotId(out string slotId))
                return;

            _pending = false;

            _ = SaveAsync(slotId);
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (isPaused)
                SaveOnInterruption();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
                SaveOnInterruption();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _cts?.Cancel();
            _cts?.Dispose();
        }
#endregion

        /// <summary>
        /// Asks for an autosave, for example at a checkpoint. A request is remembered rather than
        /// dropped: it runs as soon as the cooldown has passed and, where the target is the selected
        /// slot, as soon as there is one.
        /// </summary>
        public void Request() => _pending = true;

        /// <summary>
        /// Saves right away, ignoring the timer and the cooldown, and restarts both. Still does
        /// nothing while autosaving is switched off, since this is an autosave either way.
        /// </summary>
        /// <returns>True when a save was written.</returns>
        public async Awaitable<bool> SaveNowAsync()
        {
            if (!IsAutosaveEnabled
                || !TryResolveServices()
                || !TryResolveSlotId(out string slotId))
                return false;

            return await SaveAsync(slotId);
        }

        /// <summary>
        /// Switches autosaving on or off. This is the player's choice, kept apart from
        /// <see cref="Suspend"/> so that leaving a cutscene cannot turn it back on behind their back.
        /// </summary>
        /// <param name="isEnabled">Whether autosaving may write anything.</param>
        public void SetAutosaveEnabled(bool isEnabled)
        {
            if (IsAutosaveEnabled == isEnabled)
                return;

            IsAutosaveEnabled = isEnabled;

            // A deadline left over from before it was switched off would otherwise fire the moment
            // it comes back on.
            if (isEnabled)
                RestartTimer();
        }

        /// <summary>Sets how often a timed autosave is offered and starts the interval over.</summary>
        /// <param name="seconds">Seconds between saves, clamped at zero. Zero leaves only requests.</param>
        public void SetIntervalSeconds(float seconds)
        {
            IntervalSeconds = Mathf.Max(0f, seconds);

            RestartTimer();
        }

        /// <summary>Sets the shortest gap allowed between two autosaves.</summary>
        /// <param name="seconds">The gap in seconds, clamped at zero.</param>
        public void SetCooldownSeconds(float seconds) => CooldownSeconds = Mathf.Max(0f, seconds);

        /// <summary>Stops autosaving until <see cref="Resume"/>, for example during a cutscene.</summary>
        public void Suspend() => IsSuspended = true;

        /// <summary>Resumes autosaving and starts the interval over.</summary>
        public void Resume()
        {
            if (!IsSuspended)
                return;

            IsSuspended = false;

            RestartTimer();
        }

        private void RestartTimer() => _nextSaveTime = Time.unscaledTime + IntervalSeconds;

        private void SaveOnInterruption()
        {
            if (!config.SaveOnFocusLoss || IsSuspended)
                return;

            _ = SaveNowAsync();
        }

        private async Awaitable<bool> SaveAsync(string slotId)
        {
            if (IsSaving)
                return false;

            IsSaving = true;

            // Counted from the start of the write rather than its end, so the gap between two runs is
            // the configured one no matter how long a save takes.
            _cooldownEndTime = Time.unscaledTime + CooldownSeconds;
            RestartTimer();

            try
            {
                SaveRequest request = await SaveRequestFactory.CreateAsync(slotId, config.DisplayName,
                    config.CaptureScreenshot);

                await _saves.SaveAsync(request, _cts.Token);

                Saved?.Invoke(slotId);

                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception exception)
            {
                CustomLogger.LogError($"Autosave to slot '{slotId}' failed: {exception.Message}", this);
                Failed?.Invoke(slotId);

                return false;
            }
            finally
            {
                IsSaving = false;
            }
        }

        private bool TryResolveSlotId(out string slotId)
        {
            if (config.Target == EAutosaveTarget.DedicatedSlot)
            {
                slotId = config.DedicatedSlotId;

                return !string.IsNullOrWhiteSpace(slotId);
            }

            // Deliberately not routed through the slot provider: the appending and named models mint a
            // fresh slot when nothing is selected, which would turn every autosave into a new save.
            slotId = _selection.SelectedSlotId;

            return !string.IsNullOrEmpty(slotId);
        }

        private bool TryResolveServices()
        {
            if (_resolved)
                return true;

            if (_unavailable)
                return false;

            // TryGet logs on its own. Going dead after one failure keeps a missing manager from
            // filling the console once per interval for the rest of the session.
            if (!ServiceLocator.TryGet(out SaveManager manager))
                return GoDead();

            _saves = manager.SaveSystem;
            _selection = manager.Selection;

            if (_saves == null || _selection == null)
                return GoDead();

            _resolved = true;

            return true;
        }

        private bool GoDead()
        {
            _unavailable = true;
            enabled = false;

            return false;
        }
    }
}