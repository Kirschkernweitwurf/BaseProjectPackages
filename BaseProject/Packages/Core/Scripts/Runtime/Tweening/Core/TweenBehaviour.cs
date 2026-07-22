using System;
using Base.CorePackage.Services.Shutdown;
using Base.CorePackage.Tweening.Core.Data;
using Base.CorePackage.Tweening.Core.Data.Profiles;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace Base.CorePackage.Tweening.Core
{
    /// <summary>
    /// Generic base class providing tween lifecycle control, loop behavior, default-value caching
    /// and optional asset driven configuration.
    /// </summary>
    /// <remarks>
    /// A tween reads its setup from the first source that is turned on:
    /// <list type="number">
    /// <item><description>a profile asset, which drives the values, the timing and the loop</description></item>
    /// <item><description>a settings asset, which drives the timing and the loop</description></item>
    /// <item><description>the fields on the component itself</description></item>
    /// </list>
    /// The inspector only shows the fields that are actually in use. Resolved settings are always a
    /// private copy, so <see cref="SetDuration"/> and friends never write into a shared asset.
    /// </remarks>
    public abstract class TweenBehaviour<T> : TweenBehaviourBase, IShutdownHandler
    {
        /// <inheritdoc/>
        public override event Action OnFinished;

        /// <inheritdoc/>
        public override event Action OnKilled;

        private static readonly Func<T, T, float, T> DefaultLerpFunc = TweenLerpUtility.Resolve<T>();

        [Tooltip("If true, a profile asset drives the values, the timing and the loop behavior.")]
        [SerializeField] private bool useProfile;

        [Tooltip("If true, a shared settings asset drives the timing and the loop behavior.")]
        [SerializeField] private bool useSettingsAsset;

        [Tooltip("The shared timing asset, used while the toggle above is on.")]
        [SerializeField] private TweenSettingsSo settingsAsset;

        [FormerlySerializedAs("<TweenSettings>k__BackingField")]
        [Tooltip("Duration, delay and easing of this tween.")]
        [SerializeField] private TweenSettings tweenSettings = new();

        [Tooltip("Loop behavior of this tween.")]
        [SerializeField] private LoopSettings loopSettings = new();

        public bool HasShutDown { get; private set; }

        public override TweenBase ActiveTween => _activeTween;

        /// <summary>
        /// The resolved timing of this tween. This is a private copy, so changing it never touches
        /// a shared asset.
        /// </summary>
        public TweenSettings TweenSettings => Application.isPlaying
            ? _resolvedSettings ??= ResolveSettings()
            : ResolveSettings();

        /// <summary>The resolved loop behavior of this tween. This is a private copy.</summary>
        public LoopSettings LoopSettings => Application.isPlaying
            ? _resolvedLoopSettings ??= ResolveLoopSettings()
            : ResolveLoopSettings();

        /// <summary>
        /// The profile driving this tween, or <c>null</c> while the profile toggle is off.
        /// </summary>
        protected TweenValueProfileSo<T> Profile => useProfile
            ? ProfileAsset
            : null;

        /// <summary>
        /// The profile field declared on the component. Every component declares its own typed
        /// field, so only profiles of a matching value type can be assigned.
        /// </summary>
        protected abstract TweenValueProfileSo<T> ProfileAsset { get; }

        /// <summary>The value the tween starts from.</summary>
        protected virtual T StartValue => Profile != null
            ? Profile.StartValue
            : LocalStartValue;

        /// <summary>The value the tween moves to.</summary>
        protected virtual T TargetValue => Profile != null
            ? Profile.TargetValue
            : LocalTargetValue;

        /// <summary>The start value authored on the component, used when no profile is assigned.</summary>
        protected virtual T LocalStartValue => DefaultValue;

        /// <summary>The target value authored on the component, used when no profile is assigned.</summary>
        protected virtual T LocalTargetValue => DefaultValue;

        /// <summary>The object whose destruction stops the tween. Defaults to this component.</summary>
        protected virtual Object TweenTarget => this;

        /// <summary>
        /// The interpolation function used by the default <see cref="CreateTween"/>. Override this
        /// for value types that <see cref="TweenLerpUtility.Resolve{T}"/> does not know.
        /// </summary>
        protected virtual Func<T, T, float, T> LerpFunc => DefaultLerpFunc;

        protected T DefaultValue;

        private TweenBase _activeTween;
        private TweenSettings _resolvedSettings;
        private LoopSettings _resolvedLoopSettings;
        private int _currentLoop;
        private bool _currentReversed;

#region Unity Callbacks
        protected virtual void Awake() => DefaultValue = GetCurrentValue();

        protected virtual void Start() => ShutdownManager.Register(this);

        protected virtual void OnDestroy()
        {
            if (!HasShutDown)
                Shutdown();
        }
#endregion

        public void Shutdown()
        {
            ShutdownManager.Deregister(this);

            Stop();
            HasShutDown = true;
        }

        public override void Play(bool isReversed)
        {
            Stop();

            _currentLoop = 0;
            _currentReversed = isReversed;

            TweenBase first = CreateTween(isReversed);
            StartTween(first);
        }

        public override void Stop(bool complete = false)
        {
            if (_activeTween == null)
                return;

            TweenBase tween = _activeTween;
            _activeTween = null;

            // Detach our handler so the tween's own Stop doesn't reroute back into loop logic.
            tween.OnComplete -= HandleTweenComplete;
            tween.Stop(complete);

            if (complete)
                OnFinished?.Invoke();

            OnKilled?.Invoke();
        }

        public override void ResetToDefault()
        {
            Stop();
            ApplyValue(DefaultValue);
        }

        /// <summary>
        /// Sets the delay of the tween to the specified value.
        /// </summary>
        public void SetDelay(float delay) => TweenSettings.SetDelay(delay);

        /// <summary>
        /// Sets the duration of the tween to the specified value.
        /// </summary>
        public void SetDuration(float duration) => TweenSettings.SetDuration(duration);

        /// <summary>
        /// Sets the easing of the tween to the specified value.
        /// </summary>
        public void SetEasing(EEasingType easing) => TweenSettings.SetEasing(easing);

        /// <summary>
        /// Swaps the shared timing asset at runtime and turns its toggle on. The timing is resolved
        /// again, which drops changes made through <see cref="SetDuration"/> and friends.
        /// </summary>
        /// <param name="newSettingsAsset">
        /// The asset to use, or <c>null</c> to fall back to the fields on this component.
        /// </param>
        public void SetSettingsAsset(TweenSettingsSo newSettingsAsset)
        {
            settingsAsset = newSettingsAsset;
            useSettingsAsset = newSettingsAsset != null;

            RefreshSettings();
        }

        /// <summary>
        /// Drops the resolved copies, so the next access reads from the assets again. Call this
        /// after changing an asset that this tween uses at runtime.
        /// </summary>
        public void RefreshSettings()
        {
            _resolvedSettings = null;
            _resolvedLoopSettings = null;
        }

        /// <summary>
        /// Returns the current value of the property being tweened.
        /// </summary>
        protected abstract T GetCurrentValue();

        /// <summary>
        /// Applies the given value to the property being tweened.
        /// </summary>
        /// <param name="value">The value to apply.</param>
        protected abstract void ApplyValue(T value);

        /// <summary>
        /// Creates the tween instance for one play. The default implementation covers every tween
        /// that moves from <see cref="StartValue"/> to <see cref="TargetValue"/>. Override it only
        /// for exotic setups.
        /// </summary>
        /// <param name="isReversed">If <c>true</c>, start and target are swapped.</param>
        protected virtual TweenBase CreateTween(bool isReversed)
        {
            Func<T, T, float, T> lerpFunc = LerpFunc;

            if (lerpFunc == null)
            {
                CustomLogger.LogError(
                    $"No interpolation function for '{typeof(T).Name}'. Override LerpFunc or CreateTween.", this);

                return null;
            }

            T start = StartValue;
            T target = TargetValue;

            T from = isReversed
                ? target
                : start;

            T to = isReversed
                ? start
                : target;

            TweenSettings settings = TweenSettings;

            return new Tween<T>(to,
                settings.Duration,
                ApplyValue,
                lerpFunc,
                Easings.Get(settings.Easing),
                TweenTarget,
                settings.Delay,
                fromGetter: () => from);
        }

        private TweenSettings ResolveSettings()
        {
            if (Profile != null)
                return Profile.Settings.Copy();

            if (useSettingsAsset && settingsAsset != null)
                return settingsAsset.Settings.Copy();

            return tweenSettings.Copy();
        }

        private LoopSettings ResolveLoopSettings()
        {
            if (Profile != null)
                return Profile.Loop.Copy();

            if (useSettingsAsset && settingsAsset != null)
                return settingsAsset.Loop.Copy();

            return loopSettings.Copy();
        }

        /// <summary>
        /// Centralized start for a tween instance (subscribes and starts).
        /// </summary>
        private void StartTween(TweenBase tween)
        {
            if (tween == null)
            {
                OnFinished?.Invoke();
                OnKilled?.Invoke();
                return;
            }

            _activeTween = tween;
            _activeTween.OnComplete += HandleTweenComplete;
            _activeTween.Start();
        }

        /// <summary>
        /// Called when the currently active tween instance completes naturally.
        /// This method handles loop logic and only fires OnFinished when the behavior is truly done.
        /// </summary>
        private void HandleTweenComplete(TweenBase completedTween)
        {
            completedTween.OnComplete -= HandleTweenComplete;
            _activeTween = null;

            LoopSettings loops = LoopSettings;
            bool hasLoopBudget = loops.LoopCount == -1 || _currentLoop < loops.LoopCount;

            if (loops.LoopType == ELoopType.None || !hasLoopBudget)
            {
                OnFinished?.Invoke();
                OnKilled?.Invoke();
                return;
            }

            _currentLoop++;

            switch (loops.LoopType)
            {
                case ELoopType.Restart:
                    ApplyValue(DefaultValue);
                    StartTween(CreateTween(_currentReversed));
                    break;

                case ELoopType.PingPong:
                    _currentReversed = !_currentReversed;
                    StartTween(CreateTween(_currentReversed));
                    break;

                case ELoopType.Continue:
                    StartTween(CreateTween(_currentReversed));
                    break;
            }
        }
    }
}