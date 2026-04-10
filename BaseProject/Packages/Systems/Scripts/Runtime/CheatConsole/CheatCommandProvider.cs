using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Utility.Logging;

namespace Systems.CheatConsole
{
    /// <summary>
    /// Provides discovery of cheat commands from assemblies and scene objects.
    /// </summary>
    internal static class CheatCommandProvider
    {
        private static List<CheatCommandInfo> _cachedStaticCommands;

        /// <summary>
        /// Discovers all cheat commands available in the current context.
        /// This includes static methods marked with <see cref="CheatCommandAttribute"/> in the executing assembly,
        /// as well as instance methods on all active and inactive MonoBehaviours in the scene.
        /// </summary>
        /// <returns></returns>
        public static List<CheatCommandInfo> DiscoverAllCommands()
        {
            List<CheatCommandInfo> result = new();
            try
            {
                if (_cachedStaticCommands == null)
                {
                    Assembly executingAssembly = Assembly.GetExecutingAssembly();
                    _cachedStaticCommands = CheatCommandRegistry.CreateFromStaticMethods(new[] { executingAssembly });
                }

                result.AddRange(_cachedStaticCommands);

                MonoBehaviour[] behaviours = UnityEngine.Object.FindObjectsByType(typeof(MonoBehaviour),
                    FindObjectsInactive.Include, FindObjectsSortMode.None) as MonoBehaviour[];

                result.AddRange(CheatCommandRegistry.CreateFromTargets(behaviours));
            }
            catch (Exception ex)
            {
                CustomLogger.LogWarning($"Failed to discover cheat commands: {ex}", null);
            }

            return result;
        }
    }
}