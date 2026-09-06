using System.Collections.Generic;
using Base.LocalizationPackage.Settings;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;

namespace Base.LocalizationPackage.Tests
{
    /// <summary>
    /// Covers which language the component answers with before a player has chosen one.
    /// <para>
    /// The stored value is an index into a list that lives in the scene, so the two drift apart the
    /// moment a language is added or removed. An index that no longer points at anything would read
    /// past the end of the list and throw on the first frame of a build that shipped with one language
    /// fewer than the save it loaded.
    /// </para>
    /// </summary>
    /// <remarks>
    /// The component is added to an object that is switched off, so no Unity callback runs and the
    /// locale can be read without the localization system standing behind it. With no setting stored
    /// yet, that read goes through the starting index, which is the path under test.
    /// </remarks>
    public sealed class LanguageSettingLocaleTests
    {
        private const string EnglishCode = "en";
        private const string FrenchCode = "fr";
        private const string GermanCode = "de";

        private readonly List<GameObject> _hosts = new();
        private readonly List<Object> _locales = new();

        /// <summary>Hands back the objects and the unsaved locale assets the test made.</summary>
        [TearDown]
        public void Cleanup()
        {
            foreach (GameObject host in _hosts)
            {
                if (host != null)
                    Object.DestroyImmediate(host);
            }

            foreach (Object locale in _locales)
            {
                if (locale != null)
                    Object.DestroyImmediate(locale);
            }

            _hosts.Clear();
            _locales.Clear();
        }

        /// <summary>The starting index is what decides the language until a player picks one.</summary>
        [Test]
        public void TheStartingIndexPicksTheLanguage()
        {
            Locale[] locales = CreateLocales(EnglishCode, GermanCode, FrenchCode);
            LanguageSetting setting = Create(locales, 1);

            Assert.That(setting.CurrentLocale, Is.SameAs(locales[1]));
        }

        /// <summary>
        /// An index past the end is what a build shipping one language fewer than the last one leaves
        /// behind. It reads as the last language rather than throwing.
        /// </summary>
        /// <param name="index">The starting index the component was left with.</param>
        [TestCase(3)]
        [TestCase(99)]
        public void AnIndexPastTheEndReadsAsTheLastLanguage(int index)
        {
            Locale[] locales = CreateLocales(EnglishCode, GermanCode, FrenchCode);
            LanguageSetting setting = Create(locales, index);

            Assert.That(setting.CurrentLocale, Is.SameAs(locales[^1]));
        }

        /// <summary>A negative index reads as the first language, for the same reason.</summary>
        /// <param name="index">The starting index the component was left with.</param>
        [TestCase(-1)]
        [TestCase(-99)]
        public void ANegativeIndexReadsAsTheFirstLanguage(int index)
        {
            Locale[] locales = CreateLocales(EnglishCode, GermanCode, FrenchCode);
            LanguageSetting setting = Create(locales, index);

            Assert.That(setting.CurrentLocale, Is.SameAs(locales[0]));
        }

        /// <summary>
        /// A project that ships one language answers with it whatever the stored index says, which is
        /// the narrowest the list can get without being empty.
        /// </summary>
        [Test]
        public void ASingleLanguageIsAlwaysTheAnswer()
        {
            Locale[] locales = CreateLocales(EnglishCode);
            LanguageSetting setting = Create(locales, 7);

            Assert.That(setting.CurrentLocale, Is.SameAs(locales[0]));
        }

        /// <summary>Creates the locales and remembers them, so the teardown destroys them.</summary>
        /// <param name="codes">The language codes to create locales for.</param>
        /// <returns>The locales, in the order the codes were given.</returns>
        private Locale[] CreateLocales(params string[] codes)
        {
            Locale[] locales = new Locale[codes.Length];

            for (int i = 0; i < codes.Length; i++)
            {
                locales[i] = Locale.CreateLocale(new LocaleIdentifier(codes[i]));
                _locales.Add(locales[i]);
            }

            return locales;
        }

        /// <summary>
        /// Builds a component on an object that is switched off, so no callback runs and the read goes
        /// through the starting index rather than through a stored setting.
        /// </summary>
        /// <param name="locales">The locales the component offers.</param>
        /// <param name="defaultIndex">The starting index it is left with.</param>
        /// <returns>The wired component.</returns>
        private LanguageSetting Create(Locale[] locales, int defaultIndex)
        {
            GameObject host = EditorUtility.CreateGameObjectWithHideFlags(nameof(LanguageSetting),
                HideFlags.HideAndDontSave);

            host.SetActive(false);
            _hosts.Add(host);

            LanguageSetting setting = host.AddComponent<LanguageSetting>();
            SerializedObject serialized = new(setting);
            SerializedProperty list = serialized.FindProperty(LanguageSetting.AvailableLocalesField);

            list.arraySize = locales.Length;

            for (int i = 0; i < locales.Length; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = locales[i];

            serialized.FindProperty(LanguageSetting.DefaultIndexField).intValue = defaultIndex;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return setting;
        }
    }
}