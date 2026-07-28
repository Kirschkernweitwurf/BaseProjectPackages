using Base.AttributePackage;
using Base.SettingsPackage.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Base.SettingsPackage.UI
{
    /// <summary>
    /// Binds a <see cref="Slider"/> to a normalized <see cref="FloatSetting"/> (0..1). The slider's own range is
    /// used for display only; the stored value is always the slider value divided by its maximum. Any non-linear
    /// mapping, such as audio decibels, belongs in the setting's applier, not here.
    /// </summary>
    [RequireComponent(typeof(Slider))]
    public sealed class SettingSlider : TypedSettingElement<float, FloatSetting>
    {
        // Sentinel that no label has been written yet, so the first value always reaches the text.
        private const int NoDisplayedValue = int.MinValue;

        [Header("Slider")]

        [SerializeField] [GetComponent] private Slider slider;
        [SerializeField] private TMP_Text percentageText;
        [SerializeField] private Button decreaseButton;
        [SerializeField] private Button increaseButton;
        [SerializeField] private float buttonStep = 1f;

        private int _displayedValue = NoDisplayedValue;
        private bool _isPushingValue;

#region Unity Callbacks
        protected override void OnDestroy()
        {
            base.OnDestroy();

            slider.onValueChanged.RemoveListener(OnSliderChanged);

            if (decreaseButton != null)
                decreaseButton.onClick.RemoveListener(Decrease);

            if (increaseButton != null)
                increaseButton.onClick.RemoveListener(Increase);
        }
#endregion

        /// <inheritdoc/>
        protected override void OnBound()
        {
            ApplyToSlider(Setting.Value);

            slider.onValueChanged.AddListener(OnSliderChanged);

            if (decreaseButton != null)
                decreaseButton.onClick.AddListener(Decrease);

            if (increaseButton != null)
                increaseButton.onClick.AddListener(Increase);
        }

        /// <inheritdoc/>
        protected override void OnSettingChanged(float normalized)
        {
            // Ignore the echo of the value this element just pushed into the setting.
            if (_isPushingValue)
                return;

            ApplyToSlider(normalized);
        }

        private void OnSliderChanged(float value)
        {
            UpdatePercentageText(value);

            _isPushingValue = true;
            Setting.Value = Mathf.Clamp01(value / slider.maxValue);
            _isPushingValue = false;
        }

        private void ApplyToSlider(float normalized)
        {
            float value = normalized * slider.maxValue;
            slider.SetValueWithoutNotify(value);
            UpdatePercentageText(value);
        }

        private void UpdatePercentageText(float value)
        {
            if (percentageText == null)
                return;

            int rounded = Mathf.RoundToInt(value);
            if (rounded == _displayedValue)
                return;

            _displayedValue = rounded;
            percentageText.text = rounded.ToString();
        }

        private void Decrease() => StepBy(-buttonStep);

        private void Increase() => StepBy(buttonStep);

        private void StepBy(float amount)
            => slider.value = Mathf.Clamp(slider.value + amount, slider.minValue, slider.maxValue);
    }
}