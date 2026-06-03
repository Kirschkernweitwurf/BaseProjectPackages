using Base.UtilityPackage.Identification;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Base.SettingsPackage.Components
{
    /// <summary>
    /// Stores an index into a curated list of <see cref="Locale"/> values and applies it through
    /// <see cref="LocalizationSettings.SelectedLocale"/>. Should be placed earlier in the scene than
    /// any component that reads localized strings during startup, so the active locale is set first.
    /// </summary>
    public sealed class LanguageSetting : IntSettingComponent
    {
        [Header("Language")]
        [Tooltip("Locales exposed to the player, in the order they appear in the menu.")]
        [SerializeField] private Locale[] availableLocales;

        [SerializeField] private int defaultIndex;

        /// <summary>The locale currently selected by the player.</summary>
        public Locale CurrentLocale => availableLocales[Mathf.Clamp(TypedSetting?
            .Value ?? defaultIndex, 0, availableLocales.Length - 1)];

        /// <inheritdoc/>
        public override PersistentKey Key => new("Language");

        /// <inheritdoc/>
        protected override int DefaultValue => Mathf.Clamp(defaultIndex, 0, availableLocales.Length - 1);

        /// <inheritdoc/>
        protected override void Apply(int index)
        {
            int safeIndex = Mathf.Clamp(index, 0, availableLocales.Length - 1);
            LocalizationSettings.SelectedLocale = availableLocales[safeIndex];
        }
    }
}