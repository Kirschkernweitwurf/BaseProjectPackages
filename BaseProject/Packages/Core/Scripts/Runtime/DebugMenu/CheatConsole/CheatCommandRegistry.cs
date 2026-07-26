using System;
using System.Collections.Generic;
using System.Reflection;
using Base.UtilityPackage.Logging;
using Object = UnityEngine.Object;

namespace Base.CorePackage.DebugMenu.CheatConsole
{
    /// <summary>
    /// Utility class to discover and create <see cref="CheatCommandInfo"/> instances
    /// from attributed methods.
    /// </summary>
    public static class CheatCommandRegistry
    {
        private const BindingFlags InstanceFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private const BindingFlags StaticFlags =
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// Creates cheat command infos for all instance methods on the provided targets
        /// that are marked with <see cref="CheatCommandAttribute"/>.
        /// </summary>
        /// <param name="targets">The objects whose methods should be scanned.</param>
        /// <returns>A list of discovered cheat command definitions.</returns>
        public static List<CheatCommandInfo> CreateFromTargets(IEnumerable<object> targets)
        {
            List<CheatCommandInfo> result = new();

            if (targets == null)
            {
                CustomLogger.LogError("Cheat command targets cannot be null.", null);
                return result;
            }

            foreach (object target in targets)
            {
                if (IsNull(target))
                    continue;

                AddCommands(target.GetType().GetMethods(InstanceFlags), target, result);
            }

            return result;
        }

        /// <summary>
        /// Creates cheat command infos for all static methods in the given assemblies
        /// that are marked with <see cref="CheatCommandAttribute"/>.
        /// </summary>
        /// <param name="assemblies">Assemblies to scan for static cheat command methods.</param>
        /// <returns>A list of discovered cheat command definitions.</returns>
        public static List<CheatCommandInfo> CreateFromStaticMethods(IEnumerable<Assembly> assemblies)
        {
            List<CheatCommandInfo> result = new();

            if (assemblies == null)
            {
                CustomLogger.LogError("Cheat command assemblies cannot be null.", null);
                return result;
            }

            foreach (Assembly assembly in assemblies)
            {
                if (assembly == null)
                    continue;

                foreach (Type type in GetTypes(assembly))
                {
                    if (type == null)
                        continue;

                    AddCommands(type.GetMethods(StaticFlags), null, result);
                }
            }

            return result;
        }

        private static Type[] GetTypes(Assembly assembly)
        {
            // A partially loadable assembly still carries usable types, so the loaded ones are kept.
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                return exception.Types;
            }
        }

        private static void AddCommands(MethodInfo[] methods, object target, List<CheatCommandInfo> result)
        {
            foreach (MethodInfo method in methods)
            {
                CheatCommandAttribute attribute = method.GetCustomAttribute<CheatCommandAttribute>(false);
                if (attribute == null)
                    continue;

                result.Add(new CheatCommandInfo(attribute, method, target));
            }
        }

        // Destroyed Unity objects are only recognized by Unity's own equality operator.
        private static bool IsNull(object target)
        {
            if (target is Object unityObject)
                return unityObject == null;

            return target == null;
        }
    }
}