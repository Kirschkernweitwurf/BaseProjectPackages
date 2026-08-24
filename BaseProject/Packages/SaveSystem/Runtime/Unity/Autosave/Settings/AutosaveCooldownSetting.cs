using Base.AttributePackage;
using Base.ServicePackage;
using Base.SettingsPackage.Components;
using Base.UtilityPackage.Identification;
using UnityEngine;

namespace Base.SaveSystemPackage.Unity.Autosave.Settings
{
    /// <summary>
    /// Stores the shortest gap allowed between two autosaves and pushes it into the
    /// <see cref="AutosaveService"/>. Optional next to <see cref="AutosaveIntervalSetting"/>: most
    /// projects keep the cooldown a developer decision and only expose the interval.
    /// </summary>
    public sealed class AutosaveCooldownSetting : FloatSettingComponent
    {
        [Title("Autosave")]
        [Tooltip("Defaults asset. Use the same one the AutosaveService references.")]
        [Required]
        [SerializeField] private AutosaveConfig config;

        /// <inheritdoc/>
        public override PersistentKey Key => AutosaveSettingKeys.Cooldown;

        /// <inheritdoc/>
        protected override float DefaultValue => config.CooldownSeconds;

        private AutosaveService _autosaveService;

#region Unity Callbacks
        // The service is a GameServiceBehaviour at -1 and this component sits at 0, so it is always
        // registered by now. TryGet reports a missing one itself, which is why Apply stays quiet.
        protected override void Awake()
        {
            base.Awake();

            ServiceLocator.TryGet(out _autosaveService);
        }
#endregion

        /// <inheritdoc/>
        protected override void Apply(float value)
        {
            if (_autosaveService == null)
                return;

            _autosaveService.SetCooldownSeconds(value);
        }
    }
}