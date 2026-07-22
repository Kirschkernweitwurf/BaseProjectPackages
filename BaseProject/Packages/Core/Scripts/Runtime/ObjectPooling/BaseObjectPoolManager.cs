using Base.AttributePackage;
using Base.UtilityPackage;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.CorePackage.ObjectPooling
{
    /// <summary>
    /// Base class for global object pool managers.
    /// Provides lifecycle control and easy access to pooled Unity objects.
    /// </summary>
    /// <typeparam name="TAsset">The Unity object type to pool.</typeparam>
    /// <typeparam name="TPool">The type of the pool manager.</typeparam>
    [DefaultExecutionOrder(-1)]
    public abstract class BaseObjectPoolManager<TAsset, TPool> : CustomSingleton<TPool>
        where TAsset : Object
        where TPool : BaseObjectPoolManager<TAsset, TPool>
    {
        [Header("Pooling Settings")]

        [Tooltip("Prefab to instantiate when new objects are needed.")]
        [Required] [SerializeField] protected TAsset prefab;

        [Tooltip("Optional parent where pooled objects will be instantiated.")]
        [SerializeField] protected Transform poolParent;

        [Tooltip("Optional number of instances to prewarm on startup.")]
        [SerializeField] private int prewarmCount;

        /// <summary>
        /// The object pool instance. This is where pooled objects are managed.
        /// </summary>
        public HashSetObjectPool<TAsset> Pool { get; private set; }

#region Unity Callbacks
        protected override void Awake()
        {
            base.Awake();

            Pool = CreatePoolInstance();

            if (prewarmCount > 0)
                Prewarm(prewarmCount);
        }
#endregion

        /// <summary>
        /// Gets an instance from the pool.
        /// </summary>
        /// <returns>The pooled instance.</returns>
        public virtual TAsset Get()
        {
            if (Pool != null)
                return Pool.Get();

            CustomLogger.LogError("Pool not initialized.", this);
            return null;
        }

        /// <summary>
        /// Releases an instance back to the pool.
        /// </summary>
        /// <param name="instance">The instance to release.</param>
        public virtual void Release(TAsset instance)
        {
            if (Pool == null)
            {
                CustomLogger.LogError("Pool not initialized.", this);
                return;
            }

            Pool.Release(instance);
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
        private static Transform GetTransform(Object obj) => obj switch
        {
            GameObject go => go.transform,
            Component comp => comp.transform,
            _ => null
        };

        private void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                TAsset instance = Pool.Get();
                Pool.Release(instance);
            }
        }
    }
}