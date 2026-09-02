using System.Collections.Generic;
using System.Text.RegularExpressions;
using Base.ServicesPackage;
using Base.TweeningPackage.Core;
using Base.TweeningPackage.Core.Data;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Base.TweeningPackage.Tests
{
    /// <summary>
    /// Covers a single tween driven by hand rather than by the runner. What matters is that it lands
    /// exactly on its target rather than near it, that finishing and cancelling are told apart, and
    /// that the events fire in the documented order.
    /// </summary>
    /// <remarks>
    /// A runner is registered for the fixture because starting a tween resolves one through the
    /// service locator. Nothing ticks it; every step here is driven by the test.
    /// </remarks>
    public sealed class TweenTests
    {
        private const float Duration = 1f;
        private const float EndValue = 10f;
        private const float StartValue = 0f;
        private const float Tolerance = 0.0001f;

        private GameObject _runnerObject;
        private GameObject _target;
        private float _value;
        private List<string> _events;

        /// <summary>Registers a runner once, so starting a tween resolves one.</summary>
        [OneTimeSetUp]
        public void BuildRunner()
        {
            _runnerObject = new GameObject(typeof(TweenRunner).Name);

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

            _runnerObject = null;
        }

        /// <summary>Every test starts from a fresh target and an empty event log.</summary>
        [SetUp]
        public void Build()
        {
            _target = new GameObject(nameof(TweenTests));
            _value = float.NaN;
            _events = new List<string>();
        }

        /// <summary>Takes the target back down between tests.</summary>
        [TearDown]
        public void Release()
        {
            if (_target != null)
                Object.DestroyImmediate(_target);

            _target = null;
        }

        /// <summary>Starting puts the value at the beginning right away.</summary>
        [Test]
        public void StartingAppliesTheStartValue()
        {
            Tween<float> tween = BuildTween();

            tween.Start();

            Assert.That(_value, Is.EqualTo(StartValue).Within(Tolerance));
            Assert.That(tween.IsRunning, Is.True);
            Assert.That(tween.IsCompleted, Is.False);
        }

        /// <summary>The start value can be read off the target rather than fixed in advance.</summary>
        [Test]
        public void TheStartValueCanBeReadFromTheTarget()
        {
            float current = 4f;
            Tween<float> tween = new(EndValue, Duration, Set, TweenLerpUtility.LerpFloatUnclamped, null, _target,
                fromGetter: () => current);

            tween.Start();

            Assert.That(_value, Is.EqualTo(4f).Within(Tolerance));
        }

        /// <summary>Ticking moves the value towards the target.</summary>
        [Test]
        public void TickingMovesTowardsTheTarget()
        {
            Tween<float> tween = BuildTween();

            tween.Start();
            tween.Tick(0.5f);

            Assert.That(_value, Is.EqualTo(5f).Within(Tolerance));
            Assert.That(tween.IsRunning, Is.True);
        }

        /// <summary>
        /// Reaching the end lands on the target exactly. A tween that stopped near its value would
        /// leave a transform slightly off where it was told to be.
        /// </summary>
        [Test]
        public void ReachingTheEndLandsExactlyOnTheTarget()
        {
            Tween<float> tween = BuildTween();

            tween.Start();
            tween.Tick(Duration);

            Assert.That(_value, Is.EqualTo(EndValue).Within(Tolerance));
            Assert.That(tween.IsCompleted, Is.True);
            Assert.That(tween.IsRunning, Is.False);
        }

        /// <summary>Overshooting the duration still lands on the target rather than past it.</summary>
        [Test]
        public void OvershootingTheDurationStillLandsOnTheTarget()
        {
            Tween<float> tween = BuildTween();

            tween.Start();
            tween.Tick(Duration * 5f);

            Assert.That(_value, Is.EqualTo(EndValue).Within(Tolerance));
        }

        /// <summary>Finishing announces completion first and the end of life after it.</summary>
        [Test]
        public void FinishingAnnouncesCompletionThenTheEnd()
        {
            Tween<float> tween = BuildTween();

            Listen(tween);
            tween.Start();
            tween.Tick(Duration);

            Assert.That(_events, Is.EqualTo(new[] { "Complete", "Kill" }));
        }

        /// <summary>Ticking a finished tween does nothing at all.</summary>
        [Test]
        public void TickingAFinishedTweenDoesNothing()
        {
            Tween<float> tween = BuildTween();

            Listen(tween);
            tween.Start();
            tween.Tick(Duration);
            tween.Tick(Duration);

            Assert.That(_events, Is.EqualTo(new[] { "Complete", "Kill" }));
        }

        /// <summary>A delay holds the value at the start until it has passed.</summary>
        [Test]
        public void ADelayHoldsTheValueAtTheStart()
        {
            Tween<float> tween = new(EndValue, Duration, Set, TweenLerpUtility.LerpFloatUnclamped, null, _target,
                delay: 1f);

            tween.Start();
            tween.Tick(0.5f);

            Assert.That(_value, Is.EqualTo(StartValue).Within(Tolerance));

            tween.Tick(0.5f);
            tween.Tick(0.5f);

            Assert.That(_value, Is.EqualTo(5f).Within(Tolerance), "the clock starts once the delay is over");
        }

        /// <summary>Cancelling leaves the value where it is and only announces the end of life.</summary>
        [Test]
        public void CancellingLeavesTheValueWhereItIs()
        {
            Tween<float> tween = BuildTween();

            Listen(tween);
            tween.Start();
            tween.Tick(0.5f);
            tween.Stop();

            Assert.That(_value, Is.EqualTo(5f).Within(Tolerance));
            Assert.That(tween.IsRunning, Is.False);
            Assert.That(_events, Is.EqualTo(new[] { "Kill" }), "a cancelled tween never completed");
        }

        /// <summary>Stopping with a snap lands on the target and counts as a completion.</summary>
        [Test]
        public void StoppingWithASnapCountsAsFinishing()
        {
            Tween<float> tween = BuildTween();

            Listen(tween);
            tween.Start();
            tween.Tick(0.5f);
            tween.Stop(complete: true);

            Assert.That(_value, Is.EqualTo(EndValue).Within(Tolerance));
            Assert.That(_events, Is.EqualTo(new[] { "Complete", "Kill" }));
        }

        /// <summary>Stopping a second time changes nothing.</summary>
        [Test]
        public void StoppingTwiceAnnouncesOnce()
        {
            Tween<float> tween = BuildTween();

            Listen(tween);
            tween.Start();
            tween.Stop();
            tween.Stop();

            Assert.That(_events, Is.EqualTo(new[] { "Kill" }));
        }

        /// <summary>Ticking a tween that was never started does nothing.</summary>
        [Test]
        public void TickingAnUnstartedTweenDoesNothing()
        {
            Tween<float> tween = BuildTween();

            tween.Tick(Duration);

            Assert.That(float.IsNaN(_value), Is.True, "nothing was ever written");
        }

        /// <summary>The easing decides where the value sits between the ends.</summary>
        [Test]
        public void TheEasingShapesTheMotion()
        {
            Tween<float> tween = new(EndValue, Duration, Set, TweenLerpUtility.LerpFloatUnclamped,
                Easings.Get(EEasingType.EaseInQuad), _target);

            tween.Start();
            tween.Tick(0.5f);

            Assert.That(_value, Is.EqualTo(2.5f).Within(Tolerance), "an ease in is only a quarter of the way at half");
        }

        /// <summary>
        /// A tween whose target was destroyed stops itself, so it cannot keep writing into a reference
        /// Unity already reports as gone.
        /// </summary>
        [Test]
        public void ATweenStopsWhenItsTargetIsDestroyed()
        {
            Tween<float> tween = BuildTween();

            tween.Start();
            Object.DestroyImmediate(_target);
            _target = null;

            LogAssert.Expect(LogType.Warning, new Regex("destroyed"));

            tween.Tick(0.5f);

            Assert.That(tween.IsRunning, Is.False);
            Assert.That(_value, Is.EqualTo(StartValue).Within(Tolerance), "the value was left alone");
        }

        /// <summary>A tween without the pieces it needs reports itself instead of starting.</summary>
        [Test]
        public void ATweenWithoutItsPiecesDoesNotStart()
        {
            Tween<float> tween = new(EndValue, Duration, null, TweenLerpUtility.LerpFloatUnclamped, null, _target);

            LogAssert.Expect(LogType.Warning, new Regex("Requires"));

            tween.Start();

            Assert.That(tween.IsRunning, Is.False);
        }

        private Tween<float> BuildTween() => new(EndValue, Duration, Set, TweenLerpUtility.LerpFloatUnclamped, null,
            _target, from: StartValue);

        private void Listen(TweenBase tween)
        {
            tween.OnComplete += _ => _events.Add("Complete");
            tween.OnKill += _ => _events.Add("Kill");
        }

        private void Set(float value) => _value = value;
    }
}