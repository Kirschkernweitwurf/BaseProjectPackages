using NUnit.Framework;
using UnityEngine;

namespace Base.AudioPackage.Tests
{
    /// <summary>
    /// Everything a container says about playback reaches the source through this one call, so a setting
    /// that is dropped here is a setting that has no effect anywhere in the project.
    /// </summary>
    public sealed class AudioSourceConfiguratorTests
    {
        private const float DefaultPitch = 1f;
        private const int DrawCount = 50;
        private const float MaxPitch = 1.05f;
        private const float MinPitch = 0.95f;
        private const string RootName = "AudioSourceConfiguratorTests";
        private const float Silence = 0f;
        private const float TestVolume = 0.25f;

        private static readonly Vector3 TestPosition = new(3f, -4f, 5f);

        private AudioClip _clip;
        private AudioContainer _container;
        private GameObject _root;
        private AudioSource _source;

        /// <summary>A fresh source, container and clip per test.</summary>
        [SetUp]
        public void Prepare()
        {
            _root = new GameObject(RootName);
            _source = AudioTestObjects.CreateSource(_root.transform);
            _container = AudioTestObjects.CreateContainer();
            _clip = AudioTestObjects.CreateClip(nameof(AudioClip));
        }

        /// <summary>None of these are saved, so they have to be destroyed by hand.</summary>
        [TearDown]
        public void Cleanup()
        {
            if (_root != null)
                Object.DestroyImmediate(_root);

            if (_container != null)
                Object.DestroyImmediate(_container);

            if (_clip != null)
                Object.DestroyImmediate(_clip);

            _root = null;
            _source = null;
            _container = null;
            _clip = null;
        }

        /// <summary>
        /// The clip is picked before the source is taken, so the configurator is what puts the two
        /// together. A source without it would play whatever the previous caller left on it.
        /// </summary>
        [Test]
        public void TheChosenClipEndsUpOnTheSource()
        {
            Apply();

            Assert.That(_source.clip, Is.EqualTo(_clip));
        }

        /// <summary>
        /// Sources come out of a pool, so the volume of whoever used it last is still on it. The
        /// container's volume has to be written every time rather than only when it differs from one.
        /// </summary>
        [Test]
        public void TheContainerVolumeIsWrittenOntoThePooledSource()
        {
            _source.volume = Silence;
            AudioTestObjects.SetVolume(_container, TestVolume);

            Apply();

            Assert.That(_source.volume, Is.EqualTo(TestVolume));
        }

        /// <summary>
        /// A looping container that lands on a source left over from a one shot would stop after one
        /// play, and a UI click on a source that does not ignore the pause would go silent in a menu.
        /// </summary>
        [Test]
        public void LoopingAndPauseBehaviourComeFromTheContainer()
        {
            AudioTestObjects.SetLoop(_container, loop: true);
            AudioTestObjects.SetIgnorePause(_container, ignorePause: true);

            Apply();

            Assert.That(_source.loop, Is.True);
            Assert.That(_source.ignoreListenerPause, Is.True);
        }

        /// <summary>
        /// A three dimensional sound is placed by moving the source, so a position that is not applied
        /// would play the sound wherever the pooled object happened to be parked.
        /// </summary>
        [Test]
        public void TheSourceMovesToTheRequestedPosition()
        {
            Apply();

            Assert.That(_source.transform.position, Is.EqualTo(TestPosition));
        }

        /// <summary>
        /// A container that does not randomize gets the neutral pitch written explicitly, because the
        /// pooled source may still carry a randomized pitch from its last use.
        /// </summary>
        [Test]
        public void AContainerWithoutRandomizationResetsThePitch()
        {
            _source.pitch = MinPitch;

            Apply();

            Assert.That(_source.pitch, Is.EqualTo(DefaultPitch));
        }

        /// <summary>
        /// The randomized pitch is what keeps a repeated sound from sounding identical, and it also
        /// decides how long the release coroutine waits, so it may never leave the configured range.
        /// </summary>
        [Test]
        public void ARandomizedPitchStaysInsideTheConfiguredRange()
        {
            AudioTestObjects.SetRandomizePitch(_container, randomizePitch: true);

            for (int i = 0; i < DrawCount; i++)
            {
                Apply();

                Assert.That(_source.pitch, Is.InRange(MinPitch, MaxPitch));
            }
        }

        /// <summary>A range with both ends on the same value is not random, it is that value.</summary>
        [Test]
        public void ARangeWithBothEndsEqualGivesExactlyThatPitch()
        {
            AudioSourceConfigurator configurator = new(MinPitch, MinPitch);

            AudioTestObjects.SetRandomizePitch(_container, randomizePitch: true);
            configurator.Apply(_source, _container, _clip, TestPosition);

            Assert.That(_source.pitch, Is.EqualTo(MinPitch));
        }

        /// <summary>
        /// Runs the configurator with the pitch range the manager ships with.
        /// </summary>
        private void Apply()
        {
            AudioSourceConfigurator configurator = new(MinPitch, MaxPitch);
            configurator.Apply(_source, _container, _clip, TestPosition);
        }
    }
}