#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using System.Reflection;
using Base.UtilityPackage.Logging;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.CorePackage.DebugMenu.CheatConsole
{
    /// <summary>
    /// Provides discovery of cheat commands from assemblies and scene objects.
    /// </summary>
    internal static class CheatCommandProvider
    {
        private static List<CheatCommandInfo> _cachedStaticCommands;

        /// <summary>
        /// Discovers all cheat commands available in the current context. This includes static methods marked
        /// with <see cref="CheatCommandAttribute"/> in the executing assembly, as well as instance methods on
        /// all active and inactive MonoBehaviours in the scene.
        /// </summary>
        /// <returns>Every cheat command that can currently be executed.</returns>
        public static List<CheatCommandInfo> DiscoverAllCommands()
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

                MonoBehaviour[] behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include,
                    FindObjectsSortMode.None);

                result.AddRange(CheatCommandRegistry.CreateFromTargets(behaviours));
            }
            catch (Exception exception)
            {
                CustomLogger.LogWarning($"Failed to discover cheat commands: {exception}", null);
            }

            return result;
        }

#if UNITY_EDITOR
        [InitializeOnEnterPlayMode]
        private static void ResetStatics() => _cachedStaticCommands = null;
#endif
    }
}