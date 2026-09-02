using System.Text.RegularExpressions;
using Base.SaveSystemPackage.Savable;
using Base.ServicesPackage.Tracking;
using Base.UtilityPackage.Identification;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Base.SaveSystemPackage.Tests
{
    /// <summary>
    /// Covers who gets to be in a save and in what order. A key has to be unique, since it is what a
    /// stored state is matched back to, and the order has to be stable so a save written twice from
    /// the same set comes out the same way.
    /// </summary>
    public sealed class SavableRegistryTests
    {
        private const string PlayerKey = "Player";


        private SavableRegistry _registry;

        /// <summary>Every test starts from an empty registry.</summary>
        [SetUp]
        public void Build() => _registry = new SavableRegistry();

        /// <summary>A registered savable takes part in the next save.</summary>
        [Test]
        public void ARegisteredSavableIsListed()
        {
            SavableProbe probe = Probe(PlayerKey);

            _registry.Register(probe);

            Assert.That(_registry.GetOrdered(), Is.EqualTo(new[] { probe }));
        }

        /// <summary>An empty registry lists nothing rather than throwing.</summary>
        [Test]
        public void AnEmptyRegistryListsNothing()
            => Assert.That(_registry.GetOrdered(), Is.Empty);

        /// <summary>Registering the same instance twice adds it once.</summary>
        [Test]
        public void TheSameInstanceIsOnlyListedOnce()
        {
            SavableProbe probe = Probe(PlayerKey);

            _registry.Register(probe);
            _registry.Register(probe);

            Assert.That(_registry.GetOrdered().Count, Is.EqualTo(1));
        }

        /// <summary>Nothing to register is ignored rather than walked into.</summary>
        [Test]
        public void NothingToRegisterIsIgnored()
        {
            _registry.Register(null);

            Assert.That(_registry.GetOrdered(), Is.Empty);
        }

        /// <summary>
        /// A savable without a key could never have its state matched back to it, so it is refused and
        /// reported instead of silently taking part.
        /// </summary>
        [Test]
        public void ASavableWithoutAKeyIsRefused()
        {
            LogAssert.Expect(LogType.Warning, new Regex(nameof(PersistentKey)));

            _registry.Register(new SavableProbe(default(PersistentKey)));

            Assert.That(_registry.GetOrdered(), Is.Empty);
        }

        /// <summary>Two savables under one key would overwrite each other, so the second is refused.</summary>
        [Test]
        public void ASecondSavableUnderTheSameKeyIsRefused()
        {
            SavableProbe first = Probe(PlayerKey);

            _registry.Register(first);

            LogAssert.Expect(LogType.Warning, new Regex(PlayerKey));

            _registry.Register(Probe(PlayerKey));

            Assert.That(_registry.GetOrdered(), Is.EqualTo(new[] { first }));
        }

        /// <summary>Higher priority runs first, so its state is written and restored before the rest.</summary>
        [Test]
        public void HigherPriorityComesFirst()
        {
            SavableProbe low = Probe("Low", EPriority.Low);
            SavableProbe critical = Probe("Critical", EPriority.Critical);
            SavableProbe medium = Probe("Medium", EPriority.Medium);

            _registry.Register(low);
            _registry.Register(critical);
            _registry.Register(medium);

            Assert.That(_registry.GetOrdered(), Is.EqualTo(new[] { critical, medium, low }));
        }

        /// <summary>Equal priorities keep the order they registered in, so the result is repeatable.</summary>
        [Test]
        public void EqualPrioritiesKeepTheirRegistrationOrder()
        {
            SavableProbe first = Probe("First");
            SavableProbe second = Probe("Second");
            SavableProbe third = Probe("Third");
            SavableProbe fourth = Probe("Fourth");

            _registry.Register(first);
            _registry.Register(second);
            _registry.Register(third);
            _registry.Register(fourth);

            Assert.That(_registry.GetOrdered(), Is.EqualTo(new[] { first, second, third, fourth }));
        }

        /// <summary>A deregistered savable drops out of the next save.</summary>
        [Test]
        public void ADeregisteredSavableIsGone()
        {
            SavableProbe probe = Probe(PlayerKey);

            _registry.Register(probe);
            _registry.Deregister(probe);

            Assert.That(_registry.GetOrdered(), Is.Empty);
        }

        /// <summary>Deregistering frees the key, so a replacement can take it over.</summary>
        [Test]
        public void DeregisteringFreesTheKey()
        {
            SavableProbe first = Probe(PlayerKey);
            SavableProbe replacement = Probe(PlayerKey);

            _registry.Register(first);
            _registry.Deregister(first);
            _registry.Register(replacement);

            Assert.That(_registry.GetOrdered(), Is.EqualTo(new[] { replacement }));
        }

        /// <summary>Deregistering something that was never registered changes nothing.</summary>
        [Test]
        public void DeregisteringAStrangerChangesNothing()
        {
            SavableProbe probe = Probe(PlayerKey);

            _registry.Register(probe);
            _registry.Deregister(Probe("Other"));
            _registry.Deregister(null);

            Assert.That(_registry.GetOrdered(), Is.EqualTo(new[] { probe }));
        }

        /// <summary>The cached order is rebuilt after a change rather than handed out stale.</summary>
        [Test]
        public void TheOrderIsRebuiltAfterAChange()
        {
            SavableProbe first = Probe("First");

            _registry.Register(first);

            Assert.That(_registry.GetOrdered().Count, Is.EqualTo(1));

            SavableProbe second = Probe("Second", EPriority.Critical);

            _registry.Register(second);

            Assert.That(_registry.GetOrdered(), Is.EqualTo(new[] { second, first }));
        }

        private static SavableProbe Probe(string key, EPriority priority = EPriority.Medium)
            => new(new PersistentKey(key), priority);
    }
}