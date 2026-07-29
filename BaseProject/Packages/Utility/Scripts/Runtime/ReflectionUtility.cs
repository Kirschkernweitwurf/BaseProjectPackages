using System;
using System.Collections.Generic;
using System.Reflection;
using Base.UtilityPackage.Logging;

namespace Base.UtilityPackage
{
    /// <summary>
    /// Helpers for reflecting over the assemblies loaded in the current application domain.
    /// </summary>
    public static class ReflectionUtility
    {
        /// <summary>
        /// Returns every type of an assembly that can actually be loaded.
        /// </summary>
        /// <param name="assembly">The assembly to read the types from.</param>
        /// <returns>The loadable types. Never null and never containing null entries.</returns>
        /// <remarks>
        /// <see cref="Assembly.GetTypes"/> throws as soon as a single type fails to load, for example when an
        /// optional dependency is missing. The exception still carries every type that did load, mixed with null
        /// entries for the ones that did not. Those nulls are filtered out here, so callers can iterate the
        /// result directly instead of repeating the same null check.
        /// </remarks>
        public static IReadOnlyList<Type> GetLoadableTypes(Assembly assembly)
        {
            if (assembly == null)
            {
                CustomLogger.LogWarning($"{nameof(GetLoadableTypes)} was called without an {nameof(assembly)}.",
                    null);

                return Array.Empty<Type>();
            }

            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                List<Type> loadable = new(exception.Types.Length);

                foreach (Type type in exception.Types)
                {
                    if (type != null)
                        loadable.Add(type);
                }

                return loadable;
            }
        }
    }
}