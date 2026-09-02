using Base.SettingsPackage.Core;
using Base.UtilityPackage.Identification;
using NUnit.Framework;

namespace Base.SettingsPackage.Tests
{
    /// <summary>
    /// Covers how each value type is written into a store that only knows integers, floats and
    /// strings. A boolean and an enum both go in as numbers, so the mapping in both directions is
    /// what decides whether a persisted setting comes back as the same value.
    /// </summary>
    public sealed class SettingStorageTests
    {
        private const string Key = "Probe";

        private SettingsStoreProbe _store;
        private PersistentKey _key;

        /// <summary>Every test starts from an empty store.</summary>
        [SetUp]
        public void Build()
        {
            _store = new SettingsStoreProbe();
            _key = new PersistentKey(Key);
        }

        /// <summary>A setting is stored under the text of its key.</summary>
        [Test]
        public void ASettingIsStoredUnderItsKey()
        {
            IntSetting setting = new(_store, _key, 0);

            setting.Value = 5;
            setting.Save();

            Assert.That(_store.Has(Key), Is.True);
        }

        /// <summary>An integer setting round trips through the store.</summary>
        [Test]
        public void AnIntegerRoundTrips()
        {
            IntSetting written = new(_store, _key, 0);

            written.Value = 42;
            written.Save();

            IntSetting read = new(_store, _key, 0);
            read.Load();

            Assert.That(read.Value, Is.EqualTo(42));
        }

        /// <summary>A float setting round trips through the store.</summary>
        [Test]
        public void AFloatRoundTrips()
        {
            FloatSetting written = new(_store, _key, 0f);

            written.Value = 0.75f;
            written.Save();

            FloatSetting read = new(_store, _key, 0f);
            read.Load();

            Assert.That(read.Value, Is.EqualTo(0.75f));
        }

        /// <summary>A string setting round trips through the store.</summary>
        [Test]
        public void AStringRoundTrips()
        {
            StringSetting written = new(_store, _key, string.Empty);

            written.Value = "English";
            written.Save();

            StringSetting read = new(_store, _key, string.Empty);
            read.Load();

            Assert.That(read.Value, Is.EqualTo("English"));
        }

        /// <summary>A boolean goes into the store as one or zero.</summary>
        [Test]
        public void ABooleanIsStoredAsANumber()
        {
            BoolSetting setting = new(_store, _key, false);

            setting.Value = true;
            setting.Save();

            Assert.That(_store.GetInt(Key, -1), Is.EqualTo(1));

            setting.Value = false;
            setting.Save();

            Assert.That(_store.GetInt(Key, -1), Is.EqualTo(0));
        }

        /// <summary>A boolean round trips through the store.</summary>
        [Test]
        public void ABooleanRoundTrips()
        {
            BoolSetting written = new(_store, _key, false);

            written.Value = true;
            written.Save();

            BoolSetting read = new(_store, _key, false);
            read.Load();

            Assert.That(read.Value, Is.True);
        }

        /// <summary>Anything other than zero reads back as true.</summary>
        [Test]
        public void AnyNonZeroNumberReadsAsTrue()
        {
            _store.SetInt(Key, 7);

            BoolSetting setting = new(_store, _key, false);
            setting.Load();

            Assert.That(setting.Value, Is.True);
        }

        /// <summary>A boolean with nothing stored falls back to its default.</summary>
        [Test]
        public void ABooleanFallsBackToItsDefault()
        {
            BoolSetting setting = new(_store, _key, true);

            setting.Load();

            Assert.That(setting.Value, Is.True);
        }

        /// <summary>An enum goes into the store as its underlying number.</summary>
        [Test]
        public void AnEnumIsStoredAsItsUnderlyingNumber()
        {
            EnumSetting<ESettingProbeMode> setting = new(_store, _key, ESettingProbeMode.Off);

            setting.Value = ESettingProbeMode.High;
            setting.Save();

            Assert.That(_store.GetInt(Key, -1), Is.EqualTo((int)ESettingProbeMode.High));
        }

        /// <summary>An enum round trips through the store.</summary>
        [Test]
        public void AnEnumRoundTrips()
        {
            EnumSetting<ESettingProbeMode> written = new(_store, _key, ESettingProbeMode.Off);

            written.Value = ESettingProbeMode.Low;
            written.Save();

            EnumSetting<ESettingProbeMode> read = new(_store, _key, ESettingProbeMode.Off);
            read.Load();

            Assert.That(read.Value, Is.EqualTo(ESettingProbeMode.Low));
        }

        /// <summary>An enum with nothing stored falls back to its default.</summary>
        [Test]
        public void AnEnumFallsBackToItsDefault()
        {
            EnumSetting<ESettingProbeMode> setting = new(_store, _key, ESettingProbeMode.High);

            setting.Load();

            Assert.That(setting.Value, Is.EqualTo(ESettingProbeMode.High));
        }

        /// <summary>Two settings under different keys do not overwrite each other.</summary>
        [Test]
        public void SeparateKeysDoNotCollide()
        {
            IntSetting first = new(_store, _key, 0);
            IntSetting second = new(_store, new PersistentKey("Other"), 0);

            first.Value = 1;
            second.Value = 2;
            first.Save();
            second.Save();

            Assert.That(_store.GetInt(Key, -1), Is.EqualTo(1));
            Assert.That(_store.GetInt("Other", -1), Is.EqualTo(2));
        }
    }
}