using System;
using System.Collections.Generic;
using System.Threading;
using Base.AttributePackage;
using Base.ServicePackage;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Base.CorePackage.SceneManagement
{
    /// <summary>
    /// Manages scene loading and unloading operations, including a persistent scene that remains loaded.
    /// Provides asynchronous methods to load scenes with progress reporting via <see cref="SceneLoadEvents"/>.
    /// Uses Unity's Awaitable for play-mode-safe, allocation-free async operations.
    /// </summary>
    public class SceneLoadingManager : GameServiceBehaviour
    {
        /// <summary>
        /// The maximum progress value to report before allowing scene activation.
        /// Unity reserves the last 0.1 of the range for activation itself.
        /// </summary>
        private const float ProgressReportMax = 0.9f;

        [SceneName] [NotNullOrEmpty] [SerializeField] private string persistentSceneName;

        private bool _persistentLoaded;
        private bool _isLoading;

#region Unity Callbacks
        private async void Start()
        {
            try
            {
                await LoadPersistentSceneAsync();
            }
            catch (OperationCanceledException)
            {
                // Expected when exiting play mode or when this manager is destroyed. Ignore.
            }
            catch (Exception e)
            {
                CustomLogger.LogError($"Error loading persistent scene: {e}", this);
            }
        }
#endregion

        /// <summary>
        /// Unloads all currently loaded scenes (except the persistent scene) and loads the specified scene.
        /// This method is asynchronous and will yield until the new scene is fully loaded.
        /// </summary>
        /// <param name="sceneName">The name of the scene to load.</param>
        /// <param name="mode">The load scene mode (Single or Additive). Default is Additive.</param>
        public async Awaitable LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Additive)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                CustomLogger.LogError("Cannot load a scene without a name.", this);
                return;
            }

            if (_isLoading)
            {
                CustomLogger.LogWarning($"Tried to load scene '{sceneName}' while another load is running.", this);
                return;
            }

            if (SceneManager.GetSceneByName(sceneName).isLoaded)
            {
                CustomLogger.LogWarning($"Tried to load scene '{sceneName}', but it was already loaded.", this);
                return;
            }

            _isLoading = true;

            try
            {
                await UnloadAllScenesAsync();
                await LoadSceneInternalAsync(sceneName, mode, destroyCancellationToken);
            }
            finally
            {
                _isLoading = false;
            }
        }

        /// <summary>
        /// Runs the actual load and reports progress through <see cref="SceneLoadEvents"/>.
        /// </summary>
        /// <param name="sceneName">The name of the scene to load.</param>
        /// <param name="mode">The load scene mode (Single or Additive).</param>
        /// <param name="token">Cancellation token tied to the owner's lifetime, so the load aborts on destroy.</param>
        private static async Awaitable LoadSceneInternalAsync(string sceneName, LoadSceneMode mode,
            CancellationToken token)
        {
            SceneLoadEvents.InvokeSceneLoadStarted(sceneName);

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, mode);
            if (operation == null)
            {
                SceneLoadEvents.InvokeSceneLoadCompleted(sceneName, false);
                return;
            }

            // Hold activation back so progress can be reported up to the reserved mark.
            operation.allowSceneActivation = false;

            while (operation.progress < ProgressReportMax)
            {
                SceneLoadEvents.InvokeSceneLoadProgress(sceneName, operation.progress);
                await Awaitable.NextFrameAsync(token);
            }

            operation.allowSceneActivation = true;

            while (!operation.isDone)
            {
                SceneLoadEvents.InvokeSceneLoadProgress(sceneName, operation.progress);
                await Awaitable.NextFrameAsync(token);
            }

            SceneLoadEvents.InvokeSceneLoadCompleted(sceneName, true);
        }

        /// <summary>
        /// Ensures the persistent scene is loaded. If it is already loaded, this method does nothing.
        /// </summary>
        private async Awaitable LoadPersistentSceneAsync()
        {
            if (_persistentLoaded)
                return;

            if (!SceneManager.GetSceneByName(persistentSceneName).isLoaded)
                await LoadSceneInternalAsync(persistentSceneName, LoadSceneMode.Additive, destroyCancellationToken);

            _persistentLoaded = true;
        }

        /// <summary>
        /// Unloads all currently loaded scenes except for the persistent scene.
        /// This method is asynchronous and will yield until all scenes are unloaded.
        /// </summary>
        private async Awaitable UnloadAllScenesAsync()
        {
            // Collect first, unloading while iterating would shift the scene indices.
            List<string> scenesToUnload = new();

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name != persistentSceneName)
                    scenesToUnload.Add(scene.name);
            }

            foreach (string sceneName in scenesToUnload)
            {
                // Unity logs its own error when a scene cannot be unloaded, so stay quiet here.
                AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(sceneName);
                if (unloadOperation == null)
                    continue;

                while (!unloadOperation.isDone)
                    await Awaitable.NextFrameAsync(destroyCancellationToken);
            }
        }
    }
}