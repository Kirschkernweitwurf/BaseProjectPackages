#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using System.Reflection;
using Base.UtilityPackage;
using Base.UtilityPackage.Logging;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.CoreDebugPackage.DebugMenu.CheatConsole
{
    /// <summary>
    /// Provides discovery of cheat commands from assemblies and scene objects.
    /// </summary>
    /// <remarks>
    /// Which types have commands is found once, so a discovery only asks the scene for objects of those types.
    /// Reading the methods of every live behaviour instead took over a second per scene load.
    /// </remarks>
    internal static class CheatCommandProvider
    {
#if !UNITY_EDITOR
        private const BindingFlags DeclaredInstanceFlags = BindingFlags.Instance | BindingFlags.Public
            | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
#endif

        private static List<CheatCommandInfo> _cachedStaticCommands;
        private static List<Type> _targetTypes;

        /// <summary>
        /// Discovers all cheat commands available in the current context. This includes static methods marked
        /// with <see cref="CheatCommandAttribute"/> in the executing assembly, as well as instance methods on
        /// all active and inactive MonoBehaviours in the scene.
        /// </summary>
        /// <returns>Every cheat command that can currently be executed.</returns>
        internal static List<CheatCommandInfo> DiscoverAllCommands()
        {
            List<CheatCommandInfo> result = new();

            try
            {
                // Static commands never change while the domain is loaded, so they are only scanned once.
                _cachedStaticCommands ??= CheatCommandRegistry.CreateFromStaticMethods(new[]
                {
                    Assembly.GetExecutingAssembly()
                });

                result.AddRange(_cachedStaticCommands);

                _targetTypes ??= FindTargetTypes();
                result.AddRange(CheatCommandRegistry.CreateFromTargets(FindTargets()));
            }
            catch (Exception exception)
            {
                CustomLogger.LogWarning($"Failed to discover cheat commands: {exception}", null);
            }

            return result;
        }

#if UNITY_EDITOR
        [InitializeOnEnterPlayMode]
        private static void ResetStatics()
        {
            _cachedStaticCommands = null;
            _targetTypes = null;
        }
#endif

        // Only behaviours can be found in the scene, so other types that declare instance commands are left out.
        private static List<Type> FindTargetTypes()
        {
            List<Type> types = new();

            foreach (MethodInfo method in FindInstanceCommandMethods())
            {
                Type type = method.DeclaringType;
                if (type == null
                    || type.ContainsGenericParameters
                    || !typeof(MonoBehaviour).IsAssignableFrom(type)
                    || types.Contains(type))
                    continue;

                types.Add(type);
            }

            return types;
        }

#if UNITY_EDITOR
        // The editor keeps an index of attributed methods, so this needs no reflection at all.
        private static IEnumerable<MethodInfo> FindInstanceCommandMethods()
        {
            foreach (MethodInfo method in TypeCache.GetMethodsWithAttribute<CheatCommandAttribute>())
            {
                if (!method.IsStatic)
                    yield return method;
            }
        }
#else
        // Only an assembly that references this one can use the attribute, so the engine and system assemblies,
        // which hold most of the types, are skipped.
        private static IEnumerable<MethodInfo> FindInstanceCommandMethods()
        {
            Assembly own = typeof(CheatCommandAttribute).Assembly;
            string ownName = own.GetName().Name;

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly != own
                    && !IsReferencing(assembly, ownName))
                    continue;

                foreach (Type type in ReflectionUtility.GetLoadableTypes(assembly))
                {
                    foreach (MethodInfo method in type.GetMethods(DeclaredInstanceFlags))
                    {
                        if (CheatCommandRegistry.IsCommand(method))
                            yield return method;
                    }
                }
            }
        }

        private static bool IsReferencing(Assembly assembly, string referenceName)
        {
            foreach (AssemblyName reference in assembly.GetReferencedAssemblies())
            {
                if (reference.Name == referenceName)
                    return true;
            }

            return false;
        }
#endif

        // A behaviour whose base class also has commands is found through both types, so the set keeps it once.
        private static HashSet<Object> FindTargets()
        {
            HashSet<Object> targets = new();

            foreach (Type type in _targetTypes)
                targets.UnionWith(Object.FindObjectsByType(type, FindObjectsInactive.Include,
                    FindObjectsSortMode.None));

            return targets;
        }
    }
}