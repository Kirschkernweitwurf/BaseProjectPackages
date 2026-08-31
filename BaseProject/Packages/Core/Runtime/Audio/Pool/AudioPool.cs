using System.Collections.Generic;
using Base.UtilityPackage.Logging;
using Base.UtilityPackage.Pooling;
using UnityEngine;
using UnityEngine.Pool;

namespace Base.CorePackage.Audio.Pool
{
    /// <summary>
    /// Pools the <see cref="AudioSource"/>s of exactly one <see cref="EAudioType"/>.
    /// </summary>
    internal class AudioPool
    {
        private readonly HashSetObjectPool<AudioSource> _pool;
        private readonly EAudioType _audioType;

        /// <summary>
        /// Creates a pool for one audio type and prewarms it.
        /// </summary>
        /// <param name="audioType">The type this pool serves. Used for logging only.</param>
        /// <param name="prefab">The AudioSource prefab to instantiate.</param>
        /// <param name="parent">The transform new instances are parented to.</param>
        /// <param name="prewarmCount">How many instances to create up front.</param>
        public AudioPool(EAudioType audioType, AudioSource prefab, Transform parent, int prewarmCount)
        {
            _audioType = audioType;
            _pool = new HashSetObjectPool<AudioSource>(prefab, parent, StopSource);

            Prewarm(prewarmCount);
        }

        /// <summary>
        /// Retrieves an AudioSource from the pool.
        /// </summary>
        /// <returns>An available AudioSource.</returns>
        public AudioSource Get() => _pool.Get();

        /// <summary>
        /// Returns an AudioSource to the pool.
        /// </summary>
        /// <param name="source">The AudioSource to release.</param>
        public void Release(AudioSource source)
        {
            if (source == null)
            {
                CustomLogger.LogWarning($"Tried releasing a null {nameof(AudioSource)} into the {_audioType} pool.",
                    null);

                return;
            }

            _pool.Release(source);
        }

        /// <summary>
        /// Releases every active instance back into the pool.
        /// </summary>
        public void ReleaseAll() => _pool.ReleaseAll();

        /// <summary>
        /// Stops an AudioSource before it goes back into the pool.
        /// </summary>
        /// <param name="source">The source being released.</param>
        private static void StopSource(AudioSource source)
        {
            if (source != null)
                source.Stop();
        }

        /// <summary>
        /// Prewarms the pool. All instances are taken first and released afterward, because releasing each
        /// one right away would just hand the same single instance back on the next take.
        /// </summary>
        /// <param name="count">Number of instances to prewarm.</param>
        private void Prewarm(int count)
        {
            if (count <= 0)
                return;

            List<AudioSource> instances = ListPool<AudioSource>.Get();

            while (instances.Count < count)
                instances.Add(_pool.Get());

            foreach (AudioSource instance in instances)
                _pool.Release(instance);

            ListPool<AudioSource>.Release(instances);
        }
    }
}