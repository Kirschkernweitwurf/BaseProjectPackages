using Managers;
using UnityEngine;
using Utility.Logging;

namespace Systems.ObjectPooling
{
    /// <summary>
    /// Base class for global object pool managers.
    /// Provides lifecycle control and easy access to pooled Unity objects.
    /// </summary>
    /// <typeparam name="TAsset">The Unity object type to pool.</typeparam>
    /// <typeparam name = "TPool">The type of the pool manager.</typeparam>
    [DefaultExecutionOrder(-1)]
    public abstract class BaseObjectPoolManager<TAsset, TPool> : CustomSingleton<TPool>
        where TAsset : Object
        where TPool : BaseObjectPoolManager<TAsset, TPool>
    {
        [Header("Pooling Settings")]
        [Tooltip("Prefab to instantiate when new objects are needed.")]
        [SerializeField] protected TAsset prefab;

        [Tooltip("Optional parent for pooled objects.")]
        [SerializeField] protected Transform poolParent;

        [Tooltip("Optional number of instances to prewarm on startup.")]
        [SerializeField] private int prewarmCount;

        private HashSetObjectPool<TAsset> _pool;

        protected override void Awake()
        {
            base.Awake();

            if (prefab == null)
            {
                CustomLogger.LogError("Prefab reference is null. Cannot initialize pool.", this);
                return;
            }

            _pool = CreatePoolInstance();

            if (prewarmCount > 0)
                Prewarm(prewarmCount);
        }

        /// <summary>
        /// Gets an instance from the pool.
        /// </summary>
        /// <returns>The pooled instance.</returns>
        public virtual TAsset Get()
        {
            if (_pool != null)
                return _pool.Get();

            CustomLogger.LogError("Pool not initialized.", this);
            return null;
        }

        /// <summary>
        /// Releases an instance back to the pool.
        /// </summary>
        /// <param name="instance">The instance to release.</param>
        public virtual void Release(TAsset instance)
        {
            if (_pool == null)
            {
                CustomLogger.LogError("Pool not initialized.", this);
                return;
            }

            _pool.Release(instance);
        }

        /// <summary>
        /// Creates the pool instance. Override to customize pool behavior.
        /// </summary>
        /// <returns>The created pool instance.</returns>
        protected virtual HashSetObjectPool<TAsset> CreatePoolInstance() => new(prefab, poolParent, ResetInstance);

        /// <summary>
        /// Resets the instance before returning it to the pool.
        /// </summary>
        /// <param name="instance">The instance to reset.</param>
        protected virtual void ResetInstance(TAsset instance)
        {
            Transform t = GetTransform(instance);
            t?.SetParent(poolParent, false);
        }

        /// <summary>
        /// Gets the Transform component from a Unity Object if possible.
        /// </summary>
        /// <param name="obj">The Unity Object.</param>
        /// <returns>The Transform component or null.</returns>
        private static Transform GetTransform(Object obj)
        {
            return obj switch
            {
                GameObject go => go.transform,
                Component comp => comp.transform,
                _ => null
            };
        }

        private void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                TAsset instance = _pool.Get();
                _pool.Release(instance);
            }
        }
    }
}