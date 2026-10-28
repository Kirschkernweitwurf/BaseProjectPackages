using System;
using System.Collections.Generic;
using System.Reflection;
using Base.UtilityPackage;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.CoreDebugPackage.DebugMenu.CheatConsole
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

        // Reading a type's methods and attributes is the expensive part, so each type is read once.
        private static readonly Dictionary<Type, MethodInfo[]> CommandMethodsByType = new();

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
                if (!UnityObjectUtility.IsAlive(target))
                    continue;

                AddCommands(GetCommandMethods(target.GetType()), target, result);
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

                foreach (Type type in ReflectionUtility.GetLoadableTypes(assembly))
                    AddCommands(type.GetMethods(StaticFlags), null, result);
            }

            return result;
        }

        /// <summary>
        /// Whether <paramref name="method"/> is marked with <see cref="CheatCommandAttribute"/>. Checks without
        /// creating the attribute, which is far cheaper than reading it.
        /// </summary>
        /// <param name="method">The method to check.</param>
        /// <returns>True when the method is a cheat command.</returns>
        internal static bool IsCommand(MethodInfo method) => method.IsDefined(typeof(CheatCommandAttribute), false);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => CommandMethodsByType.Clear();

        private static MethodInfo[] GetCommandMethods(Type type)
        {
            if (CommandMethodsByType.TryGetValue(type, out MethodInfo[] methods))
                return methods;

            methods = Array.FindAll(type.GetMethods(InstanceFlags), IsCommand);
            CommandMethodsByType.Add(type, methods);

            return methods;
        }

        private static void AddCommands(MethodInfo[] methods, object target, List<CheatCommandInfo> result)
        {
            foreach (MethodInfo method in methods)
            {
                if (!IsCommand(method))
                    continue;

                result.Add(new CheatCommandInfo(method.GetCustomAttribute<CheatCommandAttribute>(false), method,
                    target));
            }
        }
    }
}