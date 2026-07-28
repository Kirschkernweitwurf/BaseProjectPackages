using Base.AttributePackage;
using Base.SettingsPackage.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Base.SettingsPackage.UI
{
    /// <summary>Binds a <see cref="Toggle"/> to a <see cref="BoolSetting"/>.</summary>
    [RequireComponent(typeof(Toggle))]
    public sealed class SettingToggle : TypedSettingElement<bool, BoolSetting>
    {
        [Header("Toggle")]

        [SerializeField] [GetComponent] private Toggle toggle;
        [SerializeField] [Required] private TMP_Text stateText;
        [SerializeField] private LocalizedString onLabel;
        [SerializeField] private LocalizedString offLabel;

#region Unity Callbacks
        protected override void OnDestroy()
        {
            base.OnDestroy();

            toggle.onValueChanged.RemoveListener(OnToggleChanged);
        }
#endregion

        /// <inheritdoc/>
        protected override void OnBound()
        {
            OnSettingChanged(Setting.Value);

            toggle.onValueChanged.AddListener(OnToggleChanged);
        }

        /// <inheritdoc/>
        protected override void OnSettingChanged(bool state)
        {
            toggle.SetIsOnWithoutNotify(state);
            stateText.text = state
                ? onLabel.GetLocalizedString()
                : offLabel.GetLocalizedString();
        }

        private void OnToggleChanged(bool state) => Setting.Value = state;
    }
}