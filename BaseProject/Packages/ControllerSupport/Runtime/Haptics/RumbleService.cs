using System;
using System.Collections.Generic;
using Base.AttributePackage;
using Base.ServicePackage;
using Base.ServicePackage.Tracking;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Base.ControllerSupportPackage.Haptics
{
    /// <summary>
    /// Drives the gamepad motors from curve-based patterns. Requests stack in a
    /// <see cref="PriorityTracker{T}"/>, so a scripted hit outranks an ambient engine hum without either
    /// side knowing about the other. Only the highest priority request reaches the motors and ties go to
    /// the most recent one, but every request keeps running its own clock while outranked, so a preempted
    /// burst expires on schedule instead of firing late. Starts from the defaults in its
    /// <see cref="RumbleConfig"/>, which a settings component then overrides with the player's choice.
    /// </summary>
    public sealed class RumbleService : GameServiceBehaviour
    {
        private const float FullIntensity = 1f;
        private const float MotorsAtRest = 0f;

        /// <summary>Raised whenever rumble is switched on or off, so UI can follow the state.</summary>
        public event Action<bool> OnRumbleEnabledChanged;

        [Title("Rumble")]
        [Tooltip("Defaults this service starts from. Share the asset with the rumble setting components.")]
        [Required]
        [SerializeField] private RumbleConfig config;

        /// <summary>Resolves competing requests. The highest priority one owns the motors.</summary>
        public PriorityTracker<RumbleRequest> RumbleTracker { get; } = new();

        /// <summary>Whether rumble reaches the motors at all.</summary>
        [ShowNativeProperty]
        public bool IsRumbleEnabled { get; private set; }

        /// <summary>Global strength multiplier applied to every request.</summary>
        [ShowNativeProperty]
        public float MainIntensity { get; private set; }

        private readonly List<RumbleRequest> _finishedRequests = new();

        private Gamepad _drivenGamepad;

#region Unity Callbacks
        protected override void Awake()
        {
            base.Awake();

            IsRumbleEnabled = config.RumbleEnabled;
            MainIntensity = config.MainIntensity;
        }

        private void Update()
        {
            AdvanceRequests();
            ApplyCurrentRequest();
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (!isPaused)
                return;

            StopMotors();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
                return;

            StopMotors();
        }

        // A pad left buzzing after the service goes away is the one failure players actually notice.
        private void OnDisable() => StopMotors();

        protected override void OnDestroy()
        {
            base.OnDestroy();

            RumbleTracker.Clear();
            StopMotors();
        }
#endregion

        /// <summary>
        /// Plays a shared pattern asset on behalf of a caller. A second call from the same caller replaces
        /// the first, so retriggering restarts the pattern instead of stacking copies of it.
        /// </summary>
        /// <param name="pattern">The pattern asset to play.</param>
        /// <param name="caller">The object requesting the rumble, used as the key for stopping it.</param>
        /// <param name="priority">Rank against other live requests. Higher wins.</param>
        /// <param name="intensity">Strength multiplier for this request alone, 0 to 1.</param>
        public void Play(RumblePattern pattern, object caller, EPriority priority = EPriority.Medium,
            float intensity = FullIntensity)
        {
            if (pattern == null)
            {
                CustomLogger.LogWarning($"Tried to play a null {nameof(RumblePattern)}.", this);
                return;
            }

            Play(pattern.Pattern, caller, priority, intensity);
        }

        /// <summary>
        /// Plays pattern data on behalf of a caller. Use this for a pattern authored inline on a component
        /// or built in code. A second call from the same caller replaces the first.
        /// </summary>
        /// <param name="pattern">The curves and timing to play.</param>
        /// <param name="caller">The object requesting the rumble, used as the key for stopping it.</param>
        /// <param name="priority">Rank against other live requests. Higher wins.</param>
        /// <param name="intensity">Strength multiplier for this request alone, 0 to 1.</param>
        public void Play(RumblePatternData pattern, object caller, EPriority priority = EPriority.Medium,
            float intensity = FullIntensity)
        {
            if (pattern == null)
            {
                CustomLogger.LogWarning($"Tried to play a null {nameof(RumblePatternData)}.", this);
                return;
            }

            if (caller == null)
            {
                CustomLogger.LogWarning("Tried to play a rumble pattern without a caller.", this);
                return;
            }

            // The tracker holds one entry per caller, so a retrigger has to clear the old entry first.
            if (RumbleTracker.HasCaller(caller))
                RumbleTracker.Remove(caller);

            RumbleTracker.Add(new RumbleRequest(pattern, caller, intensity), (uint)priority, caller);
        }

        /// <summary>Plays a flat burst at a fixed strength, without needing an authored pattern.</summary>
        /// <param name="low">Low frequency motor strength, 0 to 1.</param>
        /// <param name="high">High frequency motor strength, 0 to 1.</param>
        /// <param name="duration">How long the burst lasts, in seconds.</param>
        /// <param name="caller">The object requesting the rumble, used as the key for stopping it.</param>
        /// <param name="priority">Rank against other live requests. Higher wins.</param>
        public void PlayBurst(float low, float high, float duration, object caller,
            EPriority priority = EPriority.Medium)
            => Play(RumblePatternData.Constant(low, high, duration), caller, priority);

        /// <summary>
        /// Stops the request belonging to a caller. Stopping something that already finished is a normal
        /// state rather than a mistake, so an unknown caller is ignored silently.
        /// </summary>
        /// <param name="caller">The object that started the rumble.</param>
        public void Stop(object caller)
        {
            if (!RumbleTracker.HasCaller(caller))
                return;

            RumbleTracker.Remove(caller);
        }

        /// <summary>Drops every request and puts the motors back to rest immediately.</summary>
        public void StopAll()
        {
            RumbleTracker.Clear();
            StopMotors();
        }

        /// <summary>
        /// Switches rumble on or off. Live requests keep running their clocks while it is off, so turning
        /// it back on does not replay a burst that has long since expired.
        /// </summary>
        /// <param name="isEnabled">Whether rumble reaches the motors.</param>
        public void SetRumbleEnabled(bool isEnabled)
        {
            if (IsRumbleEnabled == isEnabled)
                return;

            IsRumbleEnabled = isEnabled;

            if (!isEnabled)
                StopMotors();

            OnRumbleEnabledChanged?.Invoke(isEnabled);
        }

        /// <summary>Sets the global strength multiplier applied to every request.</summary>
        /// <param name="intensity">The multiplier, clamped to 0 to 1.</param>
        public void SetMainIntensity(float intensity) => MainIntensity = Mathf.Clamp01(intensity);

        private void AdvanceRequests()
        {
            IReadOnlyList<TrackedItem<RumbleRequest>> tracked = RumbleTracker.TrackedItems;

            if (tracked.Count == 0)
                return;

            // Outranked requests advance too, so a preempted burst expires on schedule instead of
            // waiting in the stack and firing once the pattern above it ends.
            foreach (TrackedItem<RumbleRequest> item in tracked)
            {
                item.Item.Advance();

                if (item.Item.IsFinished)
                    _finishedRequests.Add(item.Item);
            }

            foreach (RumbleRequest finished in _finishedRequests)
                RumbleTracker.Remove(finished.Caller);

            _finishedRequests.Clear();
        }

        private void ApplyCurrentRequest()
        {
            if (!IsRumbleEnabled)
            {
                StopMotors();
                return;
            }

            TrackedItem<RumbleRequest> current = RumbleTracker.CurrentTrackedItem;

            if (current == null)
            {
                StopMotors();
                return;
            }

            current.Item.Sample(out float low, out float high);
            ApplyMotorSpeeds(low * MainIntensity, high * MainIntensity);
        }

        private void ApplyMotorSpeeds(float low, float high)
        {
            Gamepad gamepad = Gamepad.current;

            // The pad was unplugged mid-pattern. Its motors died with it, so there is nothing to reset.
            if (gamepad == null)
            {
                _drivenGamepad = null;
                return;
            }

            // A pad swapped mid-pattern would otherwise keep buzzing on the speeds it was left with.
            if (_drivenGamepad != null
                && _drivenGamepad != gamepad)
                _drivenGamepad.SetMotorSpeeds(MotorsAtRest, MotorsAtRest);

            _drivenGamepad = gamepad;
            gamepad.SetMotorSpeeds(Mathf.Clamp01(low), Mathf.Clamp01(high));
        }

        // A null driven pad means the motors are already at rest, which keeps this off the hot path.
        private void StopMotors()
        {
            if (_drivenGamepad == null)
                return;

            _drivenGamepad.SetMotorSpeeds(MotorsAtRest, MotorsAtRest);
            _drivenGamepad = null;
        }
    }
}