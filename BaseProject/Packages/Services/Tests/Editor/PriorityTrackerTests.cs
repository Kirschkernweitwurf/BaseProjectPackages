using System.Text.RegularExpressions;
using Base.ServicesPackage.Tracking;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Base.ServicesPackage.Tests
{
    /// <summary>
    /// Covers the rule that decides which request wins when several are active at once: the highest
    /// priority takes the top spot, the most recent of equal priorities takes it, and giving a request
    /// back restores whatever was underneath it. The change event has to fire exactly when the top
    /// spot actually changes hands, since listeners apply it to cursor state or time scale.
    /// </summary>
    public sealed class PriorityTrackerTests
    {
        private const string FirstItem = "First";
        private const string SecondItem = "Second";


        private PriorityTracker<string> _tracker;
        private object _firstCaller;
        private object _secondCaller;
        private int _changeCount;
        private TrackedItem<string> _lastChange;

        /// <summary>Every test starts from an empty tracker with the change event counted.</summary>
        [SetUp]
        public void Build()
        {
            _tracker = new PriorityTracker<string>();
            _firstCaller = new object();
            _secondCaller = new object();
            _changeCount = 0;
            _lastChange = null;

            _tracker.OnCurrentActiveItemChanged += OnChanged;
        }

        /// <summary>An empty tracker has no top item to hand out.</summary>
        [Test]
        public void AnEmptyTrackerHasNothingActive()
        {
            Assert.That(_tracker.CurrentTrackedItem, Is.Null);
            Assert.That(_tracker.TrackedItems, Is.Empty);
        }

        /// <summary>The first request takes the top spot and announces itself.</summary>
        [Test]
        public void TheFirstRequestBecomesActive()
        {
            _tracker.Add(FirstItem, (uint)EPriority.Low, _firstCaller);

            Assert.That(_tracker.CurrentTrackedItem, Is.Not.Null);
            Assert.That(_tracker.CurrentTrackedItem.Item, Is.EqualTo(FirstItem));
            Assert.That(_changeCount, Is.EqualTo(1));
            Assert.That(_lastChange, Is.SameAs(_tracker.CurrentTrackedItem));
        }

        /// <summary>A stronger request takes the top spot from a weaker one.</summary>
        [Test]
        public void AHigherPriorityTakesOver()
        {
            _tracker.Add(FirstItem, (uint)EPriority.Low, _firstCaller);
            _tracker.Add(SecondItem, (uint)EPriority.High, _secondCaller);

            Assert.That(_tracker.CurrentTrackedItem.Item, Is.EqualTo(SecondItem));
            Assert.That(_changeCount, Is.EqualTo(2));
        }

        /// <summary>A weaker request waits its turn and does not disturb the listeners.</summary>
        [Test]
        public void ALowerPriorityWaits()
        {
            _tracker.Add(FirstItem, (uint)EPriority.High, _firstCaller);
            _tracker.Add(SecondItem, (uint)EPriority.Low, _secondCaller);

            Assert.That(_tracker.CurrentTrackedItem.Item, Is.EqualTo(FirstItem));
            Assert.That(_changeCount, Is.EqualTo(1), "the top spot never changed hands");
        }

        /// <summary>Between equals the most recent request wins.</summary>
        [Test]
        public void TheMostRecentOfEqualPrioritiesWins()
        {
            _tracker.Add(FirstItem, (uint)EPriority.Medium, _firstCaller);
            _tracker.Add(SecondItem, (uint)EPriority.Medium, _secondCaller);

            Assert.That(_tracker.CurrentTrackedItem.Item, Is.EqualTo(SecondItem));
        }

        /// <summary>Giving a request back uncovers the one underneath it.</summary>
        [Test]
        public void RemovingTheTopUncoversTheNextOne()
        {
            _tracker.Add(FirstItem, (uint)EPriority.Low, _firstCaller);
            _tracker.Add(SecondItem, (uint)EPriority.High, _secondCaller);
            _tracker.Remove(_secondCaller);

            Assert.That(_tracker.CurrentTrackedItem.Item, Is.EqualTo(FirstItem));
            Assert.That(_changeCount, Is.EqualTo(3));
        }

        /// <summary>Removing something that is not on top leaves the top spot alone.</summary>
        [Test]
        public void RemovingUnderneathLeavesTheTopAlone()
        {
            _tracker.Add(FirstItem, (uint)EPriority.High, _firstCaller);
            _tracker.Add(SecondItem, (uint)EPriority.Low, _secondCaller);
            _tracker.Remove(_secondCaller);

            Assert.That(_tracker.CurrentTrackedItem.Item, Is.EqualTo(FirstItem));
            Assert.That(_changeCount, Is.EqualTo(1));
        }

        /// <summary>The last request going back leaves nothing active, and says so.</summary>
        [Test]
        public void RemovingTheLastRequestClearsTheTopSpot()
        {
            _tracker.Add(FirstItem, (uint)EPriority.Low, _firstCaller);
            _tracker.Remove(_firstCaller);

            Assert.That(_tracker.CurrentTrackedItem, Is.Null);
            Assert.That(_changeCount, Is.EqualTo(2));
            Assert.That(_lastChange, Is.Null, "listeners have to be told that nothing is active");
        }

        /// <summary>Clearing drops every request and resets the tiebreaker.</summary>
        [Test]
        public void ClearingDropsEverythingAndResetsTheOrder()
        {
            _tracker.Add(FirstItem, (uint)EPriority.Low, _firstCaller);
            _tracker.Add(SecondItem, (uint)EPriority.Low, _secondCaller);
            _tracker.Clear();

            Assert.That(_tracker.CurrentTrackedItem, Is.Null);
            Assert.That(_tracker.TrackedItems, Is.Empty);

            _tracker.Add(FirstItem, (uint)EPriority.Low, _firstCaller);

            Assert.That(_tracker.CurrentTrackedItem.Order, Is.EqualTo(0));
        }

        /// <summary>A tracked item carries the priority and order it was filed with.</summary>
        [Test]
        public void ATrackedItemCarriesItsPriorityAndOrder()
        {
            _tracker.Add(FirstItem, (uint)EPriority.Critical, _firstCaller);

            TrackedItem<string> tracked = _tracker.CurrentTrackedItem;

            Assert.That(tracked.Item, Is.EqualTo(FirstItem));
            Assert.That(tracked.Priority, Is.EqualTo((uint)EPriority.Critical));
            Assert.That(tracked.Order, Is.EqualTo(0));
        }

        /// <summary>The tracker answers what is held and who is holding it.</summary>
        [Test]
        public void TheTrackerReportsWhatItHolds()
        {
            _tracker.Add(FirstItem, (uint)EPriority.Low, _firstCaller);

            Assert.That(_tracker.IsTracked(FirstItem), Is.True);
            Assert.That(_tracker.IsTracked(SecondItem), Is.False);
            Assert.That(_tracker.IsTracked(null), Is.False);
            Assert.That(_tracker.HasCaller(_firstCaller), Is.True);
            Assert.That(_tracker.HasCaller(_secondCaller), Is.False);
            Assert.That(_tracker.HasCaller(null), Is.False);
        }

        /// <summary>A listener that arrives late can ask to be brought up to date.</summary>
        [Test]
        public void InitializeAnnouncesTheCurrentState()
        {
            _tracker.Add(FirstItem, (uint)EPriority.Low, _firstCaller);
            _tracker.Initialize();

            Assert.That(_changeCount, Is.EqualTo(2));
            Assert.That(_lastChange.Item, Is.EqualTo(FirstItem));
        }

        /// <summary>A request without an item would hand out nothing when it wins.</summary>
        [Test]
        public void ARequestWithoutAnItemIsReported()
        {
            LogAssert.Expect(LogType.Warning, new Regex("null item"));

            _tracker.Add(null, (uint)EPriority.Low, _firstCaller);

            Assert.That(_tracker.CurrentTrackedItem, Is.Null);
        }

        /// <summary>A request without a caller could never be given back.</summary>
        [Test]
        public void ARequestWithoutACallerIsReported()
        {
            LogAssert.Expect(LogType.Warning, new Regex("add with a null caller"));

            _tracker.Add(FirstItem, (uint)EPriority.Low, null);

            Assert.That(_tracker.CurrentTrackedItem, Is.Null);
        }

        /// <summary>One caller holds one request, since the caller is the key to give it back.</summary>
        [Test]
        public void ASecondRequestFromTheSameCallerIsReported()
        {
            _tracker.Add(FirstItem, (uint)EPriority.Low, _firstCaller);

            LogAssert.Expect(LogType.Warning, new Regex("same caller twice"));

            _tracker.Add(SecondItem, (uint)EPriority.High, _firstCaller);

            Assert.That(_tracker.CurrentTrackedItem.Item, Is.EqualTo(FirstItem));
            Assert.That(_tracker.TrackedItems.Count, Is.EqualTo(1));
        }

        /// <summary>Giving back something that was never taken is reported.</summary>
        [Test]
        public void RemovingFromAnUnknownCallerIsReported()
        {
            LogAssert.Expect(LogType.Warning, new Regex("unknown caller"));

            _tracker.Remove(_firstCaller);
        }

        /// <summary>Giving back without a caller has nothing to look up.</summary>
        [Test]
        public void RemovingWithoutACallerIsReported()
        {
            LogAssert.Expect(LogType.Warning, new Regex("remove with a null caller"));

            _tracker.Remove(null);
        }

        private void OnChanged(TrackedItem<string> tracked)
        {
            _changeCount++;
            _lastChange = tracked;
        }
    }
}