using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Base.AudioPackage.Tests
{
    /// <summary>
    /// The container is the only place a designer sets playback up, so a wrong default ships silently in
    /// every asset made from that point on. The defaults, the unlimited flag the play limit reads and
    /// the clip draw are covered here.
    /// </summary>
    public sealed class AudioContainerTests
    {
        private const int DrawCount = 50;
        private const string EmptyCollectionMessage = "empty collection";
        private const string FirstClipName = "Alpha";
        private const float FullVolume = 1f;
        private const float NoDelay = 0f;
        private const string SecondClipName = "Beta";
        private const float TestVolume = 0.25f;
        private const string ThirdClipName = "Gamma";

        private readonly List<AudioClip> _clips = new();

        private AudioContainer _container;

        /// <summary>A fresh asset per test, so one test's edit cannot decide the next one's outcome.</summary>
        [SetUp]
        public void Prepare() => _container = AudioTestObjects.CreateContainer();

        /// <summary>Nothing here is saved, so the instances have to be destroyed by hand.</summary>
        [TearDown]
        public void Cleanup()
        {
            foreach (AudioClip clip in _clips)
            {
                if (clip != null)
                    Object.DestroyImmediate(clip);
            }

            _clips.Clear();

            if (_container != null)
                Object.DestroyImmediate(_container);

            _container = null;
        }

        /// <summary>
        /// A new container is the one a designer drags a clip onto and expects to hear. Anything below
        /// full volume would make that first test play quieter than the clip was authored.
        /// </summary>
        [Test]
        public void AFreshContainerPlaysAtFullVolume()
            => Assert.That(_container.Volume, Is.EqualTo(FullVolume));

        /// <summary>A delay nobody asked for would push every sound behind the action that caused it.</summary>
        [Test]
        public void AFreshContainerPlaysImmediately()
            => Assert.That(_container.Delay, Is.EqualTo(NoDelay));

        /// <summary>A fresh container neither loops nor outlives a pause on its own.</summary>
        [Test]
        public void AFreshContainerNeitherLoopsNorIgnoresPause()
        {
            Assert.That(_container.Loop, Is.False);
            Assert.That(_container.IgnorePause, Is.False);
        }

        /// <summary>
        /// The play limit releases the oldest source once it is reached, so a limit that applies by
        /// default would cut sounds off in a project that never asked for one.
        /// </summary>
        [Test]
        public void AFreshContainerHasNoPlayLimit()
            => Assert.That(_container.HasUnlimitedClips, Is.True);

        /// <summary>Any limit from zero upwards is a real limit the manager has to enforce.</summary>
        /// <param name="maxClipsPlaying">The limit written to the container.</param>
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(8)]
        public void ALimitOfZeroOrMoreIsARealLimit(int maxClipsPlaying)
        {
            AudioTestObjects.SetMaxClipsPlaying(_container, maxClipsPlaying);

            Assert.That(_container.HasUnlimitedClips, Is.False);
            Assert.That(_container.MaxClipsPlaying, Is.EqualTo(maxClipsPlaying));
        }

        /// <summary>
        /// Minus one is how the inspector spells unlimited, and the field clamps at minus one, so every
        /// negative value has to mean the same thing rather than only the one the tooltip names.
        /// </summary>
        /// <param name="maxClipsPlaying">The limit written to the container.</param>
        [TestCase(-1)]
        [TestCase(-4)]
        public void ANegativeLimitMeansUnlimited(int maxClipsPlaying)
        {
            AudioTestObjects.SetMaxClipsPlaying(_container, maxClipsPlaying);

            Assert.That(_container.HasUnlimitedClips, Is.True);
        }

        /// <summary>A single clip is the common case, and it has to come back every single time.</summary>
        [Test]
        public void AContainerWithOneClipAlwaysReturnsThatClip()
        {
            AudioClip clip = CreateClip(FirstClipName);

            AudioTestObjects.SetClips(_container, clip);

            for (int i = 0; i < DrawCount; i++)
                Assert.That(_container.GetRandomClip(), Is.EqualTo(clip));
        }

        /// <summary>
        /// The draw is random, so what can be asserted is that it never leaves the array. An off by one
        /// in the range would show up here as a null or as an index that is not in the container.
        /// </summary>
        [Test]
        public void AContainerOnlyEverReturnsAClipItHolds()
        {
            AudioClip[] clips =
            {
                CreateClip(FirstClipName),
                CreateClip(SecondClipName),
                CreateClip(ThirdClipName)
            };

            AudioTestObjects.SetClips(_container, clips);

            for (int i = 0; i < DrawCount; i++)
                Assert.That(clips, Has.Member(_container.GetRandomClip()));
        }

        /// <summary>
        /// A container with an empty array is a wiring mistake, so it reports itself rather than playing
        /// nothing quietly. The caller checks for the null it gets back.
        /// </summary>
        [Test]
        public void AnEmptyContainerReturnsNothingAndSaysSo()
        {
            AudioTestObjects.SetClips(_container);
            LogAssert.Expect(LogType.Warning, new Regex(EmptyCollectionMessage));

            Assert.That(_container.GetRandomClip(), Is.Null);
        }

        /// <summary>
        /// Every playback setting is an auto property behind a serialized backing field. If a rename ever
        /// split the two apart, the inspector would write to a field nothing reads.
        /// </summary>
        [Test]
        public void TheInspectorFieldsFeedThePropertiesTheManagerReads()
        {
            AudioTestObjects.SetVolume(_container, TestVolume);
            AudioTestObjects.SetLoop(_container, loop: true);
            AudioTestObjects.SetIgnorePause(_container, ignorePause: true);
            AudioTestObjects.SetRandomizePitch(_container, randomizePitch: true);

            Assert.That(_container.Volume, Is.EqualTo(TestVolume));
            Assert.That(_container.Loop, Is.True);
            Assert.That(_container.IgnorePause, Is.True);
            Assert.That(_container.RandomizePitch, Is.True);
        }

        /// <summary>
        /// Creates a clip and keeps it, so the teardown destroys it rather than leaving it in memory.
        /// </summary>
        /// <param name="name">The clip name.</param>
        /// <returns>The new clip.</returns>
        private AudioClip CreateClip(string name)
        {
            AudioClip clip = AudioTestObjects.CreateClip(name);
            _clips.Add(clip);

            return clip;
        }
    }
}