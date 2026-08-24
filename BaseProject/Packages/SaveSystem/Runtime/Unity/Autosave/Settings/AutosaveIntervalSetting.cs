using Base.AttributePackage;
using Base.ServicePackage;
using Base.SettingsPackage.Components;
using Base.UtilityPackage.Identification;
using UnityEngine;

namespace Base.SaveSystemPackage.Unity.Autosave.Settings
{
    /// <summary>
    /// Stores how often a timed autosave is offered and pushes it into the
    /// <see cref="AutosaveService"/>. Pair it with <see cref="AutosaveEnabledSetting"/> for a slider
    /// next to a toggle. The value is in seconds; a menu that shows minutes converts on display.
    /// </summary>
    public sealed class AutosaveIntervalSetting : FloatSettingComponent
    {
        [Title("Autosave")]
        [Tooltip("Defaults asset. Use the same one the AutosaveService references.")]
        [Required]
        [SerializeField] private AutosaveConfig config;

        /// <inheritdoc/>
        public override PersistentKey Key => AutosaveSettingKeys.Interval;

        /// <inheritdoc/>
        protected override float DefaultValue => config.IntervalSeconds;

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

            _autosaveService.SetIntervalSeconds(value);
        }
    }
}