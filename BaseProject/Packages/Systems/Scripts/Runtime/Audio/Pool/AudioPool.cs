using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Utility.Logging;

namespace Systems.Audio.Pool
{
    /// <summary>
    /// Manages object pooling for AudioSources to optimize performance.
    /// </summary>
    public class AudioPool
    {
        private readonly Dictionary<AudioSource, bool> _activeSources = new();
        private readonly ObjectPool<AudioSource> _pool;

        /// <summary>
        /// Creates a new AudioPool with the specified prefab, parent, default size and maximum size.
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="parent"></param>
        /// <param name="defaultSize"></param>
        /// <param name="maxSize"></param>
        public AudioPool(AudioSource prefab, Transform parent, int defaultSize, int maxSize)
        {
            _pool = new ObjectPool<AudioSource>(
                createFunc: () => Object.Instantiate(prefab, parent),
                actionOnGet: ActivateSource,
                actionOnRelease: DeactivateSource,
                actionOnDestroy: src => Object.Destroy(src.gameObject),
                collectionCheck: true,
                defaultCapacity: defaultSize,
                maxSize: maxSize
            );
        }

        /// <summary>
        /// Activates an AudioSource and adds it to the active sources list.
        /// </summary>
        /// <param name="src"></param>
        private void ActivateSource(AudioSource src)
        {
            if (src == null)
            {
                CustomLogger.LogWarning("AudioSource is null when activating.", null);
                return;
            }

            src.gameObject.SetActive(true);
            _activeSources[src] = true;
        }

        /// <summary>
        /// Deactivates an AudioSource and removes it from the active sources list.
        /// </summary>
        /// <param name="src"></param>
        private void DeactivateSource(AudioSource src)
        {
            if (src == null)
            {
                CustomLogger.LogWarning("AudioSource is null when deactivating.", null);
                return;
            }

            src.Stop();
            src.gameObject.SetActive(false);
            _activeSources.Remove(src);
        }

        /// <summary>
        /// Retrieves an AudioSource from the pool.
        /// </summary>
        /// <returns>Available AudioSource</returns>
        public AudioSource GetSource() => _pool.Get();

        /// <summary>
        /// Returns an AudioSource to the pool.
        /// </summary>
        /// <param name="source">The AudioSource to release.</param>
        public void ReleaseSource(AudioSource source)
        {
            if (_activeSources.ContainsKey(source))
            {
                _pool.Release(source);
            }
        }

        /// <summary>
        /// Clears the pool, destroying all instances.
        /// </summary>
        public void ClearPool()
        {
            _pool.Clear();
            _activeSources.Clear();
        }
    }
}