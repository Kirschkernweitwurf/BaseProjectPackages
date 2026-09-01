using System.Collections.Generic;
using UnityEngine;

namespace Base.CorePackage.Audio
{
    /// <summary>
    /// Tracks the <see cref="AudioSource"/>s currently playing for each
    /// <see cref="AudioContainer"/>, with fast lookups in both directions.
    /// Destroyed sources are pruned lazily so a cleared pool cannot leave stale entries behind.
    /// </summary>
    internal class ActiveSounds
    {
        private readonly Dictionary<AudioContainer, List<AudioSource>> _sourcesByContainer = new();
        private readonly Dictionary<AudioSource, AudioContainer> _containerBySource = new();

        /// <summary>
        /// Registers a source as playing for the given container.
        /// </summary>
        /// <param name="container">The container the source plays a clip of.</param>
        /// <param name="source">The source that started playing.</param>
        internal void Add(AudioContainer container, AudioSource source)
        {
            if (!_sourcesByContainer.TryGetValue(container, out List<AudioSource> sources))
            {
                sources = new List<AudioSource>();
                _sourcesByContainer[container] = sources;
            }

            sources.Add(source);
            _containerBySource[source] = container;
        }

        /// <summary>
        /// Removes a source from tracking. Safe to call for untracked or already destroyed sources.
        /// </summary>
        /// <param name="source">The source to stop tracking.</param>
        internal void Remove(AudioSource source)
        {
            if (!_containerBySource.Remove(source, out AudioContainer container))
                return;

            if (!_sourcesByContainer.TryGetValue(container, out List<AudioSource> sources))
                return;

            sources.Remove(source);

            if (sources.Count == 0)
                _sourcesByContainer.Remove(container);
        }

        /// <summary>
        /// Gets the sources currently playing for a container.
        /// </summary>
        /// <param name="container">The container to look up.</param>
        /// <param name="sources">The sources playing for the container, empty if none are.</param>
        /// <returns>True if at least one source is playing for the container.</returns>
        internal bool TryGetSources(AudioContainer container, out IReadOnlyList<AudioSource> sources)
        {
            sources = null;

            if (!_sourcesByContainer.TryGetValue(container, out List<AudioSource> tracked))
                return false;

            PruneDestroyed(container, tracked);

            if (tracked.Count == 0)
                return false;

            sources = tracked;
            return true;
        }

        /// <summary>
        /// Gets the container a source is playing for, if it is tracked.
        /// </summary>
        /// <param name="source">The source to look up.</param>
        /// <param name="container">The container the source belongs to.</param>
        /// <returns>True if the source is tracked.</returns>
        internal bool TryGetContainer(AudioSource source, out AudioContainer container)
            => _containerBySource.TryGetValue(source, out container);

        /// <summary>
        /// Counts how many live sources are playing for a container.
        /// </summary>
        /// <param name="container">The container to count sources for.</param>
        internal int CountOf(AudioContainer container)
            => TryGetSources(container, out IReadOnlyList<AudioSource> sources)
                ? sources.Count
                : 0;

        /// <summary>
        /// Gets the oldest live source playing for a container, or null if none is.
        /// </summary>
        /// <param name="container">The container to look up.</param>
        internal AudioSource GetOldest(AudioContainer container)
            => TryGetSources(container, out IReadOnlyList<AudioSource> sources)
                ? sources[0]
                : null;

        /// <summary>
        /// Copies every tracked source into the given buffer, so callers can release while iterating.
        /// </summary>
        /// <param name="buffer">The buffer to fill. It is not cleared first.</param>
        internal void CopyAllSourcesTo(List<AudioSource> buffer)
        {
            foreach (AudioSource source in _containerBySource.Keys)
                buffer.Add(source);
        }

        /// <summary>
        /// Drops all tracking without touching the sources themselves.
        /// Call this when the pools were cleared behind the manager's back.
        /// </summary>
        internal void Clear()
        {
            _sourcesByContainer.Clear();
            _containerBySource.Clear();
        }

        /// <summary>
        /// Removes sources that Unity destroyed since they were registered, for example on a scene load.
        /// </summary>
        /// <param name="container">The container the sources belong to.</param>
        /// <param name="sources">The tracked source list of that container.</param>
        private void PruneDestroyed(AudioContainer container, List<AudioSource> sources)
        {
            for (int i = sources.Count - 1; i >= 0; i--)
            {
                if (sources[i] != null)
                    continue;

                _containerBySource.Remove(sources[i]);
                sources.RemoveAt(i);
            }

            if (sources.Count == 0)
                _sourcesByContainer.Remove(container);
        }
    }
}