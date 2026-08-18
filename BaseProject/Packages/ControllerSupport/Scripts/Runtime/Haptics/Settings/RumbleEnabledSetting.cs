using Base.AttributePackage;
using Base.ServicePackage;
using Base.SettingsPackage.Components;
using Base.UtilityPackage.Identification;
using UnityEngine;

namespace Base.ControllerSupportPackage.Haptics.Settings
{
    /// <summary>
    /// Stores whether gamepad rumble is allowed and pushes it into the <see cref="RumbleService"/>.
    /// Both the key and the default come from the controller package, so this component, the service and
    /// the config asset cannot drift apart.
    /// </summary>
    public sealed class RumbleEnabledSetting : BoolSettingComponent
    {
        [Title("Rumble")]
        [Tooltip("Defaults asset. Use the same one the RumbleService references.")]
        [Required]
        [SerializeField] private RumbleConfig config;

        /// <inheritdoc/>
        public override PersistentKey Key => RumbleSettingKeys.Enabled;

        /// <inheritdoc/>
        protected override bool DefaultValue => config.RumbleEnabled;

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
        protected override void Apply(bool value)
        {
            if (_rumbleService == null)
                return;

            _rumbleService.SetRumbleEnabled(value);
        }
    }
}