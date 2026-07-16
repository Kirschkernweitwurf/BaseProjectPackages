using UnityEngine;

namespace Base.CorePackage.Tweening.Core.Data.Profiles
{
    /// <summary>
    /// Base class for tween profile assets. A profile bundles the values, the timing and the loop
    /// behavior of a tween, so that many components can share one authored setup.
    /// </summary>
    /// <remarks>
    /// Turn on the settings asset toggle to share the timing with other profiles. While it is on,
    /// the timing fields below are hidden and unused.
    /// </remarks>
    public abstract class TweenProfileSo : ScriptableObject
    {
        [Tooltip("If true, a shared settings asset drives the timing and the loop behavior.")]
        [SerializeField] private bool useSettingsAsset;

        [Tooltip("The shared timing asset, used while the toggle above is on.")]
        [SerializeField] private TweenSettingsSo settingsAsset;

        [Tooltip("Duration, delay and easing of this profile.")]
        [SerializeField] private TweenSettings tweenSettings = new();

        [Tooltip("Loop behavior of this profile.")]
        [SerializeField] private LoopSettings loopSettings = new();

        /// <summary>
        /// The timing this profile provides. This is the authored asset data, so treat it as read only
        /// and call <see cref="TweenSettings.Copy"/> before changing anything.
        /// </summary>
        public TweenSettings Settings => useSettingsAsset && settingsAsset != null
            ? settingsAsset.Settings
            : tweenSettings;

        /// <summary>
        /// The loop behavior this profile provides. This is the authored asset data, so treat it as
        /// read only and call <see cref="LoopSettings.Copy"/> before changing anything.
        /// </summary>
        public LoopSettings Loop => useSettingsAsset && settingsAsset != null
            ? settingsAsset.Loop
            : loopSettings;
    }
}