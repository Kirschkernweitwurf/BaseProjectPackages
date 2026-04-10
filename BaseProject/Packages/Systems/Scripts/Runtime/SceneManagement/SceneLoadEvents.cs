using System;

namespace SceneManagement
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

        internal static void InvokeSceneLoadStarted(string sceneName)
            => OnSceneLoadStarted?.Invoke(sceneName);

        internal static void InvokeSceneLoadProgress(string sceneName, float progress)
            => OnSceneLoadProgress?.Invoke(sceneName, progress);

        internal static void InvokeSceneLoadCompleted(string sceneName, bool success)
            => OnSceneLoadCompleted?.Invoke(sceneName, success);
    }
}