using System.Text.RegularExpressions;
using Base.SettingsPackage.Core;
using Base.UtilityPackage.Identification;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Base.SettingsPackage.Tests
{
    /// <summary>
    /// Covers the registry a settings menu drives. It has to keep the order settings were registered
    /// in, since one setting sometimes has to be applied before another, and it has to commit the
    /// whole set once rather than per setting.
    /// </summary>
    public sealed class SettingsRegistryTests
    {
        private const string AbsentKey = "Absent";
        private const string FirstKey = "MasterVolume";
        private const string SecondKey = "MusicVolume";

        private SettingsStoreProbe _store;
        private SettingsRegistry _registry;
        private FloatSetting _first;
        private FloatSetting _second;

        /// <summary>Every test starts from a registry holding two float settings.</summary>
        [SetUp]
        public void Build()
        {
            _store = new SettingsStoreProbe();
            _registry = new SettingsRegistry(_store);
            _first = _registry.Register(new FloatSetting(_store, new PersistentKey(FirstKey), 0.5f));
            _second = _registry.Register(new FloatSetting(_store, new PersistentKey(SecondKey), 0.5f));
        }

        /// <summary>Registering hands the setting straight back, so it can be captured in one statement.</summary>
        [Test]
        public void RegisteringHandsTheSettingBack()
        {
            Assert.That(_first, Is.Not.Null);
            Assert.That(_first.Key.Value, Is.EqualTo(FirstKey));
        }

        /// <summary>The order settings were registered in is the order they are applied in.</summary>
        [Test]
        public void TheRegistrationOrderIsKept() => Assert.That(_registry.Settings, Is.EqualTo(new ISetting[]
        {
            _first,
            _second
        }));

        /// <summary>The registry answers whether it holds a key.</summary>
        [Test]
        public void TheRegistryReportsWhatItHolds()
        {
            Assert.That(_registry.Contains(new PersistentKey(FirstKey)), Is.True);
            Assert.That(_registry.Contains(new PersistentKey(AbsentKey)), Is.False);
        }

        /// <summary>A setting is resolved by key as the type the caller asked for.</summary>
        [Test]
        public void ASettingIsResolvedByKey()
        {
            Assert.That(_registry.TryGet(new PersistentKey(FirstKey), out FloatSetting found), Is.True);
            Assert.That(found, Is.SameAs(_first));
            Assert.That(_registry.Get<FloatSetting>(new PersistentKey(SecondKey)), Is.SameAs(_second));
        }

        /// <summary>A key nobody registered is a bug in the caller and is reported as one.</summary>
        [Test]
        public void AnUnknownKeyIsReported()
        {
            LogAssert.Expect(LogType.Error, new Regex(AbsentKey));

            Assert.That(_registry.TryGet(new PersistentKey(AbsentKey), out FloatSetting found), Is.False);
            Assert.That(found, Is.Null);
        }

        /// <summary>Asking for the wrong type is reported rather than answered with nothing.</summary>
        [Test]
        public void TheWrongTypeIsReported()
        {
            LogAssert.Expect(LogType.Error, new Regex(nameof(BoolSetting)));

            Assert.That(_registry.TryGet(new PersistentKey(FirstKey), out BoolSetting found), Is.False);
            Assert.That(found, Is.Null);
        }

        /// <summary>The shorthand answers with nothing when the lookup fails.</summary>
        [Test]
        public void TheShorthandAnswersNothingOnAFailedLookup()
        {
            LogAssert.Expect(LogType.Error, new Regex(AbsentKey));

            Assert.That(_registry.Get<FloatSetting>(new PersistentKey(AbsentKey)), Is.Null);
        }

        /// <summary>
        /// Two settings under one key would fight over the same persisted value, so the second is
        /// refused and the caller gets the one that is already registered.
        /// </summary>
        [Test]
        public void ASecondSettingUnderTheSameKeyIsRefused()
        {
            LogAssert.Expect(LogType.Error, new Regex(FirstKey));

            FloatSetting duplicate = _registry.Register(new FloatSetting(_store, new PersistentKey(FirstKey), 1f));

            Assert.That(duplicate, Is.SameAs(_first));
            Assert.That(_registry.Settings.Count, Is.EqualTo(2));
        }

        /// <summary>Nothing to register is reported rather than walked into.</summary>
        [Test]
        public void NothingToRegisterIsReported()
        {
            LogAssert.Expect(LogType.Error, new Regex("null setting"));

            Assert.That(_registry.Register<FloatSetting>(null), Is.Null);
            Assert.That(_registry.Settings.Count, Is.EqualTo(2));
        }

        /// <summary>Loading reaches every registered setting.</summary>
        [Test]
        public void LoadingReachesEverySetting()
        {
            _store.SetFloat(FirstKey, 0.1f);
            _store.SetFloat(SecondKey, 0.2f);

            _registry.LoadAll();

            Assert.That(_first.Value, Is.EqualTo(0.1f));
            Assert.That(_second.Value, Is.EqualTo(0.2f));
        }

        /// <summary>Saving writes every setting and commits the batch exactly once.</summary>
        [Test]
        public void SavingWritesEverySettingAndCommitsOnce()
        {
            _first.Value = 0.1f;
            _second.Value = 0.2f;

            _registry.SaveAll();

            Assert.That(_store.GetFloat(FirstKey, float.NaN), Is.EqualTo(0.1f));
            Assert.That(_store.GetFloat(SecondKey, float.NaN), Is.EqualTo(0.2f));
            Assert.That(_store.FlushCount, Is.EqualTo(1));
        }

        /// <summary>Saving commits even when nothing changed, so the caller does not have to check.</summary>
        [Test]
        public void SavingCommitsEvenWithoutChanges()
        {
            _registry.SaveAll();

            Assert.That(_store.FlushCount, Is.EqualTo(1));
        }

        /// <summary>Reverting takes every setting back to what was last written.</summary>
        [Test]
        public void RevertingReachesEverySetting()
        {
            _first.Value = 0.1f;
            _second.Value = 0.2f;
            _registry.SaveAll();

            _first.Value = 0.9f;
            _second.Value = 0.9f;
            _registry.RevertAll();

            Assert.That(_first.Value, Is.EqualTo(0.1f));
            Assert.That(_second.Value, Is.EqualTo(0.2f));
        }

        /// <summary>Resetting takes every setting back to its default.</summary>
        [Test]
        public void ResettingReachesEverySetting()
        {
            _first.Value = 0.1f;
            _second.Value = 0.2f;

            _registry.ResetAllToDefault();

            Assert.That(_first.IsDefault, Is.True);
            Assert.That(_second.IsDefault, Is.True);
        }

        /// <summary>
        /// A change to any registered setting reaches the registry, so a preset button can follow the
        /// whole set without subscribing to each setting itself.
        /// </summary>
        [Test]
        public void AnyChangeReachesTheRegistry()
        {
            int changes = 0;

            _registry.OnAnyValueChanged += () => changes++;

            _first.Value = 0.1f;
            _second.Value = 0.2f;

            Assert.That(changes, Is.EqualTo(2));
        }

        /// <summary>A setting that did not move does not wake the registry either.</summary>
        [Test]
        public void AnUnchangedSettingDoesNotReachTheRegistry()
        {
            int changes = 0;

            _registry.OnAnyValueChanged += () => changes++;

            float unchanged = _first.Value;
            _first.Value = unchanged;

            Assert.That(changes, Is.EqualTo(0));
        }
    }
}