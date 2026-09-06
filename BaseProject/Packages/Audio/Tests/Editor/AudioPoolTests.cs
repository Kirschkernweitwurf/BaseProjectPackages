using System.Text.RegularExpressions;
using Base.AudioPackage.Pool;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Base.AudioPackage.Tests
{
    /// <summary>
    /// The pool is what keeps playback from instantiating a source per sound. A prewarm that creates the
    /// wrong number, or a take that builds a new instance while one is sitting free, turns into a hitch
    /// in the frame the first sounds play in, which is exactly where it is hardest to notice.
    /// </summary>
    public sealed class AudioPoolTests
    {
        private const string CreationFailure = "Failed to create a new instance";
        private const string MissingPrefab = "cannot create instances";
        private const string ParentName = "PoolParent";
        private const string PrefabName = "AudioSourcePrefab";
        private const int PrewarmCount = 3;
        private const string RootName = "AudioPoolTests";
        private const EAudioType TestAudioType = EAudioType.Sfx2D;

        private AudioSource _prefab;
        private Transform _poolParent;
        private GameObject _root;

        /// <summary>
        /// A prefab and a pool parent that are siblings, so the parent's child count is a clean measure of
        /// how many instances the pool made.
        /// </summary>
        [SetUp]
        public void Prepare()
        {
            _root = new GameObject(RootName);

            GameObject prefabHost = new(PrefabName);
            prefabHost.transform.SetParent(_root.transform);
            _prefab = prefabHost.AddComponent<AudioSource>();

            GameObject parentHost = new(ParentName);
            parentHost.transform.SetParent(_root.transform);
            _poolParent = parentHost.transform;
        }

        /// <summary>One root holds the prefab, the parent and every instance the pool made.</summary>
        [TearDown]
        public void Cleanup()
        {
            if (_root != null)
                Object.DestroyImmediate(_root);

            _root = null;
            _prefab = null;
            _poolParent = null;
        }

        /// <summary>
        /// Prewarming is the whole point of the pool: the instances exist before the first sound plays
        /// rather than being built during it.
        /// </summary>
        /// <param name="prewarmCount">How many instances the pool is asked to create up front.</param>
        [TestCase(1)]
        [TestCase(4)]
        public void APoolCreatesExactlyAsManyInstancesAsItIsAskedFor(int prewarmCount)
        {
            CreatePool(prewarmCount);

            Assert.That(_poolParent.childCount, Is.EqualTo(prewarmCount));
        }

        /// <summary>
        /// A prewarm count of zero is a valid setting for a type that rarely plays, and it has to build
        /// nothing rather than falling through into a loop that never ends.
        /// </summary>
        [Test]
        public void APrewarmCountOfZeroCreatesNothing()
        {
            CreatePool(0);

            Assert.That(_poolParent.childCount, Is.EqualTo(0));
        }

        /// <summary>
        /// Prewarmed instances are taken and released again, so they have to end up parked and silent.
        /// An active one would be a source nobody owns sitting in the scene.
        /// </summary>
        [Test]
        public void PrewarmedInstancesWaitDeactivated()
        {
            CreatePool(PrewarmCount);

            for (int i = 0; i < _poolParent.childCount; i++)
                Assert.That(_poolParent.GetChild(i).gameObject.activeSelf, Is.False);
        }

        /// <summary>Instances belong under the given parent, not loose in the scene root.</summary>
        [Test]
        public void InstancesAreParentedUnderThePoolParent()
        {
            AudioPool pool = CreatePool(0);
            AudioSource source = pool.Get();

            Assert.That(source.transform.parent, Is.EqualTo(_poolParent));
        }

        /// <summary>A source that was handed out is active, otherwise it could not play anything.</summary>
        [Test]
        public void ATakenSourceIsActive()
        {
            AudioPool pool = CreatePool(PrewarmCount);
            AudioSource source = pool.Get();

            Assert.That(source.gameObject.activeSelf, Is.True);
        }

        /// <summary>
        /// Every take that lands inside the prewarmed set has to reuse one, and every take has to hand out
        /// an instance that nobody else is holding.
        /// </summary>
        [Test]
        public void TakingUpToThePrewarmCountReusesInstancesAndNeverRepeatsOne()
        {
            AudioPool pool = CreatePool(PrewarmCount);
            AudioSource[] taken = new AudioSource[PrewarmCount];

            for (int i = 0; i < PrewarmCount; i++)
                taken[i] = pool.Get();

            Assert.That(_poolParent.childCount, Is.EqualTo(PrewarmCount));
            Assert.That(taken, Is.Unique);
        }

        /// <summary>
        /// More sounds than were prewarmed is a normal peak rather than an error, so the pool grows
        /// instead of returning nothing.
        /// </summary>
        [Test]
        public void TakingMoreThanWerePrewarmedGrowsThePool()
        {
            AudioPool pool = CreatePool(PrewarmCount);

            for (int i = 0; i < PrewarmCount + 1; i++)
                pool.Get();

            Assert.That(_poolParent.childCount, Is.EqualTo(PrewarmCount + 1));
        }

        /// <summary>A released source is parked again, so it neither plays nor shows up as in use.</summary>
        [Test]
        public void AReleasedSourceIsDeactivated()
        {
            AudioPool pool = CreatePool(0);
            AudioSource source = pool.Get();

            pool.Release(source);

            Assert.That(source.gameObject.activeSelf, Is.False);
        }

        /// <summary>
        /// The free instance is the one the next take has to get. Building a second one while the first
        /// sits available is the leak the pool exists to prevent.
        /// </summary>
        [Test]
        public void AReleasedSourceIsTheOneHandedOutNext()
        {
            AudioPool pool = CreatePool(0);
            AudioSource first = pool.Get();

            pool.Release(first);

            Assert.That(pool.Get(), Is.EqualTo(first));
            Assert.That(_poolParent.childCount, Is.EqualTo(1));
        }

        /// <summary>Clearing parks every source that was handed out, in one call.</summary>
        [Test]
        public void ReleasingEverythingParksEverySourceThatWasHandedOut()
        {
            AudioPool pool = CreatePool(PrewarmCount);

            for (int i = 0; i < PrewarmCount; i++)
                pool.Get();

            pool.ReleaseAll();

            for (int i = 0; i < _poolParent.childCount; i++)
                Assert.That(_poolParent.GetChild(i).gameObject.activeSelf, Is.False);
        }

        /// <summary>
        /// Releasing nothing is a caller bug rather than a lifecycle event, so it is reported by name and
        /// swallowed instead of reaching the pool and throwing there.
        /// </summary>
        [Test]
        public void ReleasingNothingIsReportedAndSurvived()
        {
            AudioPool pool = CreatePool(0);

            LogAssert.Expect(LogType.Warning, new Regex(nameof(AudioSource)));

            Assert.DoesNotThrow(() => pool.Release(null));
        }

        /// <summary>
        /// A scene load can destroy a source while something still holds it. That reaches the pool as a
        /// fake null, which only the explicit comparison catches.
        /// </summary>
        [Test]
        public void ReleasingADestroyedSourceIsReportedAndSurvived()
        {
            AudioPool pool = CreatePool(0);
            AudioSource source = pool.Get();

            Object.DestroyImmediate(source.gameObject);
            LogAssert.Expect(LogType.Warning, new Regex(nameof(AudioSource)));

            Assert.DoesNotThrow(() => pool.Release(source));
        }

        /// <summary>
        /// Without a prefab the pool can never hand anything out, so it says so once when it is built and
        /// again when something asks it for a source, rather than returning null in silence.
        /// </summary>
        [Test]
        public void APoolWithoutAPrefabReportsItAndHandsNothingOut()
        {
            LogAssert.Expect(LogType.Error, new Regex(MissingPrefab));
            LogAssert.Expect(LogType.Error, new Regex(CreationFailure));

            AudioPool pool = new(TestAudioType, null, _poolParent, 0);

            Assert.That(pool.Get(), Is.Null);
        }

        /// <summary>
        /// Builds a pool around the shared prefab and parent.
        /// </summary>
        /// <param name="prewarmCount">How many instances to create up front.</param>
        /// <returns>The new pool.</returns>
        private AudioPool CreatePool(int prewarmCount)
            => new(TestAudioType, _prefab, _poolParent, prewarmCount);
    }
}