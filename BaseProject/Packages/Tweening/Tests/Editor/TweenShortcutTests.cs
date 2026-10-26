using Base.ServicesPackage;
using Base.TweeningPackage.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.TweeningPackage.Tests
{
    /// <summary>
    /// Covers the shortcuts whose value is a shape rather than a straight line: a punch must come back exactly where it
    /// started, a jump and a path must land on their end. Also covers stopping tweens by their component.
    /// </summary>
    public sealed class TweenShortcutTests
    {
        private const float Duration = 1f;
        private const float Tolerance = 0.0001f;

        private GameObject _runnerObject;
        private Transform _transform;

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

        /// <summary>Every test starts from a fresh transform.</summary>
        [SetUp]
        public void Build() => _transform = EditorUtility
            .CreateGameObjectWithHideFlags(nameof(TweenShortcutTests), HideFlags.HideAndDontSave).transform;

        /// <summary>Takes the transform back down.</summary>
        [TearDown]
        public void Release() => Object.DestroyImmediate(_transform.gameObject);

        /// <summary>A punch swells past the start and comes back exactly to it.</summary>
        [Test]
        public void APunchComesBackToWhereItStarted([Values(1, 10)] int vibrato)
        {
            Tween<float> tween = _transform.TweenPunchScale(Vector3.one, Duration, vibrato);

            tween.Tick(Duration * 0.1f);
            Assert.That(_transform.localScale.x, Is.GreaterThan(1f));

            tween.Tick(Duration);
            Assert.That(_transform.localScale, Is.EqualTo(Vector3.one));
        }

        /// <summary>A jump rises above the straight line and lands on its end.</summary>
        [Test]
        public void AJumpLandsOnItsEnd()
        {
            Vector3 end = Vector3.right * 4f;
            Tween<float> tween = _transform.TweenJump(end, 2f, 1, Duration);

            tween.Tick(Duration * 0.5f);
            Assert.That(_transform.position.y, Is.GreaterThan(0f));

            tween.Tick(Duration);
            Assert.That(Vector3.Distance(_transform.position, end), Is.LessThan(Tolerance));
        }

        /// <summary>A path, straight or curved, ends on its last point.</summary>
        [Test]
        public void APathEndsOnItsLastPoint([Values(false, true)] bool isCurved)
        {
            Vector3[] path = { Vector3.right, Vector3.right + Vector3.up, Vector3.up * 2f };
            Tween<float> tween = _transform.TweenLocalPath(path, Duration, isCurved);

            tween.Tick(Duration);

            Assert.That(Vector3.Distance(_transform.localPosition, path[^1]), Is.LessThan(Tolerance));
        }

        /// <summary>Killing by the component stops its tweens and leaves others running.</summary>
        [Test]
        public void KillingByTheComponentStopsOnlyItsTweens()
        {
            Transform other = EditorUtility.CreateGameObjectWithHideFlags("Other", HideFlags.HideAndDontSave)
                .transform;
            Tween<Vector3> mine = _transform.TweenScale(2f, Duration);
            Tween<Vector3> theirs = other.TweenScale(2f, Duration);

            int stopped = _transform.KillTweens();

            Assert.That(stopped, Is.EqualTo(1));
            Assert.That(mine.IsRunning, Is.False);
            Assert.That(theirs.IsRunning, Is.True);

            theirs.Stop();
            Object.DestroyImmediate(other.gameObject);
        }
    }
}
