using System;
using UnityEngine;

namespace Base.CorePackage.SceneManagement
{
    /// <summary>
    /// Provides events related to scene loading operations.
    /// </summary>
    public static class SceneLoadEvents
    {
        /// <summary>
        /// Invoked when a scene load operation starts.
        /// </summary>
        public static event Action<string> OnSceneLoadStarted;

        /// <summary>
        /// Invoked to report progress of an ongoing scene load operation.
        /// </summary>
        public static event Action<string, float> OnSceneLoadProgress;

        /// <summary>
        /// Invoked when a scene load operation completes, indicating success or failure.
        /// </summary>
        public static event Action<string, bool> OnSceneLoadCompleted;

        /// <summary>Raises <see cref="OnSceneLoadStarted"/>. Only the scene loader calls this.</summary>
        /// <param name="sceneName">The scene beginning to load.</param>
        internal static void InvokeSceneLoadStarted(string sceneName) => OnSceneLoadStarted?.Invoke(sceneName);

        /// <summary>Raises <see cref="OnSceneLoadProgress"/>. Only the scene loader calls this.</summary>
        /// <param name="sceneName">The scene being loaded.</param>
        /// <param name="progress">How far along the load is, from 0 to 1.</param>
        internal static void InvokeSceneLoadProgress(string sceneName, float progress)
            => OnSceneLoadProgress?.Invoke(sceneName, progress);

        /// <summary>Raises <see cref="OnSceneLoadCompleted"/>. Only the scene loader calls this.</summary>
        /// <param name="sceneName">The scene that finished loading.</param>
        /// <param name="success">False when the load failed, which subscribers have to handle.</param>
        internal static void InvokeSceneLoadCompleted(string sceneName, bool success)
            => OnSceneLoadCompleted?.Invoke(sceneName, success);

        // With domain reload disabled, handlers from the previous play session survive and would fire
        // into destroyed objects, so clear them before the first scene loads.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetEvents()
        {
            OnSceneLoadStarted = null;
            OnSceneLoadProgress = null;
            OnSceneLoadCompleted = null;
        }
    }
}