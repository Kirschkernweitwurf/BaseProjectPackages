using Base.AttributePackage;
using Base.ServicePackage;
using Base.SettingsPackage.Controls;
using Base.UtilityPackage.Identification;
using UnityEngine;

namespace Base.SettingsPackage.Components
{
    /// <summary>
    /// Stores whether one look axis is flipped and pushes it into <see cref="ControlSettings"/>. Use one
    /// instance per axis; the axis picks the key, so the two never collide.
    /// </summary>
    public sealed class InvertLookSetting : BoolSettingComponent
    {
        [Title("Invert Look")]
        [Tooltip("Which axis this instance flips. Each axis gets its own key and its own toggle.")]
        [SerializeField] private ELookAxis axis = ELookAxis.Vertical;

        [Tooltip("Whether the axis is flipped before the player has ever changed the setting.")]
        [SerializeField] private bool defaultInverted;

        /// <inheritdoc/>
        public override PersistentKey Key => axis == ELookAxis.Horizontal
            ? ControlSettingKeys.InvertHorizontal
            : ControlSettingKeys.InvertVertical;

        /// <inheritdoc/>
        protected override bool DefaultValue => defaultInverted;

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
        protected override void Apply(bool isInverted)
        {
            if (_controlSettings == null)
                return;

            _controlSettings.SetInverted(axis, isInverted);
        }
    }
}