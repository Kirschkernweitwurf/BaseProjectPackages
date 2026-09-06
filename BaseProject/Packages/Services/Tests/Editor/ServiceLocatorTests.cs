using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Base.ServicesPackage.Tests
{
    /// <summary>
    /// Covers what the locator has to get right for a scene reload to be survivable: an entry is
    /// filed under the type that was asked for, a replacement cannot be wiped by the instance it
    /// replaced, and an entry whose object was destroyed is reported and cleaned up rather than
    /// handed out.
    /// </summary>
    public sealed class ServiceLocatorTests
    {
        private const string First = "First";
        private const string Second = "Second";

        private UnityServiceProbe _probe;

        /// <summary>
        /// The locator is static and its reset only runs on entering play mode, so every test has to
        /// hand back what it registered.
        /// </summary>
        [TearDown]
        public void Reset()
        {
            Forget<ServiceProbe>();
            Forget<UnityServiceProbe>();
            Forget<IGameService>();

            if (_probe != null)
                Object.DestroyImmediate(_probe);

            _probe = null;
        }

        /// <summary>A registered service is handed back to whoever asks for its type.</summary>
        [Test]
        public void ARegisteredServiceIsFound()
        {
            ServiceProbe probe = new(First);

            ServiceLocator.Register(probe);

            Assert.That(ServiceLocator.TryGet(out ServiceProbe found), Is.True);
            Assert.That(found, Is.SameAs(probe));
        }

        /// <summary>A missing service is a bug, so it is reported rather than passed over.</summary>
        [Test]
        public void AMissingServiceIsReported()
        {
            LogAssert.Expect(LogType.Error, new Regex(nameof(ServiceProbe)));

            Assert.That(ServiceLocator.TryGet(out ServiceProbe found), Is.False);
            Assert.That(found, Is.Null);
        }

        /// <summary>An optional service that is absent is a normal state and stays quiet.</summary>
        [Test]
        public void AMissingOptionalServiceIsNotReported()
        {
            Assert.That(ServiceLocator.TryGetOptional(out ServiceProbe found), Is.False);
            Assert.That(found, Is.Null);
        }

        /// <summary>The shorthand hands back the same instance the lookup would.</summary>
        [Test]
        public void TheShorthandHandsBackTheService()
        {
            ServiceProbe probe = new(First);

            ServiceLocator.Register(probe);

            Assert.That(ServiceLocator.Get<ServiceProbe>(), Is.SameAs(probe));
        }

        /// <summary>The shorthand reports a missing service and answers with nothing.</summary>
        [Test]
        public void TheShorthandAnswersNothingWhenTheServiceIsMissing()
        {
            LogAssert.Expect(LogType.Error, new Regex(nameof(ServiceProbe)));

            Assert.That(ServiceLocator.Get<ServiceProbe>(), Is.Null);
        }

        /// <summary>A second live registration is a conflict, and the newer instance wins.</summary>
        [Test]
        public void ASecondLiveRegistrationOverwritesAndIsReported()
        {
            ServiceProbe first = new(First);
            ServiceProbe second = new(Second);

            ServiceLocator.Register(first);

            LogAssert.Expect(LogType.Warning, new Regex(nameof(ServiceProbe)));

            ServiceLocator.Register(second);

            Assert.That(ServiceLocator.TryGet(out ServiceProbe found), Is.True);
            Assert.That(found.Label, Is.EqualTo(Second));
        }

        /// <summary>A service is filed under the type it was registered as, not under its own.</summary>
        [Test]
        public void TheRegisteredTypeIsTheKey()
        {
            ServiceProbe probe = new(First);

            ServiceLocator.Register<IGameService>(probe);

            Assert.That(ServiceLocator.TryGetOptional(out ServiceProbe _), Is.False, "the concrete type is not a key");
            Assert.That(ServiceLocator.TryGetOptional(out IGameService found), Is.True);
            Assert.That(found, Is.SameAs(probe));
        }

        /// <summary>Deregistering removes the entry, so a later lookup finds nothing.</summary>
        [Test]
        public void DeregisteringRemovesTheEntry()
        {
            ServiceProbe probe = new(First);

            ServiceLocator.Register(probe);
            ServiceLocator.Deregister(probe);

            Assert.That(ServiceLocator.TryGetOptional(out ServiceProbe _), Is.False);
        }

        /// <summary>Deregistering without an instance clears whatever is filed under the type.</summary>
        [Test]
        public void DeregisteringWithoutAnInstanceClearsTheType()
        {
            ServiceLocator.Register(new ServiceProbe(First));
            ServiceLocator.Deregister<ServiceProbe>();

            Assert.That(ServiceLocator.TryGetOptional(out ServiceProbe _), Is.False);
        }

        /// <summary>
        /// The instance that was replaced must not take the replacement with it when it tears down,
        /// which is the ordinary case during a scene reload.
        /// </summary>
        [Test]
        public void AnOutdatedInstanceCannotWipeItsReplacement()
        {
            ServiceProbe first = new(First);
            ServiceProbe second = new(Second);

            ServiceLocator.Register(first);

            LogAssert.Expect(LogType.Warning, new Regex(nameof(ServiceProbe)));

            ServiceLocator.Register(second);
            ServiceLocator.Deregister(first);

            Assert.That(ServiceLocator.TryGet(out ServiceProbe found), Is.True);
            Assert.That(found.Label, Is.EqualTo(Second));
        }

        /// <summary>Deregistering something that was never registered is reported, not silent.</summary>
        [Test]
        public void DeregisteringAnUnknownTypeIsReported()
        {
            LogAssert.Expect(LogType.Warning, new Regex(nameof(ServiceProbe)));

            ServiceLocator.Deregister<ServiceProbe>();
        }

        /// <summary>A registration without a type has no key to be found under.</summary>
        [Test]
        public void ARegistrationWithoutATypeIsReported()
        {
            LogAssert.Expect(LogType.Error, new Regex("without a type"));

            ServiceLocator.Register(null, new ServiceProbe(First));
        }

        /// <summary>A registration without an instance would hand out nothing later.</summary>
        [Test]
        public void ARegistrationWithoutAnInstanceIsReported()
        {
            LogAssert.Expect(LogType.Error, new Regex(nameof(ServiceProbe)));

            ServiceLocator.Register<ServiceProbe>(null);

            Assert.That(ServiceLocator.TryGetOptional(out ServiceProbe _), Is.False);
        }

        /// <summary>
        /// A service whose object was destroyed without deregistering is the case a plain null check
        /// would miss. It has to be reported and the stale entry dropped, not handed out.
        /// </summary>
        [Test]
        public void ADestroyedServiceIsReportedAndCleanedUp()
        {
            ServiceLocator.Register(CreateProbe());
            Object.DestroyImmediate(_probe);

            LogAssert.Expect(LogType.Error, new Regex(nameof(UnityServiceProbe)));

            Assert.That(ServiceLocator.TryGet(out UnityServiceProbe found), Is.False);
            Assert.That(found, Is.Null);
            Assert.That(ServiceLocator.TryGetOptional(out UnityServiceProbe _), Is.False,
                "the stale entry has to be gone, not merely refused");
        }

        /// <summary>A destroyed service is absent for an optional lookup as well.</summary>
        [Test]
        public void ADestroyedServiceIsAbsentForAnOptionalLookup()
        {
            ServiceLocator.Register(CreateProbe());
            Object.DestroyImmediate(_probe);

            Assert.That(ServiceLocator.TryGetOptional(out UnityServiceProbe _), Is.False);

            ServiceLocator.Deregister(typeof(UnityServiceProbe));
        }

        private UnityServiceProbe CreateProbe()
        {
            _probe = ScriptableObject.CreateInstance<UnityServiceProbe>();

            return _probe;
        }

        private static void Forget<T>() where T : class, IGameService
        {
            if (ServiceLocator.TryGetOptional(out T service))
                ServiceLocator.Deregister(service);
        }
    }
}