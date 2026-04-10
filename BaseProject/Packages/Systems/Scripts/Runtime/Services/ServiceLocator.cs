using System;
using System.Collections.Generic;
using Utility.Logging;

namespace Systems.Services
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
        /// <param name="type">The type of the service being registered.</param>
        /// <param name="service">The service instance to register.</param>
        public static void Register(Type type, IGameService service)
        {
            if (Services.ContainsKey(type))
            {
                CustomLogger.LogWarning($"Service {type.Name} is already registered. " +
                                        "Overwriting with new instance.", service as UnityEngine.Object);
            }

            Services[type] = service;
        }

        /// <summary>
        /// Removes a service from the locator.
        /// </summary>
        /// <param name="type">The type of the service to remove.</param>
        public static void Deregister(Type type)
        {
            if (Services.Remove(type))
                return;

            CustomLogger.LogWarning($"Service {type.Name} is not registered. Cannot deregister.", null);
        }

        /// <summary>
        /// Attempts to retrieve a service of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the service to retrieve.</typeparam>
        /// <param name="service">The retrieved service, or null if not found.</param>
        /// <returns><c>true</c> if the service was found and is not null; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// Handles null checks, cleanup, and logging if necessary.
        /// </remarks>
        public static bool TryGet<T>(out T service) where T : class, IGameService
        {
            if (Services.TryGetValue(typeof(T), out IGameService rawService))
            {
                if (rawService != null)
                {
                    service = rawService as T;
                    return service != null;
                }

                // Cleanup if the entry exists but is null
                CustomLogger.LogError($"Service {typeof(T).Name} is null, removing from locator.", null);
                Services.Remove(typeof(T));
            }

            CustomLogger.LogError($"Service {typeof(T).Name} not found!", null);
            service = null;
            return false;
        }

        /// <summary>
        /// Retrieves a service of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the service to retrieve.</typeparam>
        /// <returns>
        /// The requested service instance, or null if not found.
        /// </returns>
        /// <remarks>
        /// Logs an error if the service is missing or null.
        /// </remarks>
        public static T Get<T>() where T : class, IGameService => TryGet(out T service) ? service : null;

#if UNITY_EDITOR
        /// <summary>
        /// Validates that all registered services have been properly deregistered.
        /// </summary>
        public static void ValidateServices()
        {
            foreach (KeyValuePair<Type, IGameService> kvp in Services)
                CustomLogger.LogError($"Service {kvp.Key.Name} was not deregistered properly.", null);
        }
#endif
    }
}