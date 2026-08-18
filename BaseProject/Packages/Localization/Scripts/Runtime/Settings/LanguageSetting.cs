using Base.AttributePackage;
using Base.SettingsPackage.Components;
using Base.UtilityPackage.Identification;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Base.LocalizationPackage.Settings
{
    /// <summary>
    /// Stores an index into a curated list of <see cref="Locale"/> values and applies it through
    /// <see cref="LocalizationSettings.SelectedLocale"/>. Should be placed earlier in the scene than
    /// any component that reads localized strings during startup, so the active locale is set first.
    /// </summary>
    public sealed class LanguageSetting : IntSettingComponent
    {
        [Title("Language")]
        [Tooltip("Locales exposed to the player, in the order they appear in the menu.")]
        [SerializeField] [NotNullOrEmpty] private Locale[] availableLocales;

        [SerializeField] private int defaultIndex;

        /// <summary>The locale currently selected by the player.</summary>
        public Locale CurrentLocale
        {
            get
            {
                int index = TypedSetting == null
                    ? defaultIndex
                    : TypedSetting.Value;

                return availableLocales[ClampIndex(index)];
            }
        }

        /// <inheritdoc/>
        public override PersistentKey Key => new("Language");

        /// <inheritdoc/>
        protected override int DefaultValue => ClampIndex(defaultIndex);

        /// <inheritdoc/>
        protected override void Apply(int index)
            => LocalizationSettings.SelectedLocale = availableLocales[ClampIndex(index)];

        private int ClampIndex(int index) => Mathf.Clamp(index, 0, availableLocales.Length - 1);
    }
}