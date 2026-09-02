using Base.SettingsPackage.Core;
using Base.UtilityPackage.Identification;
using NUnit.Framework;

namespace Base.SettingsPackage.Tests
{
    /// <summary>
    /// Covers what every setting does regardless of the type it holds: it starts at its default, it
    /// only announces a real change, and it can go back either to what was last written or to the
    /// default. A settings menu with an apply and a cancel button rests entirely on those two.
    /// </summary>
    /// <remarks>
    /// Driven through a float setting, since the behavior lives in the shared base and the value type
    /// only decides which store method is used.
    /// </remarks>
    public sealed class SettingLifecycleTests
    {
        private const float DefaultValue = 0.5f;
        private const string Key = "MasterVolume";
        private const float OtherValue = 0.25f;
        private const float StoredValue = 0.8f;

        private SettingsStoreProbe _store;
        private FloatSetting _setting;
        private int _typedChanges;
        private int _untypedChanges;
        private float _lastValue;

        /// <summary>Every test starts from a setting at its default with both events counted.</summary>
        [SetUp]
        public void Build()
        {
            _store = new SettingsStoreProbe();
            _setting = new FloatSetting(_store, new PersistentKey(Key), DefaultValue);
            _typedChanges = 0;
            _untypedChanges = 0;
            _lastValue = float.NaN;

            _setting.OnValueChanged += OnTypedChanged;
            _setting.OnChanged += OnUntypedChanged;
        }

        /// <summary>A setting that was never loaded already holds something usable.</summary>
        [Test]
        public void ANewSettingStartsAtItsDefault()
        {
            Assert.That(_setting.Value, Is.EqualTo(DefaultValue));
            Assert.That(_setting.DefaultValue, Is.EqualTo(DefaultValue));
            Assert.That(_setting.IsDefault, Is.True);
            Assert.That(_setting.Key.Value, Is.EqualTo(Key));
        }

        /// <summary>A new value is announced to both kinds of listener, once each.</summary>
        [Test]
        public void AChangedValueIsAnnouncedOnce()
        {
            _setting.Value = OtherValue;

            Assert.That(_setting.Value, Is.EqualTo(OtherValue));
            Assert.That(_typedChanges, Is.EqualTo(1));
            Assert.That(_untypedChanges, Is.EqualTo(1));
            Assert.That(_lastValue, Is.EqualTo(OtherValue));
        }

        /// <summary>
        /// Assigning the value it already holds announces nothing, so an applier that rebuilds a
        /// resolution or restarts an audio bus does not run for a change that did not happen.
        /// </summary>
        [Test]
        public void AnUnchangedValueIsNotAnnounced()
        {
            _setting.Value = DefaultValue;

            Assert.That(_typedChanges, Is.EqualTo(0));
            Assert.That(_untypedChanges, Is.EqualTo(0));
        }

        /// <summary>A setting knows whether it still holds what it shipped with.</summary>
        [Test]
        public void ASettingKnowsWhenItIsNoLongerDefault()
        {
            _setting.Value = OtherValue;

            Assert.That(_setting.IsDefault, Is.False);

            _setting.ResetToDefault();

            Assert.That(_setting.IsDefault, Is.True);
        }

        /// <summary>Loading takes the persisted value and tells everyone about it.</summary>
        [Test]
        public void LoadingTakesThePersistedValue()
        {
            _store.SetFloat(Key, StoredValue);

            _setting.Load();

            Assert.That(_setting.Value, Is.EqualTo(StoredValue));
            Assert.That(_typedChanges, Is.EqualTo(1));
            Assert.That(_untypedChanges, Is.EqualTo(1));
        }

        /// <summary>Loading with nothing persisted keeps the default rather than clearing the value.</summary>
        [Test]
        public void LoadingWithoutAPersistedValueKeepsTheDefault()
        {
            _setting.Load();

            Assert.That(_setting.Value, Is.EqualTo(DefaultValue));
        }

        /// <summary>
        /// Loading always announces, even when the value did not move. Listeners apply the value rather
        /// than react to a difference, so a fresh launch has to reach them too.
        /// </summary>
        [Test]
        public void LoadingAlwaysAnnounces()
        {
            _store.SetFloat(Key, DefaultValue);

            _setting.Load();

            Assert.That(_typedChanges, Is.EqualTo(1));
        }

        /// <summary>Saving an untouched setting writes nothing.</summary>
        [Test]
        public void SavingAnUntouchedSettingWritesNothing()
        {
            _setting.Save();

            Assert.That(_store.Has(Key), Is.False);
        }

        /// <summary>Saving writes the value that is currently held.</summary>
        [Test]
        public void SavingWritesTheCurrentValue()
        {
            _setting.Value = OtherValue;
            _setting.Save();

            Assert.That(_store.GetFloat(Key, float.NaN), Is.EqualTo(OtherValue));
        }

        /// <summary>Saving twice without a change in between writes once.</summary>
        [Test]
        public void SavingTwiceWritesOnce()
        {
            _setting.Value = OtherValue;
            _setting.Save();
            _store.Delete(Key);
            _setting.Save();

            Assert.That(_store.Has(Key), Is.False, "the second save had nothing to write");
        }

        /// <summary>Reverting goes back to what was last written, which is what cancel does.</summary>
        [Test]
        public void RevertingGoesBackToTheLastSavedValue()
        {
            _setting.Value = OtherValue;
            _setting.Save();
            _setting.Value = StoredValue;

            _setting.Revert();

            Assert.That(_setting.Value, Is.EqualTo(OtherValue));
        }

        /// <summary>Reverting an unsaved setting goes back to what was loaded.</summary>
        [Test]
        public void RevertingAfterLoadingGoesBackToTheLoadedValue()
        {
            _store.SetFloat(Key, StoredValue);
            _setting.Load();
            _setting.Value = OtherValue;

            _setting.Revert();

            Assert.That(_setting.Value, Is.EqualTo(StoredValue));
        }

        /// <summary>Reverting a setting that never moved announces nothing.</summary>
        [Test]
        public void RevertingAnUnchangedSettingIsSilent()
        {
            _setting.Revert();

            Assert.That(_untypedChanges, Is.EqualTo(0));
        }

        /// <summary>Resetting goes back to the default and announces it.</summary>
        [Test]
        public void ResettingGoesBackToTheDefault()
        {
            _setting.Value = OtherValue;
            _typedChanges = 0;

            _setting.ResetToDefault();

            Assert.That(_setting.Value, Is.EqualTo(DefaultValue));
            Assert.That(_typedChanges, Is.EqualTo(1));
        }

        /// <summary>A reset is a change like any other, so it still has to be saved to persist.</summary>
        [Test]
        public void AResetStillHasToBeSaved()
        {
            _setting.Value = OtherValue;
            _setting.Save();
            _setting.ResetToDefault();

            Assert.That(_store.GetFloat(Key, float.NaN), Is.EqualTo(OtherValue), "the store is untouched so far");

            _setting.Save();

            Assert.That(_store.GetFloat(Key, float.NaN), Is.EqualTo(DefaultValue));
        }

        private void OnTypedChanged(float value)
        {
            _typedChanges++;
            _lastValue = value;
        }

        private void OnUntypedChanged() => _untypedChanges++;
    }
}