using UnityEditor;
using UnityEngine;

namespace Base.UtilityPackage.Logging.CustomLogHandler
{
    /// <summary>
    /// Installs the custom log handler at startup, both in edit mode and play mode.
    /// Can be toggled in the editor via the menu item "Tools/Base Packages/Logging/Enable Custom Log Handler".
    /// </summary>
    public static class CustomLogHandlerBootstrap
    {
        private const string EnabledPrefKey = "Base.Logging.CustomLogHandler.Enabled";
        private const string MenuPath = "Tools/Base Packages/Unity Editor/Logging/Enable Custom Log Handler";
        private const int MenuPriority = -11;

        // In builds there are no EditorPrefs, so the handler is always enabled.
        private static bool IsEnabled
        {
#if UNITY_EDITOR
            get => EditorPrefs.GetBool(EnabledPrefKey, false);
            set => EditorPrefs.SetBool(EnabledPrefKey, value);
#else
            get => true;
#endif
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void InstallRuntime() => Install();

        private static void Install()
        {
            if (!IsEnabled)
                return;

            // May already be ours from a previous session / domain reload.
            if (Debug.unityLogger.logHandler is CustomLogHandler)
                return;

            ILogHandler unityDefault = Debug.unityLogger.logHandler;
            Debug.unityLogger.logHandler = new CustomLogHandler(unityDefault);
        }

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void InstallEditor() => Install();

        [MenuItem(MenuPath, false, MenuPriority)]
        private static void Toggle()
        {
            IsEnabled = !IsEnabled;
            if (IsEnabled)
                Install();
            else
                Uninstall();
        }

        [MenuItem(MenuPath, true, MenuPriority)]
        private static bool ToggleValidate()
        {
            Menu.SetChecked(MenuPath, IsEnabled);
            return true;
        }

        private static void Uninstall()
        {
            // Restore the genuine Unity handler we previously wrapped.
            if (Debug.unityLogger.logHandler is CustomLogHandler custom)
                Debug.unityLogger.logHandler = custom.DefaultLogHandler;
        }
#endif
    }
}