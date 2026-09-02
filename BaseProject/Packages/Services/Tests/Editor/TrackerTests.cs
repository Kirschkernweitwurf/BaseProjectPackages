using System.Text.RegularExpressions;
using Base.ServicesPackage.Tracking;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Base.ServicesPackage.Tests
{
    /// <summary>
    /// Covers the plain key to value tracker: a key holds one element, a second claim on the same key
    /// is refused and reported rather than silently overwriting what is already there.
    /// </summary>
    public sealed class TrackerTests
    {
        private const string FirstValue = "First";
        private const string Key = "Alpha";
        private const string OtherKey = "Beta";
        private const string SecondValue = "Second";


        private Tracker<string, string> _tracker;

        /// <summary>Every test starts from an empty tracker.</summary>
        [SetUp]
        public void Build() => _tracker = new Tracker<string, string>();

        /// <summary>A registered element comes back under its key.</summary>
        [Test]
        public void ARegisteredElementIsFound()
        {
            Assert.That(_tracker.Register(Key, FirstValue), Is.True);
            Assert.That(_tracker.TryGet(Key, out string found), Is.True);
            Assert.That(found, Is.EqualTo(FirstValue));
        }

        /// <summary>An unknown key finds nothing and says so.</summary>
        [Test]
        public void AnUnknownKeyFindsNothing()
        {
            Assert.That(_tracker.TryGet(Key, out string found), Is.False);
            Assert.That(found, Is.Null);
        }

        /// <summary>A key holds one element, so a second claim is refused and reported.</summary>
        [Test]
        public void AKeyCannotBeClaimedTwice()
        {
            _tracker.Register(Key, FirstValue);

            LogAssert.Expect(LogType.Warning, new Regex(Key));

            Assert.That(_tracker.Register(Key, SecondValue), Is.False);
            Assert.That(_tracker.TryGet(Key, out string found), Is.True);
            Assert.That(found, Is.EqualTo(FirstValue), "the first element stays");
        }

        /// <summary>Two keys hold their elements independently.</summary>
        [Test]
        public void SeparateKeysHoldSeparateElements()
        {
            _tracker.Register(Key, FirstValue);
            _tracker.Register(OtherKey, SecondValue);

            Assert.That(_tracker.TryGet(Key, out string first), Is.True);
            Assert.That(_tracker.TryGet(OtherKey, out string second), Is.True);
            Assert.That(first, Is.EqualTo(FirstValue));
            Assert.That(second, Is.EqualTo(SecondValue));
        }

        /// <summary>A removed key finds nothing afterwards.</summary>
        [Test]
        public void RemovingAKeyDropsItsElement()
        {
            _tracker.Register(Key, FirstValue);

            Assert.That(_tracker.Remove(Key), Is.True);
            Assert.That(_tracker.TryGet(Key, out string _), Is.False);
        }

        /// <summary>Removing an unknown key reports it instead of throwing.</summary>
        [Test]
        public void RemovingAnUnknownKeyReportsFalse()
            => Assert.That(_tracker.Remove(Key), Is.False);

        /// <summary>A removed key is free to be claimed again.</summary>
        [Test]
        public void ARemovedKeyCanBeClaimedAgain()
        {
            _tracker.Register(Key, FirstValue);
            _tracker.Remove(Key);

            Assert.That(_tracker.Register(Key, SecondValue), Is.True);
            Assert.That(_tracker.TryGet(Key, out string found), Is.True);
            Assert.That(found, Is.EqualTo(SecondValue));
        }

        /// <summary>Clearing has to leave the tracker as empty as a fresh one.</summary>
        [Test]
        public void ClearingDropsEveryElement()
        {
            _tracker.Register(Key, FirstValue);
            _tracker.Register(OtherKey, SecondValue);
            _tracker.Clear();

            Assert.That(_tracker.TryGet(Key, out string _), Is.False);
            Assert.That(_tracker.TryGet(OtherKey, out string _), Is.False);
        }
    }
}