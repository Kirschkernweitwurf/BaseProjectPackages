using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Base.ServicesPackage.PlayTests
{
    /// <summary>
    /// Covers the half of the locator contract that only exists once frames are running: registration
    /// happens in a Unity callback, and <c>Destroy</c> does not take effect until the frame ends, so
    /// an entry stays live for the rest of the frame in which its object was destroyed.
    /// </summary>
    public sealed class GameServiceBehaviourPlayTests
    {
        private const string AlreadyRegistered = "already registered";

        private readonly List<GameObject> _hosts = new();

        /// <summary>
        /// Destroys immediately rather than deferring, so each service deregisters itself before the
        /// next test runs instead of at the end of a frame that test will never see.
        /// </summary>
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

        /// <summary>A service files itself under its own type as soon as its object wakes up.</summary>
        [UnityTest]
        public IEnumerator AServiceRegistersItselfWhenItsObjectIsCreated()
        {
            ServiceBehaviourProbe probe = CreateProbe();

            yield return null;

            Assert.That(ServiceLocator.TryGetOptional(out ServiceBehaviourProbe found), Is.True);
            Assert.That(found, Is.SameAs(probe));
        }

        /// <summary>
        /// The entry outlives the <c>Destroy</c> call by the rest of the frame, because Unity runs
        /// <c>OnDestroy</c> at the end of it. Anything resolving a service mid-frame depends on this.
        /// </summary>
        [UnityTest]
        public IEnumerator ADestroyedServiceIsStillRegisteredForTheRestOfTheFrame()
        {
            ServiceBehaviourProbe probe = CreateProbe();

            yield return null;

            Object.Destroy(probe.gameObject);

            Assert.That(ServiceLocator.TryGetOptional(out ServiceBehaviourProbe _), Is.True);
        }

        /// <summary>Once the frame is over the object is gone and so is its entry.</summary>
        [UnityTest]
        public IEnumerator ADestroyedServiceIsDeregisteredOnTheNextFrame()
        {
            ServiceBehaviourProbe probe = CreateProbe();

            yield return null;

            Object.Destroy(probe.gameObject);

            yield return null;

            Assert.That(ServiceLocator.TryGetOptional(out ServiceBehaviourProbe _), Is.False);
        }

        /// <summary>
        /// The case the deregistration guard exists for: during a scene reload the replacement wakes
        /// up before the outgoing instance is destroyed, so the late <c>OnDestroy</c> must not wipe it.
        /// </summary>
        [UnityTest]
        public IEnumerator AReplacementSurvivesTheOutgoingInstanceBeingDestroyed()
        {
            ServiceBehaviourProbe outgoing = CreateProbe();

            yield return null;

            LogAssert.Expect(LogType.Warning, new Regex(AlreadyRegistered));
            ServiceBehaviourProbe replacement = CreateProbe();

            yield return null;

            Object.Destroy(outgoing.gameObject);

            yield return null;

            Assert.That(ServiceLocator.TryGetOptional(out ServiceBehaviourProbe found), Is.True);
            Assert.That(found, Is.SameAs(replacement));
        }

        /// <summary>Creates a hosted probe and remembers the object so the teardown can clean it up.</summary>
        private ServiceBehaviourProbe CreateProbe()
        {
            GameObject host = new(nameof(ServiceBehaviourProbe));
            _hosts.Add(host);

            return host.AddComponent<ServiceBehaviourProbe>();
        }
    }
}