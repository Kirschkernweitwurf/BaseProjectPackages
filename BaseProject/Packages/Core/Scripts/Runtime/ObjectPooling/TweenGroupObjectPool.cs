using System;
using System.Collections.Generic;
using Base.CorePackage.Tweening.Components.System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.CorePackage.ObjectPooling
{
    /// <summary>
    /// Object pool for animated UI objects. Caches the <see cref="TweenGroup"/> of every instance
    /// and plays its enter and exit animation on activation and deactivation.
    /// </summary>
    /// <typeparam name="T">The pooled Unity object type.</typeparam>
    public sealed class TweenGroupObjectPool<T> : HashSetObjectPool<T> where T : Object
    {
        private readonly Dictionary<T, TweenGroup> _tweenCache = new();

        /// <summary>
        /// Creates a pool for the given prefab.
        /// </summary>
        /// <param name="prefab">The prefab new instances are created from.</param>
        /// <param name="parent">Optional parent for new instances.</param>
        /// <param name="resetAction">Optional action run on an object before it goes back into the pool.</param>
        public TweenGroupObjectPool(T prefab, Transform parent = null, Action<T> resetAction = null)
            : base(prefab, parent, resetAction) { }

        /// <summary>
        /// Creates a new instance and caches its <see cref="TweenGroup"/> when it has one.
        /// </summary>
        /// <returns>The new instance, or null when the prefab is missing.</returns>
        protected override T CreateInstance()
        {
            T instance = base.CreateInstance();

            if (instance == null)
                return null;

            if (TryFindTweenGroup(instance, out TweenGroup tweenGroup))
                _tweenCache[instance] = tweenGroup;

            return instance;
        }

        /// <summary>
        /// Activates an object and plays its cached animation forward.
        /// </summary>
        /// <param name="objectToEnable">The object to activate.</param>
        protected override void ActivateObject(T objectToEnable)
        {
            base.ActivateObject(objectToEnable);

            if (TryGetCachedTweenGroup(objectToEnable, out TweenGroup tweenGroup))
                tweenGroup.Show();
        }

        /// <summary>
        /// Plays the cached animation in reverse and deactivates the object.
        /// </summary>
        /// <param name="objectToDisable">The object to deactivate.</param>
        protected override void DeactivateObject(T objectToDisable)
        {
            if (TryGetCachedTweenGroup(objectToDisable, out TweenGroup tweenGroup))
                tweenGroup.Hide();

            base.DeactivateObject(objectToDisable);
        }

        private static bool TryFindTweenGroup(T instance, out TweenGroup tweenGroup)
        {
            tweenGroup = null;

            switch (instance)
            {
                case GameObject gameObject:
                    gameObject.TryGetComponent(out tweenGroup);
                    break;
                case Component component:
                    component.TryGetComponent(out tweenGroup);
                    break;
            }

            return tweenGroup != null;
        }

        /// <summary>
        /// Looks up the cached group. Stays quiet when the instance was destroyed, that is a normal
        /// state for pooled objects after a scene unload.
        /// </summary>
        private bool TryGetCachedTweenGroup(T instance, out TweenGroup tweenGroup)
            => _tweenCache.TryGetValue(instance, out tweenGroup)
                && tweenGroup != null;
    }
}