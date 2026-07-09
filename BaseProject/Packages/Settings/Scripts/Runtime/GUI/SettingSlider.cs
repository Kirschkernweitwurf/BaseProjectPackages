using Base.SettingsPackage.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Base.SettingsPackage.GUI
{
    /// <summary>
    /// Binds a <see cref="Slider"/> to a normalised <see cref="FloatSetting"/> (0..1). The slider's own range is
    /// used for display only; the stored value is always the slider value divided by its maximum. Any non-linear
    /// mapping, such as audio decibels, belongs in the setting's applier, not here.
    /// </summary>
    [RequireComponent(typeof(Slider))]
    public sealed class SettingSlider : SettingElement
    {
        [Header("Slider")]

        [SerializeField] private TMP_Text percentageText;
        [SerializeField] private Button decreaseButton;
        [SerializeField] private Button increaseButton;
        [SerializeField] private float buttonStep = 1f;

        private FloatSetting _setting;
        private Slider _slider;

#region Unity Callbacks
        private void Awake() => _slider = GetComponent<Slider>();

        private void OnDestroy()
        {
            if (_setting != null)
                _setting.OnValueChanged -= OnSettingChanged;

            _slider.onValueChanged.RemoveListener(OnSliderChanged);
            decreaseButton?.onClick.RemoveListener(Decrease);
            increaseButton?.onClick.RemoveListener(Increase);
        }
#endregion

        /// <inheritdoc/>
        protected override void Bind(SettingsRegistry registry)
        {
            if (!registry.TryGet(SettingKey, out _setting))
                return;

            ApplyToSlider(_setting.Value);

            _slider.onValueChanged.AddListener(OnSliderChanged);
            _setting.OnValueChanged += OnSettingChanged;
            decreaseButton?.onClick.AddListener(Decrease);
            increaseButton?.onClick.AddListener(Increase);
        }

        /// <inheritdoc/>
        protected override void ResetSetting() => _setting?.ResetToDefault();

        private void StepBy(float amount)
            => _slider.value = Mathf.Clamp(_slider.value + amount, _slider.minValue, _slider.maxValue);

        private void OnSliderChanged(float value)
        {
            UpdatePercentageText(value);
            _setting.Value = Mathf.Clamp01(value / _slider.maxValue);
        }

        private void OnSettingChanged(float normalised) => ApplyToSlider(normalised);

        private void ApplyToSlider(float normalised)
        {
            float value = normalised * _slider.maxValue;
            _slider.SetValueWithoutNotify(value);
            UpdatePercentageText(value);
        }

        private void UpdatePercentageText(float value)
        {
            if (percentageText != null)
                percentageText.text = Mathf.RoundToInt(value).ToString();
        }

        private void Decrease() => StepBy(-buttonStep);

        private void Increase() => StepBy(buttonStep);
    }
}