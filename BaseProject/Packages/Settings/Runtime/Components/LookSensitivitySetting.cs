using Base.AttributePackage;
using Base.ServicePackage;
using Base.SettingsPackage.Controls;
using Base.UtilityPackage.Identification;
using UnityEngine;

namespace Base.SettingsPackage.Components
{
    /// <summary>
    /// Stores a normalized 0..1 look sensitivity and pushes the multiplier it maps to into
    /// <see cref="ControlSettings"/>. The setting stays normalized so the same slider works whatever
    /// range a project picks, and only the mapping moves when that range is tuned.
    /// </summary>
    public sealed class LookSensitivitySetting : FloatSettingComponent
    {
        private const float MaxBound = 10f;
        private const float MinBound = 0.05f;

        [Title("Look Sensitivity")]
        [Tooltip("Multiplier range the normalized setting maps onto. X is the slowest, Y the fastest.")]
        [MinMaxSlider(MinBound, MaxBound)]
        [SerializeField] private Vector2 sensitivityRange = new(0.2f, 3f);

        [Tooltip("Position in that range used before the player has ever changed the setting.")]
        [SerializeField] [Range(0f, 1f)] private float defaultNormalized = 0.5f;

        /// <inheritdoc/>
        public override PersistentKey Key => ControlSettingKeys.LookSensitivity;

        /// <inheritdoc/>
        protected override float DefaultValue => defaultNormalized;

        private ControlSettings _controlSettings;

#region Unity Callbacks
        // The service is a GameServiceBehaviour at -97 and this component sits at 0, so it is always
        // registered by now. TryGet reports a missing one itself, which is why Apply stays quiet.
        protected override void Awake()
        {
            base.Awake();

            ServiceLocator.TryGet(out _controlSettings);
        }
#endregion

        /// <inheritdoc/>
        protected override void Apply(float normalized)
        {
            if (_controlSettings == null)
                return;

            _controlSettings.SetLookSensitivity(Mathf.Lerp(sensitivityRange.x, sensitivityRange.y,
                Mathf.Clamp01(normalized)));
        }
    }
}