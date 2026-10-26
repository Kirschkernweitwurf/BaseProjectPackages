using System.Text.RegularExpressions;
using Base.ServicesPackage;
using Base.TweeningPackage.Core;
using Base.TweeningPackage.Core.Data;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Base.TweeningPackage.Tests
{
    /// <summary>
    /// Covers where a timeline places its tweens and callbacks, that a placed tween reads its start value when the
    /// timeline reaches it, and that loops and completing work on the whole timeline. Every step is driven by hand.
    /// </summary>
    public sealed class TweenTimelineTests
    {
        private const float Duration = 1f;
        private const float Half = 0.5f;
        private const float Tolerance = 0.0001f;

        private GameObject _runnerObject;
        private Transform _a;
        private Transform _b;

        /// <summary>Registers a runner once, so starting a tween resolves one.</summary>
        [OneTimeSetUp]
        public void BuildRunner()
        {
            _runnerObject = EditorUtility.CreateGameObjectWithHideFlags(nameof(TweenRunner),
                HideFlags.HideAndDontSave);

            ServiceLocator.Register(_runnerObject.AddComponent<TweenRunner>());
        }

        /// <summary>Takes the runner back down once the fixture is done.</summary>
        [OneTimeTearDown]
        public void ReleaseRunner()
        {
            if (ServiceLocator.TryGetOptional(out TweenRunner runner))
                ServiceLocator.Deregister(runner);

            if (_runnerObject != null)
                Object.DestroyImmediate(_runnerObject);
        }

        /// <summary>Every test starts from two fresh transforms at scale 0.</summary>
        [SetUp]
        public void Build()
        {
            _a = Make("A");
            _b = Make("B");
        }

        /// <summary>Takes both transforms back down.</summary>
        [TearDown]
        public void Release()
        {
            Object.DestroyImmediate(_a.gameObject);
            Object.DestroyImmediate(_b.gameObject);
        }

        /// <summary>Appended tweens run one after another.</summary>
        [Test]
        public void AppendedTweensRunOneAfterAnother()
        {
            TweenTimeline timeline = Tweens.Timeline()
                .Append(Scale(_a, 1f))
                .Append(Scale(_b, 1f));

            timeline.Tick(Duration * Half);
            Assert.That(_a.localScale.x, Is.EqualTo(Half).Within(Tolerance));
            Assert.That(_b.localScale.x, Is.EqualTo(0f));

            timeline.Tick(Duration);
            Assert.That(_a.localScale.x, Is.EqualTo(1f));
            Assert.That(_b.localScale.x, Is.EqualTo(Half).Within(Tolerance));
            Assert.That(timeline.Duration, Is.EqualTo(Duration * 2f).Within(Tolerance));
        }

        /// <summary>A joined tween runs alongside the one before it.</summary>
        [Test]
        public void AJoinedTweenRunsAlongside()
        {
            TweenTimeline timeline = Tweens.Timeline()
                .Append(Scale(_a, 1f))
                .Join(Scale(_b, 1f));

            timeline.Tick(Duration * Half);

            Assert.That(_b.localScale.x, Is.EqualTo(Half).Within(Tolerance));
            Assert.That(timeline.Duration, Is.EqualTo(Duration).Within(Tolerance));
        }

        /// <summary>An inserted tween starts at its time.</summary>
        [Test]
        public void AnInsertedTweenStartsAtItsTime()
        {
            TweenTimeline timeline = Tweens.Timeline().Insert(Half, Scale(_a, 1f));

            timeline.Tick(Half * Half);
            Assert.That(_a.localScale.x, Is.EqualTo(0f));

            timeline.Tick(Half);
            Assert.That(_a.localScale.x, Is.EqualTo(Half * Half).Within(Tolerance));
        }

        /// <summary>A prepended tween runs first and moves the rest back.</summary>
        [Test]
        public void APrependedTweenRunsFirst()
        {
            TweenTimeline timeline = Tweens.Timeline()
                .Append(Scale(_a, 1f))
                .Prepend(Scale(_b, 1f));

            timeline.Tick(Duration * Half);

            Assert.That(_b.localScale.x, Is.EqualTo(Half).Within(Tolerance));
            Assert.That(_a.localScale.x, Is.EqualTo(0f));
        }

        /// <summary>A tween placed after another reads its start value when the timeline reaches it.</summary>
        [Test]
        public void ALaterTweenStartsFromWhereTheEarlierOneLeft()
        {
            TweenTimeline timeline = Tweens.Timeline()
                .Append(Scale(_a, 1f))
                .Append(Scale(_a, 2f));

            timeline.Tick(Duration + Duration * Half);

            Assert.That(_a.localScale.x, Is.EqualTo(1f + Half).Within(Tolerance));
        }

        /// <summary>A callback runs once, when the timeline reaches it.</summary>
        [Test]
        public void ACallbackRunsOnceAtItsTime()
        {
            int calls = 0;
            TweenTimeline timeline = Tweens.Timeline()
                .Append(Scale(_a, 1f))
                .AppendCallback(() => calls++)
                .AppendInterval(Duration);

            timeline.Tick(Duration * Half);
            Assert.That(calls, Is.EqualTo(0));

            timeline.Tick(Duration);
            timeline.Tick(Duration * Half);
            Assert.That(calls, Is.EqualTo(1));
        }

        /// <summary>Back and forth plays the timeline backward on every second loop.</summary>
        [Test]
        public void PingPongPlaysBackward()
        {
            TweenTimeline timeline = Tweens.Timeline()
                .Join(Scale(_a, 1f))
                .SetLoops(-1, ELoopType.PingPong);

            timeline.Tick(Duration);
            timeline.Tick(Duration * Half);
            Assert.That(_a.localScale.x, Is.EqualTo(Half).Within(Tolerance));

            timeline.Tick(Duration * Half);
            Assert.That(_a.localScale.x, Is.EqualTo(0f).Within(Tolerance));
            timeline.Stop();
        }

        /// <summary>Completing puts every tween on its end and tells each one it finished.</summary>
        [Test]
        public void CompletingPutsEveryTweenOnItsEnd()
        {
            bool isChildDone = false;
            TweenTimeline timeline = Tweens.Timeline()
                .Append(Scale(_a, 1f))
                .Append(Scale(_b, 1f).WhenComplete(() => isChildDone = true));

            timeline.Stop(complete: true);

            Assert.That(_a.localScale.x, Is.EqualTo(1f));
            Assert.That(_b.localScale.x, Is.EqualTo(1f));
            Assert.That(isChildDone, Is.True);
        }

        /// <summary>A tween with endless loops has no end and stays off the timeline.</summary>
        [Test]
        public void AnEndlessTweenIsRefused()
        {
            LogAssert.Expect(LogType.Warning, new Regex("endless"));

            TweenTimeline timeline = Tweens.Timeline().Append(Scale(_a, 1f).SetLoops(-1));

            Assert.That(timeline.Duration, Is.EqualTo(0f));
            timeline.Stop();
        }

        private static Transform Make(string name)
        {
            Transform transform = EditorUtility.CreateGameObjectWithHideFlags(name, HideFlags.HideAndDontSave)
                .transform;

            transform.localScale = Vector3.zero;
            return transform;
        }

        private static Tween<Vector3> Scale(Transform transform, float to)
            => transform.TweenScale(to, Duration).SetEase(EEasingType.Linear);
    }
}
