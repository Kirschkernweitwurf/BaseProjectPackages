using System;
using System.Collections.Generic;
using Base.AttributesPackage;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable UnusedMember.Global

namespace Base.AudioPackage.Pool
{
    /// <summary>
    /// Owns one <see cref="AudioPool"/> per <see cref="EAudioType"/> and hands sources out by type.
    /// </summary>
    public class AudioPoolManager : MonoBehaviour
    {
        /// <summary>
        /// Raised after pools were cleared, so listeners can drop their references to the released sources.
        /// </summary>
        public event Action PoolsCleared;

        [Title("Setup")]
        [Tooltip("Parent for all pooled audio sources. Falls back to this transform when left empty.")]
        [SerializeField] private Transform poolParent;

        [Tooltip("How many audio sources each pool creates up front, so the first sounds do not cause a hitch.")]
        [Min(0)] [SerializeField] private int prewarmCount = 4;

        [Tooltip("If true, clears all pools when a new scene is loaded.")]
        [SerializeField] private bool isClearingPoolAfterSceneLoad;

        [Title("Prefabs")]
        [Required] [SerializeField] private AudioSource audioSource2DPrefab;
        [Required] [SerializeField] private AudioSource audioSource3DPrefab;
        [Required] [SerializeField] private AudioSource audioSourceMusicPrefab;
        [Required] [SerializeField] private AudioSource audioSourceUiPrefab;

        /// <summary>
        /// The transform pooled sources are parented to. The field is optional, so this falls back to
        /// the manager itself instead of leaving instances loose in the scene root.
        /// </summary>
        private Transform PoolParent => poolParent != null
            ? poolParent
            : transform;

        private readonly Dictionary<EAudioType, AudioPool> _pools = new();

#region Unity Callbacks
        private void Awake()
        {
            SceneManager.activeSceneChanged += OnSceneChanged;

            InitializePools();
            ValidatePools();
        }

        private void OnDestroy() => SceneManager.activeSceneChanged -= OnSceneChanged;
#endregion

        /// <summary>
        /// Gets an audio source from the pool for the given type.
        /// </summary>
        /// <param name="type">The audio type to retrieve a source for.</param>
        /// <returns>A pooled source, or null if the type has no pool.</returns>
        public AudioSource GetAudioSource(EAudioType type) => _pools.TryGetValue(type, out AudioPool pool)
            ? pool.Get()
            : null;

        /// <summary>
        /// Returns an audio source to the pool for the given type.
        /// </summary>
        /// <param name="type">The audio type the source belongs to.</param>
        /// <param name="source">The source to release.</param>
        public void ReleaseAudioSource(EAudioType type, AudioSource source)
        {
            if (_pools.TryGetValue(type, out AudioPool pool))
                pool.Release(source);
        }

        /// <summary>
        /// Releases every active source for the given type back to its pool.
        /// </summary>
        /// <param name="type">The audio type to clear.</param>
        public void ClearPool(EAudioType type)
        {
            if (!_pools.TryGetValue(type, out AudioPool pool))
            {
                CustomLogger.LogWarning($"No pool found for {nameof(EAudioType)}.{type}.", this);
                return;
            }

            pool.ReleaseAll();
            PoolsCleared?.Invoke();
        }

        /// <summary>
        /// Releases every active source across all pools.
        /// </summary>
        public void ClearPools()
        {
            foreach (AudioPool pool in _pools.Values)
                pool.ReleaseAll();

            PoolsCleared?.Invoke();
        }

        /// <summary>
        /// Clears the pools on a scene change, if that is enabled.
        /// </summary>
        private void OnSceneChanged(Scene _, Scene __)
        {
            if (isClearingPoolAfterSceneLoad)
                ClearPools();
        }

        /// <summary>
        /// Creates one pool per audio type. Types without a prefab are skipped and reported by
        /// <see cref="ValidatePools"/>.
        /// </summary>
        private void InitializePools()
        {
            TryCreatePool(EAudioType.Sfx2D, audioSource2DPrefab);
            TryCreatePool(EAudioType.Sfx3D, audioSource3DPrefab);
            TryCreatePool(EAudioType.Music, audioSourceMusicPrefab);
            TryCreatePool(EAudioType.Ui, audioSourceUiPrefab);
        }

        /// <summary>
        /// Creates a pool for one type, unless its prefab is missing. A missing prefab is already reported
        /// by <see cref="RequiredAttribute"/>, so this stays quiet and lets validation do the talking.
        /// </summary>
        /// <param name="type">The audio type the pool serves.</param>
        /// <param name="prefab">The prefab to pool.</param>
        private void TryCreatePool(EAudioType type, AudioSource prefab)
        {
            if (prefab == null)
                return;

            _pools[type] = new AudioPool(type, prefab, PoolParent, prewarmCount);
        }

        /// <summary>
        /// Reports every <see cref="EAudioType"/> that ended up without a pool, once, at startup.
        /// This catches both an unassigned prefab and a new enum entry nobody wired up yet.
        /// </summary>
        private void ValidatePools()
        {
            foreach (EAudioType type in Enum.GetValues(typeof(EAudioType)))
            {
                if (_pools.ContainsKey(type))
                    continue;

                CustomLogger.LogError($"No pool for {nameof(EAudioType)}.{type}. Every sound of that type will be"
                    + $" silent. Assign its prefab on {name} or add it to {nameof(InitializePools)}.", this);
            }
        }
    }
}