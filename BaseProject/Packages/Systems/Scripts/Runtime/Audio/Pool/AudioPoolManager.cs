using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utility.Logging;

namespace Systems.Audio.Pool
{
    /// <summary>
    /// Manages multiple audio pools.
    /// </summary>
    public class AudioPoolManager : MonoBehaviour
    {
        [Space] [SerializeField] private Transform poolParent;

        [Space]
        [Tooltip("If true, clears all pools when a new scene is loaded.")]
        [SerializeField]
        private bool isClearingPoolAfterSceneLoad;

        [Header("Prefabs")]
        [SerializeField] private AudioSource audioSource2DPrefab;

        [SerializeField] private AudioSource audioSource3DPrefab;
        [SerializeField] private AudioSource audioSourceMusicPrefab;
        [SerializeField] private AudioSource audioSourceUiPrefab;

        [Header("Pool Settings")]
        [SerializeField] private int avgPoolSize2d;

        [SerializeField] private int maxPoolSize2d;
        [SerializeField] private int avgPoolSize3d;
        [SerializeField] private int maxPoolSize3d;
        [SerializeField] private int avgPoolSizeM;
        [SerializeField] private int maxPoolSizeM;
        [SerializeField] private int avgPoolSizeUi;
        [SerializeField] private int maxPoolSizeUi;

        private readonly Dictionary<EAudioType, AudioPool> _audioPools = new();

        private void Awake()
        {
            SceneManager.activeSceneChanged += OnSceneChanged;
            InitializePools();
        }

        private void OnDestroy()
        {
            SceneManager.activeSceneChanged -= OnSceneChanged;
        }

        /// <summary>
        /// Gets an audio source from the pool for the specified type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public AudioSource GetAudioSource(EAudioType type)
        {
            return _audioPools.TryGetValue(type, out AudioPool pool) ? pool.GetSource() : null;
        }

        /// <summary>
        /// Releases an audio source back to the pool for the specified type.s
        /// </summary>
        /// <param name="type"></param>
        /// <param name="source"></param>
        public void ReleaseAudioSource(EAudioType type, AudioSource source)
        {
            if (_audioPools.TryGetValue(type, out AudioPool pool))
            {
                pool.ReleaseSource(source);
            }
        }

        /// <summary>
        /// Clears the pool for the specified type.
        /// </summary>
        /// <param name="type"></param>
        public void ClearPool(EAudioType type)
        {
            if (!_audioPools.TryGetValue(type, out AudioPool pool))
            {
                CustomLogger.LogWarning("Pool not found for type: " + type, this);
                return;
            }

            pool.ClearPool();
        }

        private void OnSceneChanged(Scene _, Scene __)
        {
            if (isClearingPoolAfterSceneLoad)
            {
                ClearPools();
            }
        }

        /// <summary>
        /// Clears all audio pools.
        /// </summary>
        private void ClearPools()
        {
            foreach (AudioPool pool in _audioPools.Values)
            {
                pool.ClearPool();
            }
        }

        /// <summary>
        /// Sets up the audio pools.
        /// </summary>
        private void InitializePools()
        {
            _audioPools[EAudioType.Sfx2D] =
                new AudioPool(audioSource2DPrefab, poolParent, avgPoolSize2d, maxPoolSize2d);
            _audioPools[EAudioType.UI] = new AudioPool(audioSourceUiPrefab, poolParent, avgPoolSizeUi, maxPoolSizeUi);
            _audioPools[EAudioType.Sfx3D] =
                new AudioPool(audioSource3DPrefab, poolParent, avgPoolSize3d, maxPoolSize3d);
            _audioPools[EAudioType.Music] =
                new AudioPool(audioSourceMusicPrefab, poolParent, avgPoolSizeM, maxPoolSizeM);
        }
    }
}