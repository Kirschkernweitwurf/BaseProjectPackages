using System.Collections.Generic;
using Base.ControllerSupportPackage.Haptics;
using Base.ControllerSupportPackage.Settings;
using Base.UtilityPackage.Identification;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Base.ControllerSupportPackage.Tests
{
    /// <summary>
    /// Covers the key each rumble setting is stored under. The component writes the value and the
    /// service reads it, so a key that does not match means the option in the menu moves and the
    /// controller keeps doing what it was doing.
    /// </summary>
    /// <remarks>
    /// The components are added to an object that is switched off, so no Unity callback runs and the
    /// key can be read without a config asset or a registered service standing behind it.
    /// </remarks>
    public sealed class RumbleSettingKeysTests
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
            => Assert.That(Create<RumbleEnabledSetting>().Key, Is.EqualTo(RumbleSettingKeys.Enabled));

        /// <summary>The intensity slider reads and writes the key the service watches for it.</summary>
        [Test]
        public void TheIntensitySliderUsesTheSharedIntensityKey()
            => Assert.That(Create<RumbleIntensitySetting>().Key, Is.EqualTo(RumbleSettingKeys.Intensity));

        /// <summary>
        /// Turning rumble off and turning it down are two settings, so they cannot land on one key.
        /// </summary>
        [Test]
        public void TheTwoRumbleSettingsDoNotShareAKey()
            => Assert.That(Create<RumbleEnabledSetting>().Key,
                Is.Not.EqualTo(Create<RumbleIntensitySetting>().Key));

        /// <summary>
        /// A key has to survive being written to disk and read back, so an empty or malformed one is
        /// a setting that never persists.
        /// </summary>
        [Test]
        public void EveryRumbleKeyIsStorable()
        {
            Assert.That(PersistentKey.IsValid(RumbleSettingKeys.Enabled.Value), Is.True);
            Assert.That(PersistentKey.IsValid(RumbleSettingKeys.Intensity.Value), Is.True);
        }

        /// <summary>
        /// Adds the component to an object that is switched off, so no callback runs and nothing has
        /// to be standing behind it.
        /// </summary>
        private T Create<T>() where T : Component
        {
            GameObject host = EditorUtility.CreateGameObjectWithHideFlags(typeof(T).Name,
                HideFlags.HideAndDontSave);
            host.SetActive(false);
            _hosts.Add(host);

            return host.AddComponent<T>();
        }
    }
}