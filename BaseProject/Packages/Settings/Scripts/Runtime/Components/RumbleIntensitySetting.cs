using Base.AttributePackage;
using Base.ControllerSupport.Haptics;
using Base.CorePackage.Services;
using Base.ToolPackage.Identification;
using UnityEngine;

namespace Base.SettingsPackage.Components
{
    /// <summary>
    /// Stores the global rumble strength and pushes it into the <see cref="RumbleService"/>, where it
    /// scales every request. Pair it with <see cref="RumbleEnabledSetting"/> for a slider next to a toggle.
    /// </summary>
    public sealed class RumbleIntensitySetting : FloatSettingComponent
    {
        [Title("Rumble")]
        [Tooltip("Defaults asset. Use the same one the RumbleService references.")]
        [Required]
        [SerializeField] private RumbleConfig config;

        /// <inheritdoc/>
        public override PersistentKey Key => RumbleSettingKeys.Intensity;

        /// <inheritdoc/>
        protected override float DefaultValue => config.MainIntensity;

        private RumbleService _rumbleService;

#region Unity Callbacks
        // The service is a GameServiceBehaviour at -1 and this component sits at 0, so it is always
        // registered by now. TryGet reports a missing one itself, which is why Apply stays quiet.
        protected override void Awake()
        {
            base.Awake();

            ServiceLocator.TryGet(out _rumbleService);
        }
#endregion

        /// <inheritdoc/>
        protected override void Apply(float value)
        {
            if (_rumbleService == null)
                return;

            _rumbleService.SetMainIntensity(value);
        }
    }
}