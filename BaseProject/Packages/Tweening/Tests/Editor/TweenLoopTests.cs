using Base.ServicesPackage;
using Base.TweeningPackage.Core;
using Base.TweeningPackage.Core.Data;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.TweeningPackage.Tests
{
    /// <summary>
    /// Covers the options a single tween takes in code: loops in every style, easing with an overshoot, and how
    /// long the tween says it runs. Every step is driven by hand.
    /// </summary>
    public sealed class TweenLoopTests
    {
        private const float Duration = 1f;
        private const float EndValue = 10f;
        private const float Half = 0.5f;
        private const float StartValue = 0f;
        private const float Tolerance = 0.0001f;

        private GameObject _runnerObject;
        private GameObject _target;
        private float _value;

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

        /// <summary>Every test starts from a fresh target.</summary>
        [SetUp]
        public void Build()
        {
            _target = EditorUtility.CreateGameObjectWithHideFlags(nameof(TweenLoopTests),
                HideFlags.HideAndDontSave);
            _value = float.NaN;
        }

        /// <summary>Takes the target back down between tests.</summary>
        [TearDown]
        public void Release()
        {
            if (_target != null)
                Object.DestroyImmediate(_target);
        }

        /// <summary>Back and forth twice goes there and back, and ends exactly on the start.</summary>
        [Test]
        public void PingPongTwiceEndsOnTheStart()
        {
            Tween<float> tween = BuildTween().SetLoops(2, ELoopType.PingPong);
            tween.Start();

            tween.Tick(Duration);
            Assert.That(_value, Is.EqualTo(EndValue).Within(Tolerance));

            tween.Tick(Duration);
            Assert.That(tween.IsCompleted, Is.True);
            Assert.That(_value, Is.EqualTo(StartValue));
        }

        /// <summary>Restart jumps back to the start for every loop and ends on the target.</summary>
        [Test]
        public void RestartLoopsJumpBackAndEndOnTheTarget()
        {
            Tween<float> tween = BuildTween().SetLoops(3);
            tween.Start();

            tween.Tick(Duration + Duration * Half);
            Assert.That(_value, Is.EqualTo(EndValue * Half).Within(Tolerance));

            tween.Tick(Duration * 2f);
            Assert.That(tween.IsCompleted, Is.True);
            Assert.That(_value, Is.EqualTo(EndValue));
        }

        /// <summary>Continue starts every loop where the last one ended.</summary>
        [Test]
        public void ContinueLoopsKeepAdding()
        {
            Tween<float> tween = BuildTween().SetLoops(2, ELoopType.Continue);
            tween.Start();

            tween.Tick(Duration * 2f);

            Assert.That(_value, Is.EqualTo(EndValue * 2f).Within(Tolerance));
        }

        /// <summary>Endless loops never complete on their own.</summary>
        [Test]
        public void EndlessLoopsNeverComplete()
        {
            Tween<float> tween = BuildTween().SetLoops(-1, ELoopType.PingPong);
            tween.Start();

            for (int i = 0; i < 100; i++)
                tween.Tick(Duration);

            Assert.That(tween.IsCompleted, Is.False);
            Assert.That(float.IsPositiveInfinity(tween.Duration), Is.True);
            tween.Stop();
        }

        /// <summary>The length counts the delay once and every loop.</summary>
        [Test]
        public void DurationCountsTheDelayAndEveryLoop()
        {
            Tween<float> tween = BuildTween().SetDelay(Half).SetLoops(3);

            Assert.That(tween.Duration, Is.EqualTo(Half + Duration * 3f).Within(Tolerance));
        }

        /// <summary>A stronger overshoot goes further past the target, and still lands exactly on it.</summary>
        [Test]
        public void OvershootGoesPastTheTargetAndLandsOnIt()
        {
            Tween<float> soft = BuildTween().SetEase(EEasingType.EaseOutBack, 1f);
            soft.Start();
            soft.Tick(Duration * 0.7f);
            float softPeak = _value;
            soft.Stop();

            Tween<float> strong = BuildTween().SetEase(EEasingType.EaseOutBack, 3f);
            strong.Start();
            strong.Tick(Duration * 0.7f);

            Assert.That(_value, Is.GreaterThan(softPeak));
            Assert.That(_value, Is.GreaterThan(EndValue));

            strong.Tick(Duration);
            Assert.That(_value, Is.EqualTo(EndValue));
        }

        /// <summary>Every new easing starts at 0 and ends at 1.</summary>
        [Test]
        public void EveryEasingStartsAtZeroAndEndsAtOne(
            [Values(EEasingType.EaseInCubic, EEasingType.EaseOutCubic, EEasingType.EaseInSine,
                EEasingType.EaseOutSine, EEasingType.EaseInOutSine, EEasingType.EaseInQuart,
                EEasingType.EaseOutQuart, EEasingType.EaseInOutQuart, EEasingType.EaseInBack,
                EEasingType.EaseInOutBack)]
            EEasingType type)
        {
            Assert.That(Easings.Get(type)(0f), Is.EqualTo(0f).Within(Tolerance));
            Assert.That(Easings.Get(type)(1f), Is.EqualTo(1f).Within(Tolerance));
        }

        /// <summary>A tween of a plain value has no Unity object and keeps running.</summary>
        [Test]
        public void ATweenWithoutATargetKeepsRunning()
        {
            Tween<float> tween = Tweens.Value(StartValue, EndValue, Duration, Set);

            tween.Tick(Duration * Half);

            Assert.That(tween.IsRunning, Is.True);
            tween.Stop();
        }

        private void Set(float value) => _value = value;

        private Tween<float> BuildTween() => new Tween<float>(EndValue, Duration, Set,
                TweenLerpUtility.LerpFloatUnclamped, null, _target, from: StartValue)
            .SetEase(EEasingType.Linear);
    }
}
