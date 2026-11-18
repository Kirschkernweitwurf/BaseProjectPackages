using System.Collections.Generic;
using UnityEditor.Localization;
using UnityEngine.Localization;
using UnityEngine.Localization.Pseudo;
using UnityEngine.Localization.Tables;

namespace Base.TextPackage.Editor
{
    /// <summary>
    /// The languages a text fit check runs in and what a source needs to fill a pseudo-locale: it renders the
    /// language the pseudo-locale is based on, then stretches the result. English is every language's fallback, so a
    /// text without a translation yet is measured in English: a missing translation is no problem, a missing English
    /// text is.
    /// </summary>
    public static class TextFitLocales
    {
        /// <summary>The code reports show for the pseudo-locale.</summary>
        public const string PseudoCode = "pseudo";

        private const string FallbackCode = "en";
        private const string PseudoName = "Pseudo-language";

        private static readonly string[] UnspacedLanguages = { "ja", "zh" };

        /// <summary>Every language the game ships, then the pseudo-locales that stress English.</summary>
        public static List<Locale> All()
        {
            List<Locale> locales = new(LocalizationEditorSettings.GetLocales());
            locales.AddRange(LocalizationEditorSettings.GetPseudoLocales());

            return locales;
        }

        /// <summary>The code a report shows for <paramref name="locale"/>, "pseudo" for the pseudo-locale.</summary>
        public static string CodeOf(Locale locale) => locale is PseudoLocale
            ? PseudoCode
            : locale.Identifier.Code;

        /// <summary>The readable name of the language behind <paramref name="code"/>, like "German (de)".</summary>
        public static string NameOf(string code)
        {
            if (code == PseudoCode)
                return PseudoName;

            foreach (Locale locale in LocalizationEditorSettings.GetLocales())
            {
                if (locale.Identifier.Code == code)
                    return locale.LocaleName;
            }

            return code;
        }

        /// <summary>
        /// The language whose texts <paramref name="locale"/> shows: itself or for a pseudo-locale the real
        /// language it is based on.
        /// </summary>
        public static Locale SourceOf(Locale locale)
        {
            if (locale is not PseudoLocale pseudo)
                return locale;

            foreach (Locale real in LocalizationEditorSettings.GetLocales())
            {
                if (real is not PseudoLocale
                    && real.Identifier.Code == pseudo.Identifier.Code)
                    return real;
            }

            return locale;
        }

        /// <summary>
        /// The entry of <paramref name="reference"/> that <paramref name="locale"/> shows: its own or the English one
        /// while it has no translation yet. Null when English has none either.
        /// </summary>
        public static StringTableEntry FindShownEntry(StringTableCollection collection,
            TableEntryReference reference, Locale locale)
        {
            if (collection == null
                || locale == null)
                return null;

            StringTableEntry own = FindEntry(collection, reference, locale.Identifier);
            if (own != null
                && !string.IsNullOrEmpty(own.Value))
                return own;

            StringTableEntry fallback = FindEntry(collection, reference, new LocaleIdentifier(FallbackCode));

            return fallback != null
                && !string.IsNullOrEmpty(fallback.Value)
                    ? fallback
                    : null;
        }

        /// <summary>
        /// The text of <paramref name="text"/> that <paramref name="locale"/> shows, see <see cref="FindShownEntry"/>.
        /// Empty when English has none either.
        /// </summary>
        public static string ShownText(LocalizedString text, Locale locale)
        {
            if (text == null
                || text.IsEmpty)
                return string.Empty;

            StringTableEntry entry = FindShownEntry(
                LocalizationEditorSettings.GetStringTableCollection(text.TableReference), text.TableEntryReference,
                locale);

            return entry == null
                ? string.Empty
                : entry.Value;
        }

        /// <summary>
        /// <paramref name="text"/> as <paramref name="locale"/> shows it: stretched for a pseudo-locale.
        /// </summary>
        public static string Stretch(string text, Locale locale) => locale is PseudoLocale pseudo
            ? pseudo.GetPseudoString(text)
            : text;

        private static StringTableEntry FindEntry(StringTableCollection collection, TableEntryReference reference,
            LocaleIdentifier locale)
        {
            StringTable table = collection.GetTable(locale) as StringTable;

            return table == null
                ? null
                : table.GetEntryFromReference(reference);
        }

        // Japanese and Chinese break between any two characters, so a long word cannot exist there. The pseudo-locale
        // pads with characters that have no spaces between them, which would only report made-up words.
        internal static bool ChecksWords(Locale locale)
        {
            if (locale is PseudoLocale)
                return false;

            foreach (string code in UnspacedLanguages)
            {
                if (locale.Identifier.Code.StartsWith(code))
                    return false;
            }

            return true;
        }
    }
}