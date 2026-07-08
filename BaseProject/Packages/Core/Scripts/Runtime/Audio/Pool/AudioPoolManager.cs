using System.Collections.Generic;
using Base.CorePackage.ObjectPooling;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Base.CorePackage.Audio.Pool
{
    /// <summary>
    /// Manages one pooled set of AudioSources per <see cref="EAudioType"/>.
    /// </summary>
    public class AudioPoolManager : MonoBehaviour
    {
        [Space]
        [SerializeField] private Transform poolParent;

        [Space]
        [Tooltip("If true, clears all pools when a new scene is loaded.")]
        [SerializeField] private bool isClearingPoolAfterSceneLoad;

        [Header("Prefabs")]

        [SerializeField] private AudioSource audioSource2DPrefab;
        [SerializeField] private AudioSource audioSource3DPrefab;
        [SerializeField] private AudioSource audioSourceMusicPrefab;
        [SerializeField] private AudioSource audioSourceUiPrefab;

        private readonly Dictionary<EAudioType, HashSetObjectPool<AudioSource>> _pools = new();

#region Unity Callbacks
        private void Awake()
        {
            SceneManager.activeSceneChanged += OnSceneChanged;
            InitializePools();
        }

        private void OnDestroy() => SceneManager.activeSceneChanged -= OnSceneChanged;
#endregion

        /// <summary>
        /// Gets an audio source from the pool for the given type, or null if the type is unknown.
        /// </summary>
        /// <param name="type">The audio type to retrieve a source for.</param>
        public AudioSource GetAudioSource(EAudioType type)
            => _pools.TryGetValue(type, out HashSetObjectPool<AudioSource> pool)
                ? pool.Get()
                : null;

        /// <summary>
        /// Returns an audio source to the pool for the given type.
        /// </summary>
        /// <param name="type">The audio type the source belongs to.</param>
        /// <param name="source">The source to release.</param>
        public void ReleaseAudioSource(EAudioType type, AudioSource source)
        {
            if (_pools.TryGetValue(type, out HashSetObjectPool<AudioSource> pool))
                pool.Release(source);
        }

        /// <summary>
        /// Releases every active source for the given type back to its pool.
        /// </summary>
        /// <param name="type">The audio type to clear.</param>
        public void ClearPool(EAudioType type)
        {
            if (_pools.TryGetValue(type, out HashSetObjectPool<AudioSource> pool))
                pool.ReleaseAll();
            else
                CustomLogger.LogWarning("Pool not found for type: " + type, this);
        }

        /// <summary>
        /// Stops a source before it is returned to the pool.
        /// </summary>
        /// <param name="source">The source to stop.</param>
        private static void StopSource(AudioSource source)
        {
            if (source != null)
                source.Stop();
        }

        private void OnSceneChanged(Scene _, Scene __)
        {
            if (isClearingPoolAfterSceneLoad)
                ClearPools();
        }

        /// <summary>
        /// Releases every active source across all pools.
        /// </summary>
        private void ClearPools()
        {
            foreach (HashSetObjectPool<AudioSource> pool in _pools.Values)
                pool.ReleaseAll();
        }

        /// <summary>
        /// Creates one pool per audio type.
        /// </summary>
        private void InitializePools()
        {
            _pools[EAudioType.Sfx2D] = CreatePool(audioSource2DPrefab);
            _pools[EAudioType.Sfx3D] = CreatePool(audioSource3DPrefab);
            _pools[EAudioType.Music] = CreatePool(audioSourceMusicPrefab);
            _pools[EAudioType.UI] = CreatePool(audioSourceUiPrefab);
        }

        /// <summary>
        /// Creates a pool that stops each source when it is released.
        /// </summary>
        /// <param name="prefab">The AudioSource prefab to pool.</param>
        private HashSetObjectPool<AudioSource> CreatePool(AudioSource prefab) => new(prefab, poolParent, StopSource);
    }
}