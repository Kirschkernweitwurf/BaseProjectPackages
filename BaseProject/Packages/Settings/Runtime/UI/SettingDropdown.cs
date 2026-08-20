using Base.AttributePackage;
using Base.SettingsPackage.Core;
using TMPro;
using UnityEngine;

namespace Base.SettingsPackage.UI
{
    /// <summary>
    /// Binds a <see cref="TMP_Dropdown"/> to an <see cref="IntSetting"/> holding the selected option index.
    /// </summary>
    [RequireComponent(typeof(TMP_Dropdown))]
    public sealed class SettingDropdown : TypedSettingElement<int, IntSetting>
    {
        [Title("Dropdown")]
        [SerializeField] [GetComponent] private TMP_Dropdown dropdown;

#region Unity Callbacks
        protected override void OnDestroy()
        {
            base.OnDestroy();

            dropdown.onValueChanged.RemoveListener(OnDropdownChanged);
        }
#endregion

        /// <inheritdoc/>
        protected override void OnBound()
        {
            OnSettingChanged(Setting.Value);

            dropdown.onValueChanged.AddListener(OnDropdownChanged);
        }

        /// <inheritdoc/>
        protected override void OnSettingChanged(int value)
        {
            if (dropdown.options.Count == 0)
                return;

            dropdown.SetValueWithoutNotify(Mathf.Clamp(value, 0, dropdown.options.Count - 1));
        }

        private void OnDropdownChanged(int value) => Setting.Value = value;
    }
}