using System.Collections.Generic;
using Base.SaveSystemPackage.Settings;
using Base.SaveSystemPackage.Unity.Autosave;
using Base.UtilityPackage.Identification;
using NUnit.Framework;
using UnityEngine;

namespace Base.SaveSystemPackage.Tests
{
    /// <summary>
    /// Covers the key each autosave setting is stored under. The component writes the value, the
    /// service reads it, and neither checks the other, so two components sharing a key means one
    /// setting quietly overwrites the other every time the player changes it.
    /// </summary>
    /// <remarks>
    /// The components are added to an object that is switched off, so no Unity callback runs and the
    /// key can be read without a config asset or a registered service standing behind it.
    /// </remarks>
    public sealed class AutosaveSettingKeysTests
    {
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

        /// <summary>The toggle reads and writes the key the service watches for it.</summary>
        [Test]
        public void TheEnabledToggleUsesTheSharedEnabledKey()
            => Assert.That(Create<AutosaveEnabledSetting>().Key, Is.EqualTo(AutosaveSettingKeys.Enabled));

        /// <summary>The interval slider reads and writes the key the service watches for it.</summary>
        [Test]
        public void TheIntervalSliderUsesTheSharedIntervalKey()
            => Assert.That(Create<AutosaveIntervalSetting>().Key, Is.EqualTo(AutosaveSettingKeys.Interval));

        /// <summary>The cooldown reads and writes the key the service watches for it.</summary>
        [Test]
        public void TheCooldownUsesTheSharedCooldownKey()
            => Assert.That(Create<AutosaveCooldownSetting>().Key, Is.EqualTo(AutosaveSettingKeys.Cooldown));

        /// <summary>
        /// The three keys are distinct. Two of them being equal is the failure that makes a player's
        /// interval land on top of their cooldown, and it is one copied line away.
        /// </summary>
        [Test]
        public void TheThreeAutosaveSettingsDoNotShareAKey()
        {
            HashSet<PersistentKey> keys = new()
            {
                Create<AutosaveEnabledSetting>().Key,
                Create<AutosaveIntervalSetting>().Key,
                Create<AutosaveCooldownSetting>().Key
            };

            Assert.That(keys, Has.Count.EqualTo(3));
        }

        /// <summary>
        /// A key has to survive being written to disk and read back, so an empty or malformed one is
        /// a setting that never persists.
        /// </summary>
        [Test]
        public void EveryAutosaveKeyIsStorable()
        {
            Assert.That(PersistentKey.IsValid(AutosaveSettingKeys.Enabled.Value), Is.True);
            Assert.That(PersistentKey.IsValid(AutosaveSettingKeys.Interval.Value), Is.True);
            Assert.That(PersistentKey.IsValid(AutosaveSettingKeys.Cooldown.Value), Is.True);
        }

        /// <summary>
        /// Adds the component to an object that is switched off, so no callback runs and nothing has
        /// to be standing behind it.
        /// </summary>
        private T Create<T>() where T : Component
        {
            GameObject host = new(typeof(T).Name);
            host.SetActive(false);
            _hosts.Add(host);

            return host.AddComponent<T>();
        }
    }
}