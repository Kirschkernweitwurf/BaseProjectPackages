using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Attributes;
using Systems.Services;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utility.Logging;

namespace SceneManagement
{
    /// <summary>
    /// Manages scene loading and unloading operations, including a persistent scene that remains loaded.
    /// Provides asynchronous methods to load scenes with progress reporting via events.
    /// </summary>
    public class SceneLoadingManager : GameServiceBehaviour
    {
        /// <summary>
        /// The maximum progress value to report before allowing scene activation.
        /// This is typically 0.9f, as Unity reserves the last 0.1f for activation.
        /// </summary>
        private const float ProgressReportMax = 0.9f;

        [SceneName] [SerializeField] private string persistentSceneName;

        private bool _persistentLoaded;

        private async void Start()
        {
            try
            {
                await LoadPersistentScene();
            }
            catch (Exception e)
            {
                CustomLogger.LogError($"Error loading persistent scene: {e}", this);
            }
        }

        /// <summary>
        /// Unloads all currently loaded scenes (except the persistent scene) and loads the specified scene.
        /// This method is asynchronous and will yield until the new scene is fully loaded.
        /// </summary>
        /// <param name="sceneName">The name of the scene to load.</param>
        /// <param name="mode">The load scene mode (Single or Additive). Default is Single.</param>
        public async Task LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Additive)
        {
            await UnloadAllScenesAsync();
            await LoadSceneInternalAsync(sceneName, mode);
        }

        /// <summary>
        /// Ensures the persistent scene is loaded. If it's already loaded, this method does nothing.
        /// </summary>
        private async Task LoadPersistentScene()
        {
            if (_persistentLoaded)
                return;

            if (!SceneManager.GetSceneByName(persistentSceneName).isLoaded)
                await LoadSceneInternalAsync(persistentSceneName, LoadSceneMode.Additive);

            _persistentLoaded = true;
        }

        /// <summary>
        /// Loads a scene asynchronously and reports progress through events.
        /// </summary>
        /// <param name="sceneName">The name of the scene to load.</param>
        /// <param name="mode">The load scene mode (Single or Additive).</param>
        private static async Task LoadSceneInternalAsync(string sceneName, LoadSceneMode mode)
        {
            bool success = false;

            SceneLoadEvents.InvokeSceneLoadStarted(sceneName);

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, mode);
            if (operation != null)
            {
                operation.allowSceneActivation = false;

                while (operation.progress < ProgressReportMax)
                {
                    SceneLoadEvents.InvokeSceneLoadProgress(sceneName, operation.progress);
                    await Task.Yield();
                }

                operation.allowSceneActivation = true;

                while (!operation.isDone)
                {
                    SceneLoadEvents.InvokeSceneLoadProgress(sceneName, operation.progress);
                    await Task.Yield();
                }

                success = true;
            }

            SceneLoadEvents.InvokeSceneLoadCompleted(sceneName, success);
        }

        /// <summary>
        /// Unloads all currently loaded scenes except for the persistent scene.
        /// This method is asynchronous and will yield until all scenes are unloaded.
        /// </summary>
        private async Task UnloadAllScenesAsync()
        {
            // Collect scene names
            List<string> scenesToUnload = new();

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name != persistentSceneName)
                    scenesToUnload.Add(scene.name);
            }

            // Unload collected scenes
            foreach (string sceneName in scenesToUnload)
            {
                AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(sceneName);
                if (unloadOp == null)
                {
                    CustomLogger.LogWarning($"Tried to unload scene '{sceneName}', but it was not loaded.", this);
                    continue;
                }

                while (!unloadOp.isDone)
                    await Task.Yield();
            }
        }
    }
}