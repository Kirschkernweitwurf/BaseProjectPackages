using Base.SettingsPackage.Core;
using UnityEngine;

namespace Base.SettingsPackage.GUI
{
    /// <summary>
    /// Multiple choice element whose selected index is stored directly in an <see cref="IntSetting"/>.
    /// </summary>
    public sealed class IntMultipleChoiceElement : MultipleChoiceElement
    {
        private IntSetting _setting;

#region Unity Callbacks
        private void OnDestroy()
        {
            if (_setting != null)
                _setting.OnValueChanged -= OnSettingChanged;
        }
#endregion

        /// <inheritdoc/>
        protected override void Bind(SettingsRegistry registry)
        {
            if (!registry.TryGet(SettingKey, out _setting))
                return;

            CurrentIndex = ClampIndex(_setting.Value);
            _setting.OnValueChanged += OnSettingChanged;

            RefreshValueText();
            BuildIndicators();
        }

        /// <inheritdoc/>
        protected override void ResetSetting() => _setting?.ResetToDefault();

        /// <inheritdoc/>
        protected override void ApplySelection() => _setting.Value = CurrentIndex;

        private void OnSettingChanged(int value)
        {
            CurrentIndex = ClampIndex(value);
            RefreshValueText();
            RefreshIndicators();
        }

        private int ClampIndex(int value) => Mathf.Clamp(value, 0, Mathf.Max(0, options.Count - 1));
    }
}