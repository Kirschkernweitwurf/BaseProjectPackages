using System;
using System.Collections.Generic;
using System.Reflection;
using Utility.Logging;

namespace Systems.CheatConsole
{
    /// <summary>
    /// Utility class to discover and create <see cref="CheatCommandInfo"/> instances
    /// from attributed methods.
    /// </summary>
    public static class CheatCommandRegistry
    {
        /// <summary>
        /// Creates cheat command infos for all public instance methods on the provided targets
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
                if (target == null)
                    continue;

                Type type = target.GetType();
                MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public
                                                                             | BindingFlags.NonPublic);

                foreach (MethodInfo method in methods)
                {
                    object[] attributes = method.GetCustomAttributes(typeof(CheatCommandAttribute), false);
                    if (attributes.Length == 0)
                        continue;

                    if (attributes[0] is not CheatCommandAttribute attribute)
                        continue;

                    CheatCommandInfo info = new(attribute, method, target);
                    result.Add(info);
                }
            }

            return result;
        }

        /// <summary>
        /// Creates cheat command infos for all public static methods in the given assemblies
        /// that are marked with <see cref="CheatCommandAttribute"/>.
        /// </summary>
        /// <param name="assemblies">Assemblies to scan for static cheat command methods.</param>
        /// <returns>A list of discovered cheat command definitions.</returns>
        public static List<CheatCommandInfo> CreateFromStaticMethods(IEnumerable<Assembly> assemblies)
        {
            if (assemblies == null)
            {
                CustomLogger.LogError("Cheat command assemblies cannot be null.", null);
                return new List<CheatCommandInfo>();
            }

            List<CheatCommandInfo> result = new();

            foreach (Assembly assembly in assemblies)
            {
                if (assembly == null)
                    continue;

                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException exception)
                {
                    types = exception.Types;
                }

                foreach (Type type in types)
                {
                    if (type == null)
                        continue;

                    MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                    foreach (MethodInfo method in methods)
                    {
                        object[] attributes = method.GetCustomAttributes(typeof(CheatCommandAttribute), false);
                        if (attributes.Length == 0)
                            continue;

                        if (attributes[0] is not CheatCommandAttribute attribute)
                            continue;

                        CheatCommandInfo info = new(attribute, method, null);
                        result.Add(info);
                    }
                }
            }

            return result;
        }
    }
}