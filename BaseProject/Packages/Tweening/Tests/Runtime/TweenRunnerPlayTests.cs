using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Base.TweeningPackage.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Base.TweeningPackage.PlayTests
{
    /// <summary>
    /// Covers the part of tweening the edit mode suite cannot reach: the runner is a component whose
    /// <c>Update</c> drives every registered tween, so completion, lifecycle events and the reaction
    /// to a destroyed target only happen once real frames pass.
    /// </summary>
    public sealed class TweenRunnerPlayTests
    {
        private const string DestroyedTargetMessage = "was destroyed";
        private const float Duration = 0.1f;
        private const float LiveDuration = 2f;
        private const float LongDelay = 10f;
        private const float MovedStartValue = 0.5f;
        private const float MovedTolerance = 0.05f;
        private const float ShortDelay = 0.1f;
        private const float StartValue = 0f;
        private const float TargetValue = 1f;
        private const float Timeout = 5f;
        private const float Tolerance = 0.0001f;

        private readonly List<GameObject> _hosts = new();

        private int _deregisteredCount;
        private int _registeredCount;
        private float _firstObserved;
        private float _liveStart;
        private float _observed;

        /// <summary>Starts every test from a known state, since the fixture instance is reused.</summary>
        [SetUp]
        public void Prepare()
        {
            _firstObserved = float.NaN;
            _liveStart = StartValue;
            _observed = StartValue;
        }

        /// <summary>
        /// The runner's events are static, so a handler left behind would be invoked by the next test.
        /// Unsubscribing a handler that never subscribed is a no-op, so this can run unconditionally.
        /// </summary>
        [TearDown]
        public void Cleanup()
        {
            TweenRunner.OnTweenRegistered -= CountRegistered;
            TweenRunner.OnTweenDeregistered -= CountDeregistered;

            foreach (GameObject host in _hosts)
            {
                if (host != null)
                    Object.DestroyImmediate(host);
            }

            _hosts.Clear();
            _registeredCount = 0;
            _deregisteredCount = 0;
            _firstObserved = float.NaN;
            _liveStart = StartValue;
            _observed = StartValue;
        }

        /// <summary>A tween left alone until its duration is up lands exactly on its target value.</summary>
        [UnityTest]
        public IEnumerator ATweenReachesItsTargetValueOnceItsDurationHasPassed()
        {
            CreateRunner();
            GameObject target = CreateHost(nameof(ATweenReachesItsTargetValueOnceItsDurationHasPassed));

            Tween<float> tween = CreateTween(target, 0f);
            tween.Start();

            yield return WaitUntilFinished(tween);

            Assert.That(tween.IsCompleted, Is.True);
            Assert.That(_observed, Is.EqualTo(TargetValue).Within(Tolerance));
        }

        /// <summary>
        /// A tween announces itself once on the way in and once on the way out, which is what any
        /// listening tool counts on to know how many tweens are live.
        /// </summary>
        [UnityTest]
        public IEnumerator TheRunnerAnnouncesATweenOnceOnEachSideOfItsLife()
        {
            CreateRunner();
            GameObject target = CreateHost(nameof(TheRunnerAnnouncesATweenOnceOnEachSideOfItsLife));

            TweenRunner.OnTweenRegistered += CountRegistered;
            TweenRunner.OnTweenDeregistered += CountDeregistered;

            Tween<float> tween = CreateTween(target, 0f);
            tween.Start();

            yield return WaitUntilFinished(tween);
            yield return null;

            Assert.That(_registeredCount, Is.EqualTo(1));
            Assert.That(_deregisteredCount, Is.EqualTo(1));
        }

        /// <summary>
        /// A tween whose target was destroyed has nothing left to write to, so it stops itself rather
        /// than throwing on the next frame.
        /// </summary>
        [UnityTest]
        public IEnumerator ATweenStopsWhenItsTargetIsDestroyed()
        {
            CreateRunner();
            GameObject target = CreateHost(nameof(ATweenStopsWhenItsTargetIsDestroyed));

            Tween<float> tween = CreateTween(target, LongDelay);
            tween.Start();

            LogAssert.Expect(LogType.Warning, new Regex(DestroyedTargetMessage));
            Object.Destroy(target);

            yield return null;
            yield return null;

            Assert.That(tween.IsRunning, Is.False);
            Assert.That(_observed, Is.EqualTo(StartValue).Within(Tolerance));
        }

        /// <summary>
        /// A literal start value has nothing to go stale, so it is applied at once and held for the
        /// length of the delay. This is what a fade in wants: invisible immediately, then a wait.
        /// </summary>
        [UnityTest]
        public IEnumerator ALiteralStartValueIsAppliedBeforeTheDelayElapses()
        {
            CreateRunner();
            GameObject target = CreateHost(nameof(ALiteralStartValueIsAppliedBeforeTheDelayElapses));

            _observed = float.NaN;

            Tween<float> tween = CreateTween(target, LongDelay);
            tween.Start();

            Assert.That(_observed, Is.EqualTo(StartValue).Within(Tolerance));

            yield return null;

            Assert.That(tween.IsCompleted, Is.False);
        }

        /// <summary>
        /// A getter is a promise to read the value at the moment movement begins, so a delay in front
        /// of it must not hand back what the value was when the tween was started. Anything that moved
        /// the target during the delay would otherwise be undone by a jump back.
        /// </summary>
        [UnityTest]
        public IEnumerator AGetterIsReadAfterTheDelayAndNotBefore()
        {
            CreateRunner();
            GameObject target = CreateHost(nameof(AGetterIsReadAfterTheDelayAndNotBefore));

            Tween<float> tween = CreateLiveTween(target);
            tween.Start();

            // Stands in for anything else moving the target while the tween is still waiting.
            _liveStart = MovedStartValue;

            yield return WaitUntilFirstWrite();

            tween.Stop();

            Assert.That(_firstObserved, Is.EqualTo(MovedStartValue).Within(MovedTolerance));
        }

        /// <summary>Spins until the tween finishes, so a stall fails on time instead of hanging.</summary>
        private static IEnumerator WaitUntilFinished(ITween tween)
        {
            float deadline = Time.unscaledTime + Timeout;

            while (!tween.IsCompleted && Time.unscaledTime < deadline)
                yield return null;
        }

        /// <summary>Puts a runner in the scene so tweens have something to register with.</summary>
        private void CreateRunner() => CreateHost(nameof(TweenRunner)).AddComponent<TweenRunner>();

        /// <summary>Creates an object and remembers it so the teardown can clean it up.</summary>
        private GameObject CreateHost(string name)
        {
            GameObject host = new(name);
            _hosts.Add(host);

            return host;
        }

        /// <summary>Builds a float tween starting from a literal value.</summary>
        private Tween<float> CreateTween(Object target, float delay) => new(TargetValue,
            Duration,
            Record,
            Mathf.LerpUnclamped,
            null,
            target,
            delay,
            from: StartValue);

        /// <summary>Builds a delayed float tween that reads its start value when it begins moving.</summary>
        private Tween<float> CreateLiveTween(Object target) => new(TargetValue,
            LiveDuration,
            Record,
            Mathf.LerpUnclamped,
            null,
            target,
            ShortDelay,
            fromGetter: () => _liveStart);

        /// <summary>Spins until the tween writes anything at all, so a stall fails instead of hanging.</summary>
        private IEnumerator WaitUntilFirstWrite()
        {
            float deadline = Time.unscaledTime + Timeout;

            while (float.IsNaN(_firstObserved) && Time.unscaledTime < deadline)
                yield return null;
        }

        /// <summary>Records every value the tween writes, and separately the very first of them.</summary>
        private void Record(float value)
        {
            if (float.IsNaN(_firstObserved))
                _firstObserved = value;

            _observed = value;
        }

        /// <summary>Counts how often the runner reported a tween as registered.</summary>
        private void CountRegistered(ITween tween) => _registeredCount++;

        /// <summary>Counts how often the runner reported a tween as deregistered.</summary>
        private void CountDeregistered(ITween tween) => _deregisteredCount++;
    }
}