using System.Collections.Generic;
using Base.LocalizationPackage.Settings;
using Base.UtilityPackage.Identification;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Base.LocalizationPackage.Tests
{
    /// <summary>
    /// Covers the key the chosen language is stored under. Unlike the other setting components in the
    /// ecosystem this one composes its key inline rather than taking it from a shared constant, so
    /// there is nothing else in the code holding it to a particular spelling.
    /// </summary>
    /// <remarks>
    /// The component is added to an object that is switched off, so no Unity callback runs and the key
    /// can be read without the localization system standing behind it.
    /// </remarks>
    public sealed class LanguageSettingTests
    {
        private const string ExpectedKey = "Language";

        private readonly List<GameObject> _hosts = new();

        /// <summary>Hands back everything the test put in the scene.</summary>
        [TearDown]
        public void Cleanup()
        {
            foreach (GameObject host in _hosts)
            {
                if (host != null)
                    Object.DestroyImmediate(host);
            }

            _hosts.Clear();
        }

        /// <summary>
        /// The spelling is pinned here because nothing else pins it. Changing it silently resets every
        /// player back to their system language on the next launch, since the old entry stops being
        /// found and the new one has never been written.
        /// </summary>
        [Test]
        public void TheLanguageIsStoredUnderItsOwnKey()
            => Assert.That(Create().Key.Value, Is.EqualTo(ExpectedKey));

        /// <summary>
        /// A key has to survive being written to disk and read back, so an empty or malformed one is a
        /// setting that never persists.
        /// </summary>
        [Test]
        public void TheLanguageKeyIsStorable()
            => Assert.That(PersistentKey.IsValid(Create().Key.Value), Is.True);

        /// <summary>
        /// Every instance answers with the same key, or two menus showing the language would each read
        /// their own copy of it.
        /// </summary>
        [Test]
        public void EveryInstanceAgreesOnTheKey()
            => Assert.That(Create().Key, Is.EqualTo(Create().Key));

        /// <summary>
        /// Adds the component to an object that is switched off, so no callback runs and nothing has
        /// to be standing behind it.
        /// </summary>
        private LanguageSetting Create()
        {
            GameObject host = EditorUtility.CreateGameObjectWithHideFlags(nameof(LanguageSetting),
                HideFlags.HideAndDontSave);
            host.SetActive(false);
            _hosts.Add(host);

            return host.AddComponent<LanguageSetting>();
        }
    }
}