using System;
using System.Collections.Generic;
using Base.UtilityPackage.Logging;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.CorePackage.ObjectPooling
{
    /// <summary>
    /// A lightweight, high-performance object pool based on <see cref="HashSet{T}"/>.
    /// Designed for constant-time allocation and release even with many entries.
    /// Supports any <see cref="UnityEngine.Object"/> type (GameObject or Component).
    /// </summary>
    /// <typeparam name="T">The Unity object type to pool (must derive from <see cref="UnityEngine.Object"/>).</typeparam>
    public class HashSetObjectPool<T> where T : Object
    {
        /// <summary>
        /// The number of available objects in the pool.
        /// </summary>
        public int AvailableCount => AvailableObjects.Count;

        /// <summary>
        /// The number of active (in-use) objects from the pool.
        /// </summary>
        public int ActiveCount => ActiveObjects.Count;

        /// <summary>
        /// The set of currently available (not in use) objects in the pool.
        /// </summary>
        public HashSet<T> AvailableObjects { get; }

        /// <summary>
        /// The set of currently active (in use) objects from the pool.
        /// </summary>
        public HashSet<T> ActiveObjects { get; }

        /// <summary>
        /// The prefab used to instantiate new objects.
        /// </summary>
        protected readonly T Prefab;

        /// <summary>
        /// Optional parent transform for instantiated objects.
        /// </summary>
        protected readonly Transform Parent;

        private readonly Action<T> _resetAction;

        public HashSetObjectPool(T prefab, Transform parent = null, Action<T> resetAction = null)
        {
            if (prefab == null)
            {
                CustomLogger.LogError("Prefab is null. Cannot create object pool.", null);
                return;
            }

            Prefab = prefab;
            Parent = parent;

            _resetAction = resetAction;

            AvailableObjects = new HashSet<T>();
            ActiveObjects = new HashSet<T>();
        }

        /// <summary>
        /// Attempts to retrieve an object from the pool.
        /// </summary>
        /// <param name="element">The retrieved object, or null if instantiation failed.</param>
        /// <returns><c>true</c> if an object was successfully retrieved; otherwise, <c>false</c>.</returns>
        public bool TryGet(out T element)
        {
            element = null;

            // Take any available instance. The enumerator is disposed before the
            // set is mutated, so this stays safe on all runtimes.
            T available = null;
            using (HashSet<T>.Enumerator enumerator = AvailableObjects.GetEnumerator())
            {
                if (enumerator.MoveNext())
                    available = enumerator.Current;
            }

            if (available != null)
            {
                AvailableObjects.Remove(available);
                ActivateObject(available);
                ActiveObjects.Add(available);
                element = available;
                return true;
            }

            T newInstance = CreateInstance();
            if (newInstance == null)
            {
                CustomLogger.LogError("Failed to instantiate new object from prefab.", null);
                return false;
            }

            ActivateObject(newInstance);
            element = newInstance;
            ActiveObjects.Add(newInstance);
            return true;
        }

        /// <summary>
        /// Retrieves an object from the pool. If none are available,
        /// a new instance is created by instantiating the prefab.
        /// </summary>
        /// <returns>The retrieved object.</returns>
        public T Get() => TryGet(out T element)
            ? element
            : null;

        /// <summary>
        /// Releases an object back to the pool. The object is reset using the provided reset action, if any.
        /// </summary>
        /// <param name="element">The object to release back to the pool.</param>
        public void Release(T element)
        {
            if (element == null)
            {
                CustomLogger.LogError("Attempted to release null element.", null);
                return;
            }

            if (!AvailableObjects.Add(element))
            {
                CustomLogger.LogError("Attempted to release an element that is already in the pool.", element);
                return;
            }

            _resetAction?.Invoke(element);
            ActiveObjects.Remove(element);
            DeactivateObject(element);
        }

        /// <summary>
        /// Releases multiple objects back to the pool.
        /// </summary>
        public void Release(List<T> elements)
        {
            foreach (T element in elements)
                Release(element);
        }

        /// <summary>
        /// Releases multiple objects back to the pool.
        /// </summary>
        public void Release(IEnumerable<T> elements)
        {
            foreach (T element in elements)
                Release(element);
        }

        /// <summary>
        /// Releases multiple objects back to the pool.
        /// </summary>
        public void Release(params T[] elements)
        {
            foreach (T element in elements)
                Release(element);
        }

        /// <summary>
        /// Releases all currently in-use objects back to the pool.
        /// </summary>
        public void ReleaseAll()
        {
            T[] inUseArray = new T[ActiveObjects.Count];
            ActiveObjects.CopyTo(inUseArray);

            foreach (T element in inUseArray)
                Release(element);
        }

        /// <summary>
        /// Checks if the pool contains the specified element (either in use or available).
        /// </summary>
        /// <param name="element">The element to check for.</param>
        /// <returns><c>true</c> if the pool contains the element; otherwise, <c>false</c>.</returns>
        public bool Contains(T element) => ActiveObjects.Contains(element) || AvailableObjects.Contains(element);

        protected virtual T CreateInstance()
        {
            T newInstance = Object.Instantiate(Prefab, Parent);
            return newInstance;
        }

        /// <summary>
        /// Activates an object when retrieved from the pool.
        /// </summary>
        protected virtual void ActivateObject(T objectToEnable)
        {
            switch (objectToEnable)
            {
                case GameObject go:
                    go.SetActive(true);
                    break;
                case Component comp:
                    comp.gameObject.SetActive(true);
                    break;
            }
        }

        /// <summary>
        /// Deactivates an object when released to the pool.
        /// </summary>
        protected virtual void DeactivateObject(T objectToDisable)
        {
            switch (objectToDisable)
            {
                case GameObject go:
                    go.SetActive(false);
                    break;
                case Component comp:
                    comp.gameObject.SetActive(false);
                    break;
            }
        }
    }
}