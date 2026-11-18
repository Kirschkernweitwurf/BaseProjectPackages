using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Base.TextPackage
{
    /// <summary>
    /// The language styled texts are prepared for: the selected one or the one an editor check measures. Readable
    /// fonts and hyphens depend on the language and outside play mode no language is selected, so a check that goes
    /// through every language names it here instead of switching the game's.
    /// </summary>
    public static class TextLocale
    {
        private static Locale _measured;

        /// <summary>The language a check measures or else the selected one, which is null outside play mode.</summary>
        public static Locale Current => _measured == null
            ? LocalizationSettings.SelectedLocale
            : _measured;

        /// <summary>Prepares every text for <paramref name="locale"/> until <see cref="EndMeasuring"/>.</summary>
        public static void BeginMeasuring(Locale locale) => _measured = locale;

        /// <summary>Back to the selected language.</summary>
        public static void EndMeasuring() => _measured = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => EndMeasuring();
    }
}