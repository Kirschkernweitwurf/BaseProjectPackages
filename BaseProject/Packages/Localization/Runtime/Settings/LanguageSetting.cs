using System;
using Base.AttributesPackage;
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
    /// A locale picked elsewhere, such as the Game view language menu in the editor, is written back into the
    /// setting, so the menu shows the language that is really active.
    /// </summary>
    public sealed class LanguageSetting : IntSettingComponent
    {
        /// <summary>The serialized name of the locale list, for tests that fill one in.</summary>
        internal const string AvailableLocalesField = nameof(availableLocales);

        /// <summary>The serialized name of the starting index, for tests that set one.</summary>
        internal const string DefaultIndexField = nameof(defaultIndex);

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

#region Unity Callbacks
        protected override void Awake()
        {
            base.Awake();

            LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
        }
#endregion

        /// <inheritdoc/>
        protected override void Apply(int index)
            => LocalizationSettings.SelectedLocale = availableLocales[ClampIndex(index)];

        private int ClampIndex(int index) => Mathf.Clamp(index, 0, availableLocales.Length - 1);

        private void OnSelectedLocaleChanged(Locale locale)
        {
            // Null while the settings context was missing in Awake.
            if (TypedSetting == null)
                return;

            // A locale outside the list, like a pseudo locale, has no menu entry, so the stored choice stays.
            int index = Array.IndexOf(availableLocales, locale);

            if (index < 0)
                return;

            TypedSetting.Value = index;
        }
    }
}