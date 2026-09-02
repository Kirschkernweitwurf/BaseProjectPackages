using System.Collections.Generic;
using Base.ControllerSupportPackage.Haptics;
using Base.UtilityPackage.Identification;
using NUnit.Framework;

namespace Base.ControllerSupportPackage.Tests
{
    /// <summary>
    /// Covers the keys the rumble settings persist under. They are built at type load, so an invalid
    /// one would throw the first time anything touched the class rather than where it was written.
    /// Two keys colliding would silently make one setting overwrite the other.
    /// </summary>
    public sealed class RumbleSettingKeysTests
    {
        /// <summary>Every key the class publishes. One test case is generated per key.</summary>
        private static IEnumerable<PersistentKey> EveryKey()
        {
            yield return RumbleSettingKeys.Enabled;
            yield return RumbleSettingKeys.Intensity;
        }

        /// <summary>A key is valid, so touching the class cannot throw.</summary>
        /// <param name="key">The key under test.</param>
        [TestCaseSource(nameof(EveryKey))]
        public void AKeyIsValid(PersistentKey key)
            => Assert.That(key.IsEmpty, Is.False);

        /// <summary>No two settings share a key, so neither can overwrite the other.</summary>
        [Test]
        public void NoTwoKeysCollide()
            => Assert.That(EveryKey(), Is.Unique);
    }
}