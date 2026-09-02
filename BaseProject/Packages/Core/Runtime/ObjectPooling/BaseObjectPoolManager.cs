using Base.AttributesPackage;
using Base.UtilityPackage;
using Base.UtilityPackage.Pooling;
using UnityEngine;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Base.CorePackage.ObjectPooling
{
    /// <summary>
    /// Base class for global object pool managers.
    /// Provides lifecycle control and easy access to pooled Unity objects.
    /// </summary>
    /// <typeparam name="TAsset">The Unity object type to pool.</typeparam>
    /// <typeparam name="TPool">The concrete pool manager type.</typeparam>
    [DefaultExecutionOrder(-1)]
    public abstract class BaseObjectPoolManager<TAsset, TPool> : CustomSingleton<TPool>
        where TAsset : Object
        where TPool : BaseObjectPoolManager<TAsset, TPool>
    {
        [Title("Pooling Settings")]
        [Tooltip("Prefab to instantiate when new objects are needed.")]
        [Required] [SerializeField] protected TAsset prefab;

        [Tooltip("Optional parent where pooled objects will be instantiated.")]
        [SerializeField] protected Transform poolParent;

        [Tooltip("Number of instances to create on startup.")]
        [Min(0)] [SerializeField] private int prewarmCount;

        /// <summary>
        /// The pool holding the instances.
        /// </summary>
        public HashSetObjectPool<TAsset> Pool { get; private set; }

#region Unity Callbacks
        protected override void Awake()
        {
            base.Awake();

            Pool = CreatePoolInstance();
            Prewarm();
        }
#endregion

        /// <summary>
        /// Takes an instance from the pool.
        /// </summary>
        /// <returns>The pooled instance.</returns>
        public virtual TAsset Get() => Pool.Get();

        /// <summary>
        /// Releases an instance back into the pool.
        /// </summary>
        /// <param name="instance">The instance to release.</param>
        public virtual void Release(TAsset instance) => Pool.Release(instance);

        /// <summary>
        /// Creates the pool instance. Override to customize pool behavior.
        /// </summary>
        /// <returns>The created pool.</returns>
        protected virtual HashSetObjectPool<TAsset> CreatePoolInstance() => new(prefab, poolParent, ResetInstance);

        /// <summary>
        /// Resets an instance before it goes back into the pool.
        /// </summary>
        /// <param name="instance">The instance to reset.</param>
        protected virtual void ResetInstance(TAsset instance)
        {
            Transform instanceTransform = GetTransform(instance);

            if (instanceTransform == null)
                return;

            instanceTransform.SetParent(poolParent, false);
        }

        private static Transform GetTransform(Object target) => target switch
        {
            GameObject gameObject => gameObject.transform,
            Component component => component.transform,
            _ => null
        };

        /// <summary>
        /// Prewarms the pool. All instances are taken first and released afterward, because releasing
        /// each one right away would just hand the same single instance back on the next take.
        /// </summary>
        private void Prewarm()
        {
            if (prewarmCount <= 0)
                return;

            TAsset[] instances = new TAsset[prewarmCount];

            for (int i = 0; i < prewarmCount; i++)
                instances[i] = Pool.Get();

            Pool.Release(instances);
        }
    }
}