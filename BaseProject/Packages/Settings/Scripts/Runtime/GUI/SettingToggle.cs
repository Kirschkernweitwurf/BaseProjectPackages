using Base.AttributePackage;
using Base.SettingsPackage.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Base.SettingsPackage.GUI
{
    /// <summary>Binds a <see cref="Toggle"/> to a <see cref="BoolSetting"/>.</summary>
    [RequireComponent(typeof(Toggle))]
    public sealed class SettingToggle : SettingElement
    {
        [Header("Toggle")]

        [SerializeField] [Required] private TMP_Text stateText;
        [SerializeField] private LocalizedString onLabel;
        [SerializeField] private LocalizedString offLabel;

        private Toggle _toggle;
        private BoolSetting _setting;

#region Unity Callbacks
        private void Awake() => _toggle = GetComponent<Toggle>();

        private void OnDestroy()
        {
            if (_setting != null)
                _setting.OnValueChanged -= OnSettingChanged;

            _toggle.onValueChanged.RemoveListener(OnToggleChanged);
        }
#endregion

        /// <inheritdoc/>
        protected override void Bind(SettingsRegistry registry)
        {
            if (!registry.TryGet(SettingKey, out _setting))
                return;

            OnSettingChanged(_setting.Value);

            _toggle.onValueChanged.AddListener(OnToggleChanged);
            _setting.OnValueChanged += OnSettingChanged;
        }

        /// <inheritdoc/>
        protected override void ResetSetting() => _setting?.ResetToDefault();

        private void OnSettingChanged(bool state)
        {
            _toggle.SetIsOnWithoutNotify(state);
            stateText.text = state
                ? onLabel.GetLocalizedString()
                : offLabel.GetLocalizedString();
        }

        private void OnToggleChanged(bool state) => _setting.Value = state;
    }
}