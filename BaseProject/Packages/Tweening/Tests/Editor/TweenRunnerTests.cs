using System.Collections.Generic;
using Base.TweeningPackage.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.TweeningPackage.Tests
{
    /// <summary>
    /// Covers the bookkeeping the runner does around its active set. The broadcast events are what
    /// tooling listens to, so a tween has to be announced exactly once when it joins and once when it
    /// leaves.
    /// </summary>
    /// <remarks>
    /// The runner advances its tweens from Update, which does not run in an editor test, so the
    /// ticking itself belongs to a play mode test. Only registration is covered here.
    /// </remarks>
    public sealed class TweenRunnerTests
    {
        private GameObject _runnerObject;
        private TweenRunner _runner;
        private List<ITween> _registered;
        private List<ITween> _deregistered;

        /// <summary>Every test starts from a fresh runner with both broadcasts recorded.</summary>
        [SetUp]
        public void Build()
        {
            _runnerObject = EditorUtility.CreateGameObjectWithHideFlags(typeof(TweenRunner).Name,
                HideFlags.HideAndDontSave);
            _runner = _runnerObject.AddComponent<TweenRunner>();
            _registered = new List<ITween>();
            _deregistered = new List<ITween>();

            TweenRunner.OnTweenRegistered += OnRegistered;
            TweenRunner.OnTweenDeregistered += OnDeregistered;
        }

        /// <summary>
        /// Unsubscribes and takes the runner down. The broadcasts are static and nothing clears them,
        /// so a handler left behind would keep firing into a finished test.
        /// </summary>
        [TearDown]
        public void Release()
        {
            TweenRunner.OnTweenRegistered -= OnRegistered;
            TweenRunner.OnTweenDeregistered -= OnDeregistered;

            if (_runnerObject != null)
                Object.DestroyImmediate(_runnerObject);

            _runnerObject = null;
            _runner = null;
        }

        /// <summary>A registered tween is announced once.</summary>
        [Test]
        public void ARegisteredTweenIsAnnounced()
        {
            TweenProbe tween = new();

            _runner.RegisterTween(tween);

            Assert.That(_registered, Is.EqualTo(new ITween[] { tween }));
        }

        /// <summary>Registering the same tween twice announces it once.</summary>
        [Test]
        public void RegisteringTwiceAnnouncesOnce()
        {
            TweenProbe tween = new();

            _runner.RegisterTween(tween);
            _runner.RegisterTween(tween);

            Assert.That(_registered.Count, Is.EqualTo(1));
        }

        /// <summary>A tween leaving the active set is announced once.</summary>
        [Test]
        public void ADeregisteredTweenIsAnnounced()
        {
            TweenProbe tween = new();

            _runner.RegisterTween(tween);
            _runner.UnregisterTween(tween);

            Assert.That(_deregistered, Is.EqualTo(new ITween[] { tween }));
        }

        /// <summary>Removing a tween that was never registered announces nothing.</summary>
        [Test]
        public void RemovingAStrangerAnnouncesNothing()
        {
            _runner.UnregisterTween(new TweenProbe());

            Assert.That(_deregistered, Is.Empty);
        }

        /// <summary>Removing a tween twice announces once.</summary>
        [Test]
        public void RemovingTwiceAnnouncesOnce()
        {
            TweenProbe tween = new();

            _runner.RegisterTween(tween);
            _runner.UnregisterTween(tween);
            _runner.UnregisterTween(tween);

            Assert.That(_deregistered.Count, Is.EqualTo(1));
        }

        /// <summary>A removed tween can join again.</summary>
        [Test]
        public void ARemovedTweenCanJoinAgain()
        {
            TweenProbe tween = new();

            _runner.RegisterTween(tween);
            _runner.UnregisterTween(tween);
            _runner.RegisterTween(tween);

            Assert.That(_registered.Count, Is.EqualTo(2));
        }

        /// <summary>Nothing to register is ignored rather than walked into.</summary>
        [Test]
        public void NothingToRegisterIsIgnored()
        {
            _runner.RegisterTween(null);
            _runner.UnregisterTween(null);

            Assert.That(_registered, Is.Empty);
            Assert.That(_deregistered, Is.Empty);
        }

        private void OnRegistered(ITween tween) => _registered.Add(tween);

        private void OnDeregistered(ITween tween) => _deregistered.Add(tween);
    }
}