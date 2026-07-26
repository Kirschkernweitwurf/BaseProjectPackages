#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using Base.UtilityPackage;
using Base.UtilityPackage.Logging;
using Object = UnityEngine.Object;

namespace Base.CorePackage.Services
{
    /// <summary>
    /// A simple service locator for managing and accessing game services.
    /// Services must implement <see cref="IGameService"/> and register/deregister themselves.
    /// Works for both MonoBehaviour-based and non-MonoBehaviour services.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, IGameService> Services = new();

        /// <summary>
        /// Adds or updates a service in the locator.
        /// </summary>
        /// <param name="type">The type the service is registered under.</param>
        /// <param name="service">The service instance to register.</param>
        public static void Register(Type type, IGameService service)
        {
            if (type == null)
            {
                CustomLogger.LogError("Cannot register a service without a type.", service as Object);
                return;
            }

            if (!UnityObjectUtility.IsAlive(service))
            {
                CustomLogger.LogError($"Cannot register a null service for {type.Name}.", null);
                return;
            }

            // Only a live duplicate is a real conflict. A destroyed leftover is a normal scene reload.
            if (Services.TryGetValue(type, out IGameService existing)
                && UnityObjectUtility.IsAlive(existing))
                CustomLogger.LogWarning($"Service {type.Name} is already registered. Overwriting the old instance.",
                    service as Object);

            Services[type] = service;
        }

        /// <summary>
        /// Adds or updates a service in the locator using a generic type parameter.
        /// </summary>
        /// <param name="service">The service instance to register.</param>
        /// <typeparam name="T">The type the service is registered under.</typeparam>
        public static void Register<T>(T service) where T : class, IGameService => Register(typeof(T), service);

        /// <summary>
        /// Removes a service from the locator using a generic type parameter.
        /// </summary>
        /// <param name="service">
        /// The instance that is deregistering. When passed, the entry is only removed if it still holds that
        /// instance, so a destroyed service cannot wipe the replacement that already registered itself.
        /// </param>
        /// <typeparam name="T">The type the service is registered under.</typeparam>
        public static void Deregister<T>(T service = null) where T : class, IGameService
            => Deregister(typeof(T), service);

        /// <summary>
        /// Removes a service from the locator.
        /// </summary>
        /// <param name="type">The type the service is registered under.</param>
        /// <param name="service">
        /// The instance that is deregistering. When passed, the entry is only removed if it still holds that
        /// instance, so a destroyed service cannot wipe the replacement that already registered itself.
        /// </param>
        public static void Deregister(Type type, IGameService service = null)
        {
            if (type == null)
            {
                CustomLogger.LogError("Cannot deregister a service without a type.", service as Object);
                return;
            }

            if (!Services.TryGetValue(type, out IGameService registered))
            {
                CustomLogger.LogWarning($"Service {type.Name} is not registered. Cannot deregister.", null);
                return;
            }

            // A newer instance already took over, so this call comes from an outdated one.
            if (!ReferenceEquals(service, null)
                && !ReferenceEquals(registered, service))
                return;

            Services.Remove(type);
        }

        /// <summary>
        /// Attempts to retrieve a service of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the service to retrieve.</typeparam>
        /// <param name="service">The retrieved service, or null if not found.</param>
        /// <returns><c>true</c> if the service was found and is alive; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// Handles null checks, cleanup, and logging on its own, so callers do not need to log again.
        /// </remarks>
        public static bool TryGet<T>(out T service) where T : class, IGameService
        {
            Type type = typeof(T);
            service = null;

            if (!Services.TryGetValue(type, out IGameService registered))
            {
                CustomLogger.LogError($"Service {type.Name} is not registered.", null);
                return false;
            }

            if (!UnityObjectUtility.IsAlive(registered))
            {
                Services.Remove(type);
                CustomLogger.LogError($"Service {type.Name} was destroyed without deregistering. Entry removed.",
                    null);

                return false;
            }

            service = registered as T;
            if (service != null)
                return true;

            CustomLogger.LogError($"Service {type.Name} is registered with an incompatible instance.",
                registered as Object);

            return false;
        }

        /// <summary>
        /// Retrieves a service of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the service to retrieve.</typeparam>
        /// <returns>The requested service instance, or null if not found.</returns>
        /// <remarks>
        /// Logs an error if the service is missing or was destroyed.
        /// </remarks>
        public static T Get<T>() where T : class, IGameService => TryGet(out T service)
            ? service
            : null;

#if UNITY_EDITOR
        [InitializeOnEnterPlayMode]
        private static void ResetStatics() => Services.Clear();
#endif
    }
}