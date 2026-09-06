using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Base.AudioPackage.Tests
{
    /// <summary>
    /// The table that decides which sources a stop, a fade or a play limit applies to. It is the one
    /// place that can hand the same source out twice or hold on to one that a scene load already
    /// destroyed, so both directions of the lookup and the pruning are covered here.
    /// </summary>
    public sealed class ActiveSoundsTests
    {
        private const string RootName = "ActiveSoundsTests";

        private ActiveSounds _activeSounds;
        private AudioContainer _container;
        private AudioContainer _otherContainer;
        private GameObject _root;

        /// <summary>A fresh table and fresh containers per test, so nothing leaks between them.</summary>
        [SetUp]
        public void Prepare()
        {
            _activeSounds = new ActiveSounds();
            _root = new GameObject(RootName);
            _container = AudioTestObjects.CreateContainer();
            _otherContainer = AudioTestObjects.CreateContainer();
        }

        /// <summary>The containers are never saved and the sources are real scene objects.</summary>
        [TearDown]
        public void Cleanup()
        {
            if (_root != null)
                Object.DestroyImmediate(_root);

            if (_container != null)
                Object.DestroyImmediate(_container);

            if (_otherContainer != null)
                Object.DestroyImmediate(_otherContainer);

            _activeSounds = null;
            _root = null;
            _container = null;
            _otherContainer = null;
        }

        /// <summary>
        /// Nothing has played yet, so a lookup has to come back empty rather than with a null list the
        /// caller then iterates.
        /// </summary>
        [Test]
        public void AContainerThatNeverPlayedHasNoSources()
        {
            Assert.That(_activeSounds.TryGetSources(_container, out IReadOnlyList<AudioSource> sources), Is.False);
            Assert.That(sources, Is.Null);
            Assert.That(_activeSounds.CountOf(_container), Is.EqualTo(0));
            Assert.That(_activeSounds.GetOldest(_container), Is.Null);
        }

        /// <summary>A registered source is findable from the container and the container from the source.</summary>
        [Test]
        public void ARegisteredSourceIsFindableFromBothEnds()
        {
            AudioSource source = AudioTestObjects.CreateSource(_root.transform);

            _activeSounds.Add(_container, source);

            Assert.That(_activeSounds.TryGetSources(_container, out IReadOnlyList<AudioSource> sources), Is.True);
            Assert.That(sources, Has.Member(source));
            Assert.That(_activeSounds.TryGetContainer(source, out AudioContainer found), Is.True);
            Assert.That(found, Is.EqualTo(_container));
        }

        /// <summary>
        /// The play limit is per container, so a count has to ignore what a different container is
        /// playing rather than counting everything that is live.
        /// </summary>
        [Test]
        public void TheCountIsPerContainer()
        {
            _activeSounds.Add(_container, AudioTestObjects.CreateSource(_root.transform));
            _activeSounds.Add(_container, AudioTestObjects.CreateSource(_root.transform));
            _activeSounds.Add(_otherContainer, AudioTestObjects.CreateSource(_root.transform));

            Assert.That(_activeSounds.CountOf(_container), Is.EqualTo(2));
            Assert.That(_activeSounds.CountOf(_otherContainer), Is.EqualTo(1));
        }

        /// <summary>
        /// The play limit releases the oldest source, so registration order is what decides which one
        /// gets cut off. Insertion order is the whole guarantee.
        /// </summary>
        [Test]
        public void TheOldestSourceIsTheOneRegisteredFirst()
        {
            AudioSource first = AudioTestObjects.CreateSource(_root.transform);
            AudioSource second = AudioTestObjects.CreateSource(_root.transform);

            _activeSounds.Add(_container, first);
            _activeSounds.Add(_container, second);

            Assert.That(_activeSounds.GetOldest(_container), Is.EqualTo(first));
        }

        /// <summary>Removing the oldest promotes the next one instead of leaving a hole.</summary>
        [Test]
        public void RemovingTheOldestPromotesTheNextOne()
        {
            AudioSource first = AudioTestObjects.CreateSource(_root.transform);
            AudioSource second = AudioTestObjects.CreateSource(_root.transform);

            _activeSounds.Add(_container, first);
            _activeSounds.Add(_container, second);
            _activeSounds.Remove(first);

            Assert.That(_activeSounds.GetOldest(_container), Is.EqualTo(second));
        }

        /// <summary>
        /// A released source has to disappear from both maps. Leaving it in the source map would let a
        /// later release hand the same instance back to the pool a second time.
        /// </summary>
        [Test]
        public void RemovingASourceClearsBothDirections()
        {
            AudioSource source = AudioTestObjects.CreateSource(_root.transform);

            _activeSounds.Add(_container, source);
            _activeSounds.Remove(source);

            Assert.That(_activeSounds.TryGetContainer(source, out AudioContainer _), Is.False);
            Assert.That(_activeSounds.CountOf(_container), Is.EqualTo(0));
        }

        /// <summary>
        /// Release runs on a source that may already have been removed, for example after the pools were
        /// cleared, so a second removal has to be a no-op rather than a throw.
        /// </summary>
        [Test]
        public void RemovingAnUntrackedSourceDoesNothing()
        {
            AudioSource source = AudioTestObjects.CreateSource(_root.transform);

            Assert.DoesNotThrow(() => _activeSounds.Remove(source));

            _activeSounds.Add(_container, source);
            _activeSounds.Remove(source);

            Assert.DoesNotThrow(() => _activeSounds.Remove(source));
        }

        /// <summary>
        /// A container whose last source went away has to read as not playing, so a fade or stop reports
        /// that instead of walking an empty list.
        /// </summary>
        [Test]
        public void AContainerWithNoSourcesLeftReadsAsNotPlaying()
        {
            AudioSource source = AudioTestObjects.CreateSource(_root.transform);

            _activeSounds.Add(_container, source);
            _activeSounds.Remove(source);

            Assert.That(_activeSounds.TryGetSources(_container, out IReadOnlyList<AudioSource> _), Is.False);
        }

        /// <summary>
        /// A scene load destroys pooled sources without telling the table, so a lookup afterwards has to
        /// drop them rather than report a destroyed source as playing.
        /// </summary>
        [Test]
        public void ADestroyedSourceIsDroppedOnTheNextLookup()
        {
            AudioSource source = AudioTestObjects.CreateSource(_root.transform);

            _activeSounds.Add(_container, source);
            Object.DestroyImmediate(source.gameObject);

            Assert.That(_activeSounds.CountOf(_container), Is.EqualTo(0));
            Assert.That(_activeSounds.TryGetSources(_container, out IReadOnlyList<AudioSource> _), Is.False);
        }

        /// <summary>
        /// Only the destroyed source goes. A sibling that survived the scene load still has to be
        /// stoppable, which it would not be if the whole container entry was dropped.
        /// </summary>
        [Test]
        public void ADestroyedSourceLeavesItsLiveSiblingsAlone()
        {
            AudioSource destroyed = AudioTestObjects.CreateSource(_root.transform);
            AudioSource alive = AudioTestObjects.CreateSource(_root.transform);

            _activeSounds.Add(_container, destroyed);
            _activeSounds.Add(_container, alive);
            Object.DestroyImmediate(destroyed.gameObject);

            Assert.That(_activeSounds.CountOf(_container), Is.EqualTo(1));
            Assert.That(_activeSounds.GetOldest(_container), Is.EqualTo(alive));
        }

        /// <summary>
        /// Stopping everything releases while iterating, so the copy is what keeps that from mutating the
        /// list it is walking. It appends, because the caller hands in a pooled buffer.
        /// </summary>
        [Test]
        public void CopyingEverySourceAppendsToTheBuffer()
        {
            AudioSource existing = AudioTestObjects.CreateSource(_root.transform);
            List<AudioSource> buffer = new() { existing };

            _activeSounds.Add(_container, AudioTestObjects.CreateSource(_root.transform));
            _activeSounds.Add(_otherContainer, AudioTestObjects.CreateSource(_root.transform));
            _activeSounds.CopyAllSourcesTo(buffer);

            Assert.That(buffer.Count, Is.EqualTo(3));
            Assert.That(buffer[0], Is.EqualTo(existing));
        }

        /// <summary>
        /// Clearing the pools hands every source back at once, so the table has to forget all of them.
        /// Anything left over would be released a second time on the next stop.
        /// </summary>
        [Test]
        public void ClearingForgetsEveryContainerAndSource()
        {
            AudioSource source = AudioTestObjects.CreateSource(_root.transform);

            _activeSounds.Add(_container, source);
            _activeSounds.Add(_otherContainer, AudioTestObjects.CreateSource(_root.transform));
            _activeSounds.Clear();

            Assert.That(_activeSounds.CountOf(_container), Is.EqualTo(0));
            Assert.That(_activeSounds.CountOf(_otherContainer), Is.EqualTo(0));
            Assert.That(_activeSounds.TryGetContainer(source, out AudioContainer _), Is.False);
        }

        /// <summary>Clearing leaves the sources themselves untouched, they belong to the pool.</summary>
        [Test]
        public void ClearingDoesNotDestroyTheSources()
        {
            AudioSource source = AudioTestObjects.CreateSource(_root.transform);

            _activeSounds.Add(_container, source);
            _activeSounds.Clear();

            Assert.That(source != null, Is.True);
        }
    }
}