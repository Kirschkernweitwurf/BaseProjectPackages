using System.Collections.Generic;
using Base.SettingsPackage.Core;

namespace Base.SettingsPackage.GUI
{
    /// <summary>
    /// Multiple choice element whose selected option label is stored in a <see cref="StringSetting"/>.
    /// Subclasses may override <see cref="ResolveOptions"/> to supply options at runtime.
    /// </summary>
    public class StringMultipleChoiceElement : MultipleChoiceElement
    {
        private StringSetting _setting;

        private void OnDestroy()
        {
            if (_setting != null)
                _setting.OnValueChanged -= OnSettingChanged;
        }

        /// <inheritdoc/>
        protected override void Bind(SettingsRegistry registry)
        {
            if (!registry.TryGet(SettingKey, out _setting))
                return;

            options = ResolveOptions();
            CurrentIndex = IndexOf(_setting.Value);
            _setting.OnValueChanged += OnSettingChanged;

            RefreshValueText();
            BuildIndicators();
        }

        /// <inheritdoc/>
        protected override void ResetSetting() => _setting?.ResetToDefault();

        /// <inheritdoc/>
        protected override void ApplySelection() => _setting.Value = options[CurrentIndex];

        /// <summary>Supplies the selectable options. Defaults to the serialized list.</summary>
        protected virtual List<string> ResolveOptions() => options;

        private void OnSettingChanged(string value)
        {
            CurrentIndex = IndexOf(value);
            RefreshValueText();
            RefreshIndicators();
        }

        private int IndexOf(string value)
        {
            int index = options.IndexOf(value);
            return index < 0 ? 0 : index;
        }
    }
}