using Base.SettingsPackage.Core;
using TMPro;
using UnityEngine;

namespace Base.SettingsPackage.GUI
{
    /// <summary>
    /// Binds a <see cref="TMP_Dropdown"/> to an <see cref="IntSetting"/> holding the selected option index.
    /// </summary>
    [RequireComponent(typeof(TMP_Dropdown))]
    public sealed class SettingDropdown : SettingElement
    {
        private TMP_Dropdown _dropdown;
        private IntSetting _setting;

#region Unity Callbacks
        private void Awake() => _dropdown = GetComponent<TMP_Dropdown>();

        private void OnDestroy()
        {
            if (_setting != null)
                _setting.OnValueChanged -= OnSettingChanged;

            _dropdown.onValueChanged.RemoveListener(OnDropdownChanged);
        }
#endregion

        /// <inheritdoc/>
        protected override void Bind(SettingsRegistry registry)
        {
            if (!registry.TryGet(SettingKey, out _setting))
                return;

            OnSettingChanged(_setting.Value);

            _dropdown.onValueChanged.AddListener(OnDropdownChanged);
            _setting.OnValueChanged += OnSettingChanged;
        }

        /// <inheritdoc/>
        protected override void ResetSetting() => _setting?.ResetToDefault();

        private void OnSettingChanged(int value)
        {
            if (_dropdown.options.Count == 0)
                return;

            _dropdown.SetValueWithoutNotify(Mathf.Clamp(value, 0, _dropdown.options.Count - 1));
        }

        private void OnDropdownChanged(int value) => _setting.Value = value;
    }
}