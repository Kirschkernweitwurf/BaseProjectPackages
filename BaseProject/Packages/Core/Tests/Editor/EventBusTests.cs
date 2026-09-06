using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using EventBusService = Base.CorePackage.EventBus.EventBus;
using Object = UnityEngine.Object;

namespace Base.CorePackage.Tests
{
    /// <summary>
    /// Covers what the bus promises a caller: an event reaches every handler of its type and nobody
    /// else, in the order they subscribed, and a dispatch that is already running survives handlers
    /// coming and going and throwing while it runs.
    /// </summary>
    public sealed class EventBusTests
    {
        private const int FirstValue = 1;
        private const string HandlerFailure = "Handler failed on purpose.";

        private GameObject _busObject;
        private EventBusService _bus;
        private List<string> _calls;

        /// <summary>The bus lives on a game object, so it is built once for the whole fixture.</summary>
        [OneTimeSetUp]
        public void BuildBus()
        {
            _busObject = EditorUtility.CreateGameObjectWithHideFlags(typeof(EventBusService).Name,
                HideFlags.HideAndDontSave);
            _bus = _busObject.AddComponent<EventBusService>();
        }

        /// <summary>Takes the game object back down once every test in the fixture has run.</summary>
        [OneTimeTearDown]
        public void ReleaseBus()
        {
            if (_busObject != null)
                Object.DestroyImmediate(_busObject);

            _busObject = null;
            _bus = null;
        }

        /// <summary>Every test starts on a bus with no handlers on it.</summary>
        [SetUp]
        public void Build()
        {
            _bus.Clear();
            _calls = new List<string>();
        }

        /// <summary>A published event reaches the handler that asked for it.</summary>
        [Test]
        public void APublishedEventReachesItsHandler()
        {
            int received = 0;

            _bus.Subscribe<ProbeEvent>(published => received = published.Value);
            _bus.Publish(new ProbeEvent(FirstValue));

            Assert.That(received, Is.EqualTo(FirstValue));
        }

        /// <summary>Handlers are called in the order they subscribed.</summary>
        [Test]
        public void HandlersAreCalledInSubscriptionOrder()
        {
            _bus.Subscribe<ProbeEvent>(_ => _calls.Add("First"));
            _bus.Subscribe<ProbeEvent>(_ => _calls.Add("Second"));
            _bus.Subscribe<ProbeEvent>(_ => _calls.Add("Third"));

            _bus.Publish(new ProbeEvent(FirstValue));

            Assert.That(_calls, Is.EqualTo(new[] { "First", "Second", "Third" }));
        }

        /// <summary>A handler only hears the event type it subscribed to.</summary>
        [Test]
        public void AHandlerOnlyHearsItsOwnEventType()
        {
            _bus.Subscribe<ProbeEvent>(_ => _calls.Add("Probe"));

            _bus.Publish(new OtherProbeEvent());

            Assert.That(_calls, Is.Empty);
        }

        /// <summary>Publishing to nobody is a normal state, not a failure.</summary>
        [Test]
        public void PublishingWithoutHandlersDoesNothing()
            => Assert.DoesNotThrow(() => _bus.Publish(new ProbeEvent(FirstValue)));

        /// <summary>An unsubscribed handler stops hearing events.</summary>
        [Test]
        public void AnUnsubscribedHandlerStopsHearing()
        {
            Action<ProbeEvent> handler = _ => _calls.Add("Handler");

            _bus.Subscribe(handler);
            _bus.Unsubscribe(handler);
            _bus.Publish(new ProbeEvent(FirstValue));

            Assert.That(_calls, Is.Empty);
        }

        /// <summary>Unsubscribing one handler leaves the others in place.</summary>
        [Test]
        public void UnsubscribingOneLeavesTheOthers()
        {
            Action<ProbeEvent> first = _ => _calls.Add("First");

            _bus.Subscribe(first);
            _bus.Subscribe<ProbeEvent>(_ => _calls.Add("Second"));
            _bus.Unsubscribe(first);
            _bus.Publish(new ProbeEvent(FirstValue));

            Assert.That(_calls, Is.EqualTo(new[] { "Second" }));
        }

        /// <summary>Unsubscribing something that never subscribed changes nothing.</summary>
        [Test]
        public void UnsubscribingAnUnknownHandlerDoesNothing()
        {
            _bus.Subscribe<ProbeEvent>(_ => _calls.Add("Handler"));
            _bus.Unsubscribe<ProbeEvent>(_ => _calls.Add("Stranger"));
            _bus.Publish(new ProbeEvent(FirstValue));

            Assert.That(_calls, Is.EqualTo(new[] { "Handler" }));
        }

        /// <summary>Disposing the token is the same as unsubscribing by hand.</summary>
        [Test]
        public void DisposingTheTokenUnsubscribes()
        {
            IDisposable token = _bus.Subscribe<ProbeEvent>(_ => _calls.Add("Handler"));

            token.Dispose();
            _bus.Publish(new ProbeEvent(FirstValue));

            Assert.That(_calls, Is.Empty);
        }

        /// <summary>Disposing twice is harmless, so callers do not have to track it.</summary>
        [Test]
        public void DisposingTheTokenTwiceIsHarmless()
        {
            IDisposable token = _bus.Subscribe<ProbeEvent>(_ => _calls.Add("Handler"));

            token.Dispose();

            Assert.DoesNotThrow(() => token.Dispose());
        }

        /// <summary>One faulty handler must not swallow the events of the others.</summary>
        [Test]
        public void AThrowingHandlerDoesNotStopTheRest()
        {
            _bus.Subscribe<ProbeEvent>(_ => _calls.Add("Before"));
            _bus.Subscribe<ProbeEvent>(_ => throw new InvalidOperationException(HandlerFailure));
            _bus.Subscribe<ProbeEvent>(_ => _calls.Add("After"));

            LogAssert.Expect(LogType.Error, new Regex(HandlerFailure));

            _bus.Publish(new ProbeEvent(FirstValue));

            Assert.That(_calls, Is.EqualTo(new[] { "Before", "After" }));
        }

        /// <summary>A handler that subscribes during a dispatch waits for the next one.</summary>
        [Test]
        public void AHandlerSubscribedDuringDispatchWaitsForTheNextEvent()
        {
            _bus.Subscribe<ProbeEvent>(_ =>
            {
                _calls.Add("Outer");
                _bus.Subscribe<ProbeEvent>(late => _calls.Add("Inner"));
            });

            _bus.Publish(new ProbeEvent(FirstValue));

            Assert.That(_calls, Is.EqualTo(new[] { "Outer" }), "the new handler must not join a running dispatch");
        }

        /// <summary>A handler removed during a dispatch still receives the event in flight.</summary>
        [Test]
        public void AHandlerRemovedDuringDispatchStillReceivesTheCurrentEvent()
        {
            Action<ProbeEvent> second = _ => _calls.Add("Second");

            _bus.Subscribe<ProbeEvent>(_ =>
            {
                _calls.Add("First");
                _bus.Unsubscribe(second);
            });

            _bus.Subscribe(second);
            _bus.Publish(new ProbeEvent(FirstValue));

            Assert.That(_calls, Is.EqualTo(new[] { "First", "Second" }));

            _calls.Clear();
            _bus.Publish(new ProbeEvent(FirstValue));

            Assert.That(_calls, Is.EqualTo(new[] { "First" }), "the removal takes effect from the next event on");
        }

        /// <summary>Clearing drops every handler of every type.</summary>
        [Test]
        public void ClearingDropsEveryHandler()
        {
            _bus.Subscribe<ProbeEvent>(_ => _calls.Add("Probe"));
            _bus.Subscribe<OtherProbeEvent>(_ => _calls.Add("Other"));

            _bus.Clear();
            _bus.Publish(new ProbeEvent(FirstValue));
            _bus.Publish(new OtherProbeEvent());

            Assert.That(_calls, Is.Empty);
        }

        /// <summary>A subscription without a handler is a bug and is reported as one.</summary>
        [Test]
        public void SubscribingWithoutAHandlerIsReported()
        {
            LogAssert.Expect(LogType.Error, new Regex(nameof(ProbeEvent)));

            IDisposable token = _bus.Subscribe<ProbeEvent>(null);

            Assert.That(token, Is.Not.Null, "an empty token still has to come back");
            Assert.DoesNotThrow(() => token.Dispose());
        }

        /// <summary>Unsubscribing without a handler is a bug and is reported as one.</summary>
        [Test]
        public void UnsubscribingWithoutAHandlerIsReported()
        {
            LogAssert.Expect(LogType.Error, new Regex(nameof(ProbeEvent)));

            _bus.Unsubscribe<ProbeEvent>(null);
        }
    }
}