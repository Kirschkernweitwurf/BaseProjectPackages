using UnityEngine;

namespace Base.UtilityPackage.Logging
{
    /// <summary>
    /// Installs and removes the <see cref="CustomLogHandler"/>. In a build the handler is installed
    /// automatically at startup; in the editor the toggle under Tools decides, so the editor assembly
    /// drives it instead and this type only exposes the operations.
    /// </summary>
    public static class CustomLogHandlerBootstrap
    {
        /// <summary>True while the custom handler is the one Unity logs through.</summary>
        public static bool IsInstalled => Debug.unityLogger.logHandler is CustomLogHandler;

        /// <summary>Wraps Unity's log handler with the custom one, unless that already happened.</summary>
        public static void Install()
        {
            // May already be ours from a previous session or domain reload.
            if (IsInstalled)
                return;

            Debug.unityLogger.logHandler = new CustomLogHandler(Debug.unityLogger.logHandler);
        }

        /// <summary>Restores the log handler that was wrapped, if the custom one is installed.</summary>
        public static void Uninstall()
        {
            // Restore the genuine Unity handler we previously wrapped.
            if (Debug.unityLogger.logHandler is CustomLogHandler custom)
                Debug.unityLogger.logHandler = custom.DefaultLogHandler;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void InstallOnLoad()
        {
            // In the editor the menu toggle owns this, and the editor assembly has already applied it
            // on domain load. Installing here would ignore a toggle that is switched off.
            if (Application.isEditor)
                return;

            Install();
        }
    }
}