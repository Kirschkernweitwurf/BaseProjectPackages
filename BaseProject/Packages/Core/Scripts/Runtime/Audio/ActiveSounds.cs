using System.Collections.Generic;
using UnityEngine;

namespace Base.CorePackage.Audio
{
    /// <summary>
    /// Tracks the <see cref="AudioSource"/>s currently playing for each
    /// <see cref="AudioContainer"/>, with fast lookups in both directions.
    /// </summary>
    internal class ActiveSounds
    {
        private readonly Dictionary<AudioContainer, List<AudioSource>> _sourcesByContainer = new();
        private readonly Dictionary<AudioSource, AudioContainer> _containerBySource = new();

        /// <summary>
        /// Registers a source as playing for the given container.
        /// </summary>
        public void Add(AudioContainer container, AudioSource source)
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
        /// Removes a source from tracking. Safe to call for untracked sources.
        /// </summary>
        public void Remove(AudioSource source)
        {
            if (source == null || !_containerBySource.Remove(source, out AudioContainer container))
                return;

            if (!_sourcesByContainer.TryGetValue(container, out List<AudioSource> sources))
                return;

            sources.Remove(source);
            if (sources.Count == 0)
                _sourcesByContainer.Remove(container);
        }

        /// <summary>
        /// Gets the sources currently playing for a container, or null if none.
        /// </summary>
        public IReadOnlyList<AudioSource> GetSources(AudioContainer container)
            => _sourcesByContainer.GetValueOrDefault(container);

        /// <summary>
        /// Gets the container a source is playing for, if it is tracked.
        /// </summary>
        public bool TryGetContainer(AudioSource source, out AudioContainer container)
            => _containerBySource.TryGetValue(source, out container);

        /// <summary>
        /// Counts how many sources are playing for a container.
        /// </summary>
        public int CountOf(AudioContainer container)
            => _sourcesByContainer.TryGetValue(container, out List<AudioSource> sources)
                ? sources.Count
                : 0;

        /// <summary>
        /// Gets the oldest source playing for a container, or null if none.
        /// </summary>
        public AudioSource GetOldest(AudioContainer container)
            => _sourcesByContainer.TryGetValue(container, out List<AudioSource> sources) && sources.Count > 0
                ? sources[0]
                : null;
    }
}