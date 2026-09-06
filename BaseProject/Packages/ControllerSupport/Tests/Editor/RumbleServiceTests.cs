using System.Text.RegularExpressions;
using Base.ControllerSupportPackage.Haptics;
using Base.ServicesPackage.Tracking;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Base.ControllerSupportPackage.Tests
{
    /// <summary>
    /// Covers the bookkeeping the service does around its live requests. One caller holds one request,
    /// so retriggering restarts a rumble instead of stacking copies of it, and the strongest request
    /// is the one that reaches the motors.
    /// </summary>
    /// <remarks>
    /// The motors themselves are only touched from Update, which does not run in an editor test, so
    /// what is covered here is which request the service considers current.
    /// </remarks>
    public sealed class RumbleServiceTests
    {
        private const float CurveEnd = 1f;
        private const float CurveStart = 0f;
        private const float Tolerance = 0.0001f;

        private GameObject _serviceObject;
        private RumbleService _service;
        private object _firstCaller;
        private object _secondCaller;

        /// <summary>Every test starts from a fresh service with no live requests.</summary>
        [SetUp]
        public void Build()
        {
            _serviceObject = new GameObject(typeof(RumbleService).Name);
            _service = _serviceObject.AddComponent<RumbleService>();
            _firstCaller = new object();
            _secondCaller = new object();
        }

        /// <summary>Takes the service back down between tests.</summary>
        [TearDown]
        public void Release()
        {
            if (_serviceObject != null)
                Object.DestroyImmediate(_serviceObject);

            _serviceObject = null;
            _service = null;
        }

        /// <summary>A played pattern becomes the live request of its caller.</summary>
        [Test]
        public void APlayedPatternBecomesALiveRequest()
        {
            RumblePatternData pattern = Burst();

            _service.Play(pattern, _firstCaller);

            Assert.That(_service.RumbleTracker.HasCaller(_firstCaller), Is.True);
            Assert.That(_service.RumbleTracker.CurrentTrackedItem.Item.Pattern, Is.SameAs(pattern));
        }

        /// <summary>
        /// A second play from the same caller replaces the first, so retriggering a rumble restarts it
        /// rather than leaving two copies fighting over the motors.
        /// </summary>
        [Test]
        public void PlayingTwiceFromOneCallerReplacesRatherThanStacks()
        {
            RumblePatternData replacement = Burst();

            _service.Play(Burst(), _firstCaller);
            _service.Play(replacement, _firstCaller);

            Assert.That(_service.RumbleTracker.TrackedItems.Count, Is.EqualTo(1));
            Assert.That(_service.RumbleTracker.CurrentTrackedItem.Item.Pattern, Is.SameAs(replacement));
        }

        /// <summary>The strongest request is the one that reaches the motors.</summary>
        [Test]
        public void TheStrongestRequestIsTheCurrentOne()
        {
            RumblePatternData important = Burst();

            _service.Play(Burst(), _firstCaller, EPriority.Low);
            _service.Play(important, _secondCaller, EPriority.Critical);

            Assert.That(_service.RumbleTracker.CurrentTrackedItem.Item.Pattern, Is.SameAs(important));
            Assert.That(_service.RumbleTracker.TrackedItems.Count, Is.EqualTo(2), "the weaker one keeps its clock");
        }

        /// <summary>A burst can be fired without authoring a pattern for it.</summary>
        [Test]
        public void ABurstNeedsNoAuthoredPattern()
        {
            _service.PlayBurst(0.4f, 0.6f, 0.5f, _firstCaller);

            Assert.That(_service.RumbleTracker.HasCaller(_firstCaller), Is.True);

            _service.RumbleTracker.CurrentTrackedItem.Item.Sample(out float low, out float high);

            Assert.That(low, Is.EqualTo(0.4f).Within(Tolerance));
            Assert.That(high, Is.EqualTo(0.6f).Within(Tolerance));
        }

        /// <summary>Stopping drops the request belonging to that caller and nobody else's.</summary>
        [Test]
        public void StoppingDropsOnlyThatCallersRequest()
        {
            _service.Play(Burst(), _firstCaller);
            _service.Play(Burst(), _secondCaller);

            _service.Stop(_firstCaller);

            Assert.That(_service.RumbleTracker.HasCaller(_firstCaller), Is.False);
            Assert.That(_service.RumbleTracker.HasCaller(_secondCaller), Is.True);
        }

        /// <summary>
        /// Stopping something that already ran out is a normal state rather than a mistake, so it stays
        /// silent instead of reporting an unknown caller.
        /// </summary>
        [Test]
        public void StoppingSomethingThatAlreadyEndedIsSilent()
        {
            Assert.DoesNotThrow(() => _service.Stop(_firstCaller));
            Assert.DoesNotThrow(() => _service.Stop(null));
        }

        /// <summary>Stopping everything leaves no live request behind.</summary>
        [Test]
        public void StoppingEverythingClearsEveryRequest()
        {
            _service.Play(Burst(), _firstCaller);
            _service.Play(Burst(), _secondCaller);

            _service.StopAll();

            Assert.That(_service.RumbleTracker.TrackedItems, Is.Empty);
            Assert.That(_service.RumbleTracker.CurrentTrackedItem, Is.Null);
        }

        /// <summary>Nothing to play is reported rather than filed as an empty request.</summary>
        [Test]
        public void NothingToPlayIsReported()
        {
            LogAssert.Expect(LogType.Warning, new Regex(nameof(RumblePatternData)));

            _service.Play((RumblePatternData)null, _firstCaller);

            Assert.That(_service.RumbleTracker.TrackedItems, Is.Empty);
        }

        /// <summary>A missing pattern asset is reported rather than walked into.</summary>
        [Test]
        public void AMissingPatternAssetIsReported()
        {
            LogAssert.Expect(LogType.Warning, new Regex($"{nameof(RumblePattern)}\\b"));

            _service.Play((RumblePattern)null, _firstCaller);

            Assert.That(_service.RumbleTracker.TrackedItems, Is.Empty);
        }

        /// <summary>A request without a caller could never be stopped again, so it is refused.</summary>
        [Test]
        public void ARequestWithoutACallerIsReported()
        {
            LogAssert.Expect(LogType.Warning, new Regex("without a caller"));

            _service.Play(Burst(), null);

            Assert.That(_service.RumbleTracker.TrackedItems, Is.Empty);
        }

        /// <summary>The main strength is brought into the range a motor accepts.</summary>
        [Test]
        public void TheMainStrengthIsClamped()
        {
            _service.SetMainIntensity(5f);

            Assert.That(_service.MainIntensity, Is.EqualTo(1f).Within(Tolerance));

            _service.SetMainIntensity(-5f);

            Assert.That(_service.MainIntensity, Is.EqualTo(0f).Within(Tolerance));
        }

        /// <summary>Switching rumble on or off is announced once.</summary>
        [Test]
        public void SwitchingRumbleIsAnnouncedOnce()
        {
            int changes = 0;

            _service.OnRumbleEnabledChanged += _ => changes++;

            _service.SetRumbleEnabled(true);
            _service.SetRumbleEnabled(true);

            Assert.That(_service.IsRumbleEnabled, Is.True);
            Assert.That(changes, Is.EqualTo(1));
        }

        /// <summary>
        /// Switching rumble off leaves the live requests running their clocks, so turning it back on
        /// does not replay a burst that has long since expired.
        /// </summary>
        [Test]
        public void SwitchingRumbleOffKeepsTheLiveRequests()
        {
            _service.Play(Burst(), _firstCaller);
            _service.SetRumbleEnabled(true);
            _service.SetRumbleEnabled(false);

            Assert.That(_service.RumbleTracker.HasCaller(_firstCaller), Is.True);
        }

        private static RumblePatternData Burst() => new(1f,
            AnimationCurve.Constant(CurveStart, CurveEnd, 1f),
            AnimationCurve.Constant(CurveStart, CurveEnd, 1f));
    }
}