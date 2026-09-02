using Base.AttributesPackage;
using Base.SaveSystemPackage.Unity.Autosave;
using Base.ServicesPackage;
using Base.SettingsPackage.Components;
using Base.UtilityPackage.Identification;
using UnityEngine;

namespace Base.SaveSystemPackage.Settings
{
    /// <summary>
    /// Stores whether autosaving is allowed and pushes it into the <see cref="AutosaveService"/>. Both
    /// the key and the default come from the save package, so this component, the service and the
    /// config asset cannot drift apart.
    /// </summary>
    public sealed class AutosaveEnabledSetting : BoolSettingComponent
    {
        [Title("Autosave")]
        [Tooltip("Defaults asset. Use the same one the AutosaveService references.")]
        [Required]
        [SerializeField] private AutosaveConfig config;

        /// <inheritdoc/>
        public override PersistentKey Key => AutosaveSettingKeys.Enabled;

        /// <inheritdoc/>
        protected override bool DefaultValue => config.AutosaveEnabled;

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
        protected override void Apply(bool value)
        {
            if (_autosaveService == null)
                return;

            _autosaveService.SetAutosaveEnabled(value);
        }
    }
}