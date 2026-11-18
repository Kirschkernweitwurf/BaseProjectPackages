using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using NUnit.Framework;
using Base.CorePackage.GameActions;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Base.CorePackage.Tests
{
    /// <summary>
    /// Checks how the action system resolves actions: the pre, on and post windows in order, cancelling,
    /// handler priority, one-shot and conditional handlers and that follow-ups run depth first.
    /// </summary>
    public sealed class GameActionSystemTests
    {
        private const int High = 10;
        private const int Low = 1;

        private readonly List<string> _log = new();

        private GameObject _host;
        private GameActionSystem _system;

        /// <summary>Creates an action system on a fresh object.</summary>
        [SetUp]
        public void SetUp()
        {
            _log.Clear();
            _host = new GameObject(nameof(GameActionSystemTests));
            _system = _host.AddComponent<GameActionSystem>();
        }

        /// <summary>Destroys the system.</summary>
        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_host);

        /// <summary>Pre handlers run before the action, post handlers after it.</summary>
        [Test]
        public void Perform_RunsPreExecutePost()
        {
            _system.SubscribePre<RecordAction>(_ => _log.Add("pre"));
            _system.SubscribeOn<RecordAction>(_ => _log.Add("on"));
            _system.SubscribePost<RecordAction>(_ => _log.Add("post"));

            _system.Perform(new RecordAction(_log, "execute"));

            Assert.That(_log, Is.EqualTo(new[] { "pre", "on", "execute", "post" }));
        }

        /// <summary>A pre handler can cancel the action, so it never executes and no post handler runs.</summary>
        [Test]
        public void Perform_CancelledInPre_SkipsExecuteAndPost()
        {
            _system.SubscribePre<RecordAction>(action => action.Cancel());
            _system.SubscribePost<RecordAction>(_ => _log.Add("post"));
            RecordAction action = new(_log, "execute");

            _system.Perform(action);

            Assert.That(_log, Is.EqualTo(new[] { "cancelled" }));
            Assert.That(action.IsResolved, Is.False);
        }

        /// <summary>Higher priority runs first; equal priorities run in the order they subscribed.</summary>
        [Test]
        public void Perform_HandlersByPriorityThenOrder()
        {
            _system.SubscribePre<RecordAction>(_ => _log.Add("low"), Low);
            _system.SubscribePre<RecordAction>(_ => _log.Add("high first"), High);
            _system.SubscribePre<RecordAction>(_ => _log.Add("high second"), High);

            _system.Perform(new RecordAction(null, null));

            Assert.That(_log, Is.EqualTo(new[] { "high first", "high second", "low" }));
        }

        /// <summary>A one-shot handler reacts to the first action only.</summary>
        [Test]
        public void Perform_OnceHandler_RunsOnce()
        {
            _system.SubscribePost<RecordAction>(_ => _log.Add("once"), once: true);

            _system.Perform(new RecordAction(null, null));
            _system.Perform(new RecordAction(null, null));

            Assert.That(_log, Is.EqualTo(new[] { "once" }));
        }

        /// <summary>A handler whose condition fails is skipped.</summary>
        [Test]
        public void Perform_FailingCondition_SkipsHandler()
        {
            _system.SubscribePost<RecordAction>(_ => _log.Add("matched"), condition: action => action.Name == "yes");

            _system.Perform(new RecordAction(null, "no"));
            _system.Perform(new RecordAction(null, "yes"));

            Assert.That(_log, Is.EqualTo(new[] { "matched" }));
        }

        /// <summary>A disposed subscription no longer reacts.</summary>
        [Test]
        public void Perform_DisposedSubscription_DoesNotReact()
        {
            _system.SubscribePost<RecordAction>(_ => _log.Add("post")).Dispose();

            _system.Perform(new RecordAction(null, null));

            Assert.That(_log, Is.Empty);
        }

        /// <summary>A follow-up performed from a post handler runs before an action queued earlier.</summary>
        [Test]
        public void Perform_FollowUpFromPost_RunsDepthFirst()
        {
            _system.SubscribePost<RecordAction>(action =>
            {
                if (action.Name == "first")
                    _system.Perform(new RecordAction(_log, "follow-up"));
            });

            // Holding the queue with an outer action lets both siblings queue before either resolves.
            _system.SubscribeOn<OuterAction>(_ =>
            {
                _system.Perform(new RecordAction(_log, "first"));
                _system.Perform(new RecordAction(_log, "second"));
            });
            _system.Perform(new OuterAction());

            Assert.That(_log, Is.EqualTo(new[] { "first", "follow-up", "second" }));
        }

        /// <summary>A follow-up performed by a reaction still runs when the action itself throws.</summary>
        [Test]
        public void Perform_ActionThrows_FollowUpStillRuns()
        {
            _system.SubscribeOn<FailingAction>(_ => _system.Perform(new RecordAction(_log, "follow-up")));
            LogAssert.Expect(LogType.Error, new Regex(nameof(FailingAction)));

            _system.Perform(new FailingAction());

            Assert.That(_log, Is.EqualTo(new[] { "follow-up" }));
        }

        /// <summary>A handler on the base action type reacts to every action.</summary>
        [Test]
        public void Perform_BaseTypeSubscription_MatchesConcreteAction()
        {
            _system.SubscribePost<GameAction>(_ => _log.Add("any"));

            _system.Perform(new RecordAction(null, null));

            Assert.That(_log, Is.EqualTo(new[] { "any" }));
        }

        /// <summary>An action that writes its name into a log when it executes or is cancelled.</summary>
        private sealed class RecordAction : GameAction
        {
            private const string CancelledEntry = "cancelled";

            private readonly List<string> _log;

            public RecordAction(List<string> log, string name)
            {
                _log = log;
                Name = name;
            }

            public string Name { get; }

            protected override Awaitable ExecuteAsync(CancellationToken token)
            {
                _log?.Add(Name);
                return Completed();
            }

            protected override void OnCancelled() => _log?.Add(CancelledEntry);
        }

        /// <summary>An action without effect, used to perform others from inside its resolution.</summary>
        private sealed class OuterAction : GameAction
        {
            protected override Awaitable ExecuteAsync(CancellationToken token) => Completed();
        }

        /// <summary>An action whose execution throws, like a broken effect.</summary>
        private sealed class FailingAction : GameAction
        {
            protected override Awaitable ExecuteAsync(CancellationToken token)
                => throw new InvalidOperationException(nameof(FailingAction));
        }

        private static Awaitable Completed()
        {
            AwaitableCompletionSource source = new();
            source.SetResult();
            return source.Awaitable;
        }
    }
}