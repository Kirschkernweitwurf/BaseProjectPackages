using Base.CorePackage.ObjectPooling;
using UnityEngine;

namespace Base.CorePackage.Audio.Pool
{
    /// <summary>
    /// Manages object pooling for AudioSources to optimize performance.
    /// </summary>
    public class AudioPool
    {
        private readonly HashSetObjectPool<AudioSource> _pool;

        /// <summary>
        /// Creates a new AudioPool with the specified prefab, parent, default size and maximum size.
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="parent"></param>
        /// <param name="defaultSize"></param>
        public AudioPool(AudioSource prefab, Transform parent, int defaultSize)
        {
            // HashSetObjectPool grows on demand and is not capped, so maxSize is not enforced.
            _pool = new HashSetObjectPool<AudioSource>(prefab, parent, ResetSource);

            Prewarm(defaultSize);
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
            if (source != null)
                _pool.Release(source);
        }

        /// <summary>
        /// Clears the pool by releasing all active instances back to it.
        /// </summary>
        public void ClearPool() => _pool.ReleaseAll();

        /// <summary>
        /// Resets an AudioSource before it is returned to the pool.
        /// </summary>
        /// <param name="src"></param>
        private static void ResetSource(AudioSource src)
        {
            if (src != null)
                src.Stop();
        }

        /// <summary>
        /// Prewarms the pool by creating and immediately releasing instances.
        /// </summary>
        /// <param name="count">Number of instances to prewarm.</param>
        private void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                AudioSource instance = _pool.Get();
                _pool.Release(instance);
            }
        }
    }
}