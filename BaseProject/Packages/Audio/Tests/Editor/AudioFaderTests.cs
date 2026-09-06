using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Base.AudioPackage.Tests
{
    /// <summary>
    /// The fade itself needs frames to run, which edit mode does not hand out. What is covered here is
    /// everything the fade decides before it starts stepping: the shortcut for a duration that is not a
    /// duration, and the two ways the source can be gone by the time the coroutine reaches it.
    /// </summary>
    public sealed class AudioFaderTests
    {
        private const float FullVolume = 1f;
        private const float HalfVolume = 0.5f;
        private const float NegativeDuration = -1f;
        private const float NoDuration = 0f;
        private const string RootName = "AudioFaderTests";
        private const float Silence = 0f;
        private const float StepDuration = 1f;

        private GameObject _root;
        private AudioSource _source;

        /// <summary>A fresh source per test, since a fade writes to it.</summary>
        [SetUp]
        public void Prepare()
        {
            _root = EditorUtility.CreateGameObjectWithHideFlags(RootName,
                HideFlags.HideAndDontSave);
            _source = AudioTestObjects.CreateSource(_root.transform);
        }

        /// <summary>The source is a real scene object, so it has to be destroyed by hand.</summary>
        [TearDown]
        public void Cleanup()
        {
            if (_root != null)
                Object.DestroyImmediate(_root);

            _root = null;
            _source = null;
        }

        /// <summary>
        /// A fade out that is started on a source the pool already reclaimed has nothing to fade, and it
        /// runs on the manager rather than on the source, so it has to end itself instead of throwing.
        /// </summary>
        [Test]
        public void AFadeOnANullSourceEndsImmediately()
        {
            IEnumerator fade = AudioFader.To(null, Silence, StepDuration);

            Assert.That(fade.MoveNext(), Is.False);
        }

        /// <summary>
        /// A scene load destroys pooled sources while a fade is still holding one. Unity reports that as
        /// a fake null, which only the explicit comparison in the fader catches.
        /// </summary>
        [Test]
        public void AFadeOnADestroyedSourceEndsImmediately()
        {
            Object.DestroyImmediate(_source.gameObject);
            IEnumerator fade = AudioFader.To(_source, Silence, StepDuration);

            Assert.That(fade.MoveNext(), Is.False);
        }

        /// <summary>
        /// A duration of zero is how a caller asks for the end state right now. Stepping it over frames
        /// would divide by zero, so it has to take the shortcut and finish in one step.
        /// </summary>
        [Test]
        public void AZeroDurationReachesTheTargetInOneStep()
        {
            _source.volume = FullVolume;
            IEnumerator fade = AudioFader.To(_source, Silence, NoDuration);

            Assert.That(fade.MoveNext(), Is.False);
            Assert.That(_source.volume, Is.EqualTo(Silence));
        }

        /// <summary>A negative duration is not a duration either, so it takes the same shortcut.</summary>
        [Test]
        public void ANegativeDurationReachesTheTargetInOneStep()
        {
            _source.volume = FullVolume;
            IEnumerator fade = AudioFader.To(_source, HalfVolume, NegativeDuration);

            Assert.That(fade.MoveNext(), Is.False);
            Assert.That(_source.volume, Is.EqualTo(HalfVolume));
        }

        /// <summary>The shortcut goes up as well as down, so a fade in is not a special case.</summary>
        [Test]
        public void AZeroDurationRaisesTheVolumeAsWell()
        {
            _source.volume = Silence;
            IEnumerator fade = AudioFader.To(_source, FullVolume, NoDuration);

            fade.MoveNext();

            Assert.That(_source.volume, Is.EqualTo(FullVolume));
        }

        /// <summary>
        /// A target outside the audible range comes from arithmetic on a settings value somewhere, so it
        /// is pulled back in rather than passed on.
        /// </summary>
        /// <param name="targetVolume">The requested target volume.</param>
        /// <param name="expected">The volume the source has to end up at.</param>
        [TestCase(2f, FullVolume)]
        [TestCase(-1f, Silence)]
        public void ATargetOutsideTheAudibleRangeIsClamped(float targetVolume, float expected)
        {
            _source.volume = HalfVolume;
            IEnumerator fade = AudioFader.To(_source, targetVolume, NoDuration);

            fade.MoveNext();

            Assert.That(_source.volume, Is.EqualTo(expected));
        }

        /// <summary>
        /// A real fade has to hand control back rather than finish on its first step, otherwise the whole
        /// tween would collapse into a single jump to the target.
        /// </summary>
        [Test]
        public void ARealDurationYieldsInsteadOfFinishing()
        {
            IEnumerator fade = AudioFader.To(_source, Silence, StepDuration, ignoreTimeScale: true);

            Assert.That(fade.MoveNext(), Is.True);
        }
    }
}