using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Base.AudioPackage.Pool;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Base.AudioPackage.PlayTests
{
    /// <summary>
    /// The manager is the only part of this package a game talks to, and everything it does needs
    /// frames: the release after playback, the fades, and the pool it takes sources from.
    /// <para>
    /// Playback is checked through the manager's own view of what is playing rather than through
    /// <c>AudioSource.isPlaying</c>, because a test runner has no audio device to answer that, and
    /// because the tracking table is what every stop, fade and play limit actually reads.
    /// </para>
    /// </summary>
    public sealed class AudioManagerPlayTests
    {
        private const string ClipName = "PlayTestClip";
        private const int ClipSamples = 400;
        private const string EmptyCollectionMessage = "empty collection";
        private const float FadeDuration = 0.05f;
        private const string ManagerName = "AudioManager";
        private const float MinimumDelay = 0.05f;
        private const string NoClipMessage = "no clip assigned";
        private const string ParentName = "PoolParent";
        private const string PoolManagerName = "AudioPoolManager";
        private const int PrewarmCount = 4;
        private const float ReleaseWait = 0.5f;

        private readonly List<GameObject> _hosts = new();
        private readonly List<Object> _assets = new();

        private AudioManager _manager;
        private AudioContainer _container;
        private Transform _parent;

        /// <summary>Wires a pool manager and a manager together and lets them start.</summary>
        [UnitySetUp]
        public IEnumerator Prepare()
        {
            AudioPoolManager poolManager = CreatePoolManager();

            GameObject host = new(ManagerName);
            host.SetActive(false);
            _hosts.Add(host);

            _manager = host.AddComponent<AudioManager>();

            AudioPlayTestObjects.SetField(_manager, AudioManager.AudioPoolManagerField, poolManager);
            AudioPlayTestObjects.SetField(_manager, AudioManager.MinimumDelayField, MinimumDelay);

            host.SetActive(true);

            _container = CreateContainer(CreateClip());

            yield return null;
        }

        /// <summary>
        /// Hands back everything the test put in the scene. Destroying the manager is what deregisters
        /// it from the locator, so the next test does not find the one before it still standing there.
        /// </summary>
        [TearDown]
        public void Cleanup()
        {
            foreach (GameObject host in _hosts)
            {
                if (host != null)
                    Object.DestroyImmediate(host);
            }

            foreach (Object asset in _assets)
            {
                if (asset != null)
                    Object.DestroyImmediate(asset);
            }

            _hosts.Clear();
            _assets.Clear();

            _manager = null;
            _container = null;
            _parent = null;
        }

        /// <summary>A container that was played reads as playing, and hands back the source it uses.</summary>
        [UnityTest]
        public IEnumerator PlayingAContainerReportsItAsPlaying()
        {
            Assert.That(_manager.IsPlaying(_container), Is.False);

            AudioSource source = _manager.PlaySound(_container, autoStop: false);

            Assert.That(source, Is.Not.Null);
            Assert.That(source.gameObject.activeSelf, Is.True);
            Assert.That(_manager.IsPlaying(_container), Is.True);

            yield return null;
        }

        /// <summary>
        /// A call with nothing to play is a wiring mistake in the caller, so it is reported rather than
        /// taking a source out of the pool and playing silence with it.
        /// </summary>
        [UnityTest]
        public IEnumerator PlayingWithoutAContainerIsReportedAndPlaysNothing()
        {
            LogAssert.Expect(LogType.Error, new Regex(nameof(AudioContainer)));

            Assert.That(_manager.PlaySound(null), Is.Null);
            Assert.That(AudioPlayTestObjects.CountActiveChildren(_parent), Is.EqualTo(0));

            yield return null;
        }

        /// <summary>
        /// A container with an empty clip array is the same mistake one step further in. The draw
        /// reports the empty array and the manager reports the container it came from.
        /// </summary>
        [UnityTest]
        public IEnumerator PlayingAContainerWithoutClipsIsReportedAndPlaysNothing()
        {
            AudioContainer empty = CreateContainer();

            LogAssert.Expect(LogType.Warning, new Regex(EmptyCollectionMessage));
            LogAssert.Expect(LogType.Error, new Regex(NoClipMessage));

            Assert.That(_manager.PlaySound(empty), Is.Null);
            Assert.That(_manager.IsPlaying(empty), Is.False);

            yield return null;
        }

        /// <summary>
        /// Stopping a container has to reach every source playing for it, not just the last one, and
        /// hand each of them back to the pool.
        /// </summary>
        [UnityTest]
        public IEnumerator StoppingAContainerStopsEverySourceOfIt()
        {
            _manager.PlaySound(_container, autoStop: false);
            _manager.PlaySound(_container, autoStop: false);

            Assert.That(AudioPlayTestObjects.CountActiveChildren(_parent), Is.EqualTo(2));

            _manager.StopSound(_container);

            Assert.That(_manager.IsPlaying(_container), Is.False);
            Assert.That(AudioPlayTestObjects.CountActiveChildren(_parent), Is.EqualTo(0));

            yield return null;
        }

        /// <summary>
        /// Without a limit a container stacks. This is the baseline the limited case is measured
        /// against, so a limit that does nothing cannot pass as one that works.
        /// </summary>
        [UnityTest]
        public IEnumerator AnUnlimitedContainerStacks()
        {
            _manager.PlaySound(_container, autoStop: false);
            _manager.PlaySound(_container, autoStop: false);
            _manager.PlaySound(_container, autoStop: false);

            Assert.That(AudioPlayTestObjects.CountActiveChildren(_parent), Is.EqualTo(3));

            yield return null;
        }

        /// <summary>
        /// A limit releases the oldest source rather than refusing the new sound, so a rapid fire effect
        /// keeps sounding without piling up voices.
        /// </summary>
        [UnityTest]
        public IEnumerator ThePlayLimitReleasesTheOldest()
        {
            AudioPlayTestObjects.SetProperty(_container, nameof(AudioContainer.MaxClipsPlaying), 1);

            _manager.PlaySound(_container, autoStop: false);
            _manager.PlaySound(_container, autoStop: false);
            _manager.PlaySound(_container, autoStop: false);

            Assert.That(AudioPlayTestObjects.CountActiveChildren(_parent), Is.EqualTo(1));
            Assert.That(_manager.IsPlaying(_container), Is.True);

            yield return null;
        }

        /// <summary>Stopping everything reaches containers the caller no longer holds a reference to.</summary>
        [UnityTest]
        public IEnumerator StoppingEverythingStopsEveryContainer()
        {
            AudioContainer other = CreateContainer(CreateClip());

            _manager.PlaySound(_container, autoStop: false);
            _manager.PlaySound(other, autoStop: false);

            _manager.StopAll();

            Assert.That(_manager.IsPlaying(_container), Is.False);
            Assert.That(_manager.IsPlaying(other), Is.False);
            Assert.That(AudioPlayTestObjects.CountActiveChildren(_parent), Is.EqualTo(0));

            yield return null;
        }

        /// <summary>
        /// Stopping a source the caller no longer holds is reported rather than throwing, since the call
        /// comes from game code that cannot know the pool already took it back.
        /// </summary>
        [UnityTest]
        public IEnumerator StoppingANullSourceIsReportedAndSurvived()
        {
            LogAssert.Expect(LogType.Warning, new Regex(nameof(AudioSource)));

            Assert.DoesNotThrow(() => _manager.StopSound((AudioSource)null));

            yield return null;
        }

        /// <summary>
        /// A fade out ends by handing the source back, so a menu that fades its music out does not leave
        /// a silent source checked out of the pool forever.
        /// </summary>
        [UnityTest]
        public IEnumerator AFadeOutReleasesTheSource()
        {
            AudioSource source = _manager.PlaySound(_container, autoStop: false);

            yield return _manager.FadeOut(source, FadeDuration);

            Assert.That(_manager.IsPlaying(_container), Is.False);
            Assert.That(source.gameObject.activeSelf, Is.False);
        }

        /// <summary>
        /// The default is that a one shot cleans up after itself. Without that, every sound a game plays
        /// would hold its source until something stopped it by hand.
        /// </summary>
        [UnityTest]
        public IEnumerator AOneShotReleasesItselfWhenTheClipIsOver()
        {
            _manager.PlaySound(_container);

            Assert.That(_manager.IsPlaying(_container), Is.True);

            yield return new WaitForSeconds(ReleaseWait);

            Assert.That(_manager.IsPlaying(_container), Is.False);
            Assert.That(AudioPlayTestObjects.CountActiveChildren(_parent), Is.EqualTo(0));
        }

        /// <summary>
        /// A looping container is never released on its own, so a background track keeps going until
        /// something stops or fades it.
        /// </summary>
        [UnityTest]
        public IEnumerator ALoopingContainerIsNeverReleasedOnItsOwn()
        {
            AudioPlayTestObjects.SetProperty(_container, nameof(AudioContainer.Loop), true);

            _manager.PlaySound(_container);

            yield return new WaitForSeconds(ReleaseWait);

            Assert.That(_manager.IsPlaying(_container), Is.True);
        }

        /// <summary>Creates a clip and remembers it, so the teardown destroys it.</summary>
        private AudioClip CreateClip()
        {
            AudioClip clip = AudioPlayTestObjects.CreateClip(ClipName, ClipSamples);
            _assets.Add(clip);

            return clip;
        }

        /// <summary>Creates a container holding the given clips and remembers it for the teardown.</summary>
        /// <param name="clips">The clips to assign. Pass none for an empty container.</param>
        /// <returns>The new container.</returns>
        private AudioContainer CreateContainer(params AudioClip[] clips)
        {
            AudioContainer container = AudioPlayTestObjects.CreateContainer();
            _assets.Add(container);

            AudioPlayTestObjects.SetProperty(container, nameof(AudioContainer.Clips), clips);

            return container;
        }

        /// <summary>
        /// Builds a fully wired pool manager on an object that is still switched off, so its pools exist
        /// by the time the manager asks for a source.
        /// </summary>
        /// <returns>The running pool manager.</returns>
        private AudioPoolManager CreatePoolManager()
        {
            GameObject host = new(PoolManagerName);
            host.SetActive(false);
            _hosts.Add(host);

            AudioPoolManager poolManager = host.AddComponent<AudioPoolManager>();

            GameObject parentHost = new(ParentName);
            _hosts.Add(parentHost);
            _parent = parentHost.transform;

            AudioPlayTestObjects.SetField(poolManager, AudioPoolManager.PrewarmCountField, PrewarmCount);
            AudioPlayTestObjects.SetField(poolManager, AudioPoolManager.PoolParentField, _parent);

            AudioPlayTestObjects.SetField(poolManager, AudioPoolManager.AudioSource2DPrefabField,
                AudioPlayTestObjects.CreatePrefab(nameof(EAudioType.Sfx2D), _hosts));

            AudioPlayTestObjects.SetField(poolManager, AudioPoolManager.AudioSource3DPrefabField,
                AudioPlayTestObjects.CreatePrefab(nameof(EAudioType.Sfx3D), _hosts));

            AudioPlayTestObjects.SetField(poolManager, AudioPoolManager.AudioSourceMusicPrefabField,
                AudioPlayTestObjects.CreatePrefab(nameof(EAudioType.Music), _hosts));

            AudioPlayTestObjects.SetField(poolManager, AudioPoolManager.AudioSourceUiPrefabField,
                AudioPlayTestObjects.CreatePrefab(nameof(EAudioType.Ui), _hosts));

            host.SetActive(true);

            return poolManager;
        }
    }
}