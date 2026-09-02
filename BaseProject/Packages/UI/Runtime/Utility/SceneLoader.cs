using System;
using System.Threading.Tasks;
using Base.CorePackage.SceneManagement;
using Base.ServicesPackage;
using Base.UtilityPackage.Logging;
using Object = UnityEngine.Object;

namespace Base.UIPackage.Utility
{
    /// <summary>
    /// Shared scene loading entry point for the UI components of this package.
    /// Keeps the resolving, awaiting and error logging in one place.
    /// </summary>
    internal static class SceneLoader
    {
        /// <summary>
        /// Unloads all scenes and additively and asynchronously loads the given scene.
        /// </summary>
        /// <param name="sceneName">The name of the scene to load.</param>
        /// <param name="context">The object the log messages point to.</param>
        internal static async Task LoadSceneAsync(string sceneName, Object context)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                CustomLogger.LogError("No scene name given. Cannot load a scene.", context);
                return;
            }

            try
            {
                if (!ServiceLocator.TryGet(out SceneLoadingManager sceneLoadingManager))
                    return;

                await sceneLoadingManager.LoadSceneAsync(sceneName);
            }
            catch (Exception e)
            {
                CustomLogger.LogError($"Ran into an error {e}, while loading the scene {sceneName}.", context);
            }
        }
    }
}