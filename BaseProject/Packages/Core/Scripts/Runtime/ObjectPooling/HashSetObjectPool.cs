using System;
using System.Collections.Generic;
using Base.UtilityPackage.Logging;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Base.CorePackage.ObjectPooling
{
    /// <summary>
    /// A lightweight object pool based on <see cref="HashSet{T}"/>.
    /// Designed for constant-time take and release, even with many entries.
    /// Supports any <see cref="Object"/> type, so GameObjects as well as Components.
    /// </summary>
    /// <typeparam name="T">The pooled Unity object type.</typeparam>
    public class HashSetObjectPool<T> where T : Object
    {
        /// <summary>
        /// The objects that are currently available, so not in use.
        /// </summary>
        public IReadOnlyCollection<T> AvailableObjects => _availableObjects;

        /// <summary>
        /// The objects that are currently in use.
        /// </summary>
        public IReadOnlyCollection<T> ActiveObjects => _activeObjects;

        /// <summary>
        /// The number of available objects in the pool.
        /// </summary>
        public int AvailableCount => _availableObjects.Count;

        /// <summary>
        /// The number of objects currently in use.
        /// </summary>
        public int ActiveCount => _activeObjects.Count;

        /// <summary>
        /// The prefab new instances are created from.
        /// </summary>
        protected readonly T Prefab;

        private readonly Transform parent;
        private readonly HashSet<T> _availableObjects = new();
        private readonly HashSet<T> _activeObjects = new();
        private readonly Action<T> _resetAction;

        /// <summary>
        /// Creates a pool for the given prefab.
        /// </summary>
        /// <param name="prefab">The prefab new instances are created from.</param>
        /// <param name="parent">Optional parent for new instances.</param>
        /// <param name="resetAction">Optional action run on an object before it goes back into the pool.</param>
        public HashSetObjectPool(T prefab, Transform parent = null, Action<T> resetAction = null)
        {
            if (prefab == null)
                CustomLogger.LogError($"{nameof(prefab)} is null, this pool cannot create instances.", parent);

            Prefab = prefab;
            this.parent = parent;
            _resetAction = resetAction;
        }

        /// <summary>
        /// Tries to take an object from the pool. Creates a new instance when none is available.
        /// </summary>
        /// <param name="element">The taken object, or null when no instance could be created.</param>
        /// <returns><c>true</c> when an object was taken; otherwise, <c>false</c>.</returns>
        public bool TryGet(out T element)
        {
            element = TakeAvailable();

            if (element == null)
                element = CreateInstance();

            if (element == null)
            {
                CustomLogger.LogError($"Failed to create a new instance from {nameof(Prefab)}.", parent);
                return false;
            }

            ActivateObject(element);
            _activeObjects.Add(element);
            return true;
        }

        /// <summary>
        /// Takes an object from the pool. Creates a new instance when none is available.
        /// </summary>
        /// <returns>The taken object, or null when no instance could be created.</returns>
        public T Get() => TryGet(out T element)
            ? element
            : null;

        /// <summary>
        /// Releases an object back into the pool.
        /// </summary>
        /// <param name="element">The object to release.</param>
        public void Release(T element)
        {
            if (element == null)
            {
                CustomLogger.LogError("Tried to release a null element.", parent);
                return;
            }

            if (!_availableObjects.Add(element))
            {
                CustomLogger.LogError("Tried to release an element that is already in the pool.", element);
                return;
            }

            _activeObjects.Remove(element);
            _resetAction?.Invoke(element);
            DeactivateObject(element);
        }

        /// <summary>
        /// Releases multiple objects back into the pool.
        /// </summary>
        /// <param name="elements">The objects to release.</param>
        public void Release(IEnumerable<T> elements)
        {
            if (elements == null)
            {
                CustomLogger.LogError("Tried to release a null collection.", parent);
                return;
            }

            foreach (T element in elements)
                Release(element);
        }

        /// <summary>
        /// Releases multiple objects back into the pool.
        /// </summary>
        /// <param name="elements">The objects to release.</param>

        // The cast picks the enumerable overload, without it this call would recurse into itself.
        public void Release(params T[] elements) => Release((IEnumerable<T>)elements);

        /// <summary>
        /// Releases every object that is currently in use back into the pool.
        /// </summary>
        public void ReleaseAll()
        {
            // Copy first, releasing mutates the active set.
            T[] active = new T[_activeObjects.Count];
            _activeObjects.CopyTo(active);

            Release(active);
        }

        /// <summary>
        /// Checks whether the pool holds the given object, either in use or available.
        /// </summary>
        /// <param name="element">The object to look for.</param>
        /// <returns><c>true</c> when the pool holds the object; otherwise, <c>false</c>.</returns>
        public bool Contains(T element) => _activeObjects.Contains(element) || _availableObjects.Contains(element);

        /// <summary>
        /// Creates a new instance from the prefab.
        /// </summary>
        /// <returns>The new instance, or null when the prefab is missing.</returns>
        protected virtual T CreateInstance()
        {
            if (Prefab == null)
                return null;

            return Object.Instantiate(Prefab, parent);
        }

        /// <summary>
        /// Activates an object that was taken from the pool.
        /// </summary>
        /// <param name="objectToEnable">The object to activate.</param>
        protected virtual void ActivateObject(T objectToEnable) => SetActive(objectToEnable, true);

        /// <summary>
        /// Deactivates an object that was released into the pool.
        /// </summary>
        /// <param name="objectToDisable">The object to deactivate.</param>
        protected virtual void DeactivateObject(T objectToDisable) => SetActive(objectToDisable, false);

        private static void SetActive(T target, bool active)
        {
            switch (target)
            {
                case GameObject gameObject:
                    gameObject.SetActive(active);
                    break;
                case Component component:
                    component.gameObject.SetActive(active);
                    break;
            }
        }

        /// <summary>
        /// Takes any available instance out of the pool. Instances that were destroyed while sitting
        /// in the pool are dropped silently, that is a normal state after a scene unload.
        /// </summary>
        /// <returns>An available instance, or null when none is left.</returns>
        private T TakeAvailable()
        {
            while (_availableObjects.Count > 0)
            {
                T available = null;

                // The enumerator is disposed before the set is mutated, so this stays safe on all runtimes.
                using (HashSet<T>.Enumerator enumerator = _availableObjects.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                        available = enumerator.Current;
                }

                _availableObjects.Remove(available);

                if (available != null)
                    return available;
            }

            return null;
        }
    }
}