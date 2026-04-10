using System;
using System.Collections.Generic;
using Systems.Tweening.Components.System;
using UnityEngine;
using Utility.Logging;
using Object = UnityEngine.Object;

namespace Systems.ObjectPooling
{
    /// <summary>
    /// Specialized object pool for animated UI objects.
    /// Automatically caches <see cref="TweenGroup"/> components for efficient reuse
    /// and plays enter/exit animations on activation and deactivation.
    /// </summary>
    /// <typeparam name="T">The pooled Unity object type.</typeparam>
    public sealed class AnimatedUIObjectPool<T> : HashSetObjectPool<T> where T : Object
    {
        private readonly Dictionary<T, TweenGroup> _tweenCache;

        public AnimatedUIObjectPool(T prefab, Transform parent = null, Action<T> resetAction = null)
            : base(prefab, parent, resetAction)
        {
            _tweenCache = new Dictionary<T, TweenGroup>();
        }

        /// <summary>
        /// Creates a new instance from the prefab and caches its <see cref="TweenGroup"/> if present.
        /// </summary>
        protected override T CreateInstance()
        {
            T instance = Object.Instantiate(Prefab, Parent);

            if (instance == null)
            {
                CustomLogger.LogError("Failed to instantiate animated UI object.", null);
                return null;
            }

            TweenGroup tweenGroup = null;

            switch (instance)
            {
                case GameObject go:
                    go.TryGetComponent(out tweenGroup);
                    break;
                case Component comp:
                    comp.TryGetComponent(out tweenGroup);
                    break;
            }

            if (tweenGroup != null)
                _tweenCache[instance] = tweenGroup;

            return instance;
        }

        /// <summary>
        /// Activates an object when retrieved from the pool.
        /// If a cached <see cref="TweenGroup"/> exists, plays its animation forward.
        /// </summary>
        protected override void ActivateObject(T objectToEnable)
        {
            base.ActivateObject(objectToEnable);

            if (_tweenCache.TryGetValue(objectToEnable, out TweenGroup tweenGroup))
                tweenGroup.Play();
        }

        /// <summary>
        /// Deactivates an object when released to the pool.
        /// If a cached <see cref="TweenGroup"/> exists, plays its animation in reverse.
        /// </summary>
        protected override void DeactivateObject(T objectToDisable)
        {
            if (_tweenCache.TryGetValue(objectToDisable, out TweenGroup tweenGroup))
                tweenGroup.Reverse();

            base.DeactivateObject(objectToDisable);
        }
    }
}