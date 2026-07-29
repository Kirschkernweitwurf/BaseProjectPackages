using UnityEngine;
#if UNITY_EDITOR
using Base.UtilityPackage.Generated;
using UnityEditor;
#endif

namespace Base.UtilityPackage.Logging
{
    /// <summary>
    /// Installs the <see cref="CustomLogHandler"/> at startup, both in edit mode and play mode.
    /// Can be toggled in the editor through the menu entry it registers.
    /// </summary>
    public static class CustomLogHandlerBootstrap
    {
        private const string EnabledPrefKey = "Base.Logging.CustomLogHandler.Enabled";
        private const string MenuPath = "Tools/Base Packages/Unity Editor/Logging/Enable Custom Log Handler";

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

            // May already be ours from a previous session or domain reload.
            if (Debug.unityLogger.logHandler is CustomLogHandler)
                return;

            Debug.unityLogger.logHandler = new CustomLogHandler(Debug.unityLogger.logHandler);
        }

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void InstallEditor() => Install();

        [MenuItem(MenuPath, false, MenuOrders.UnityEditor)]
        private static void Toggle()
        {
            IsEnabled = !IsEnabled;

            if (IsEnabled)
                Install();
            else
                Uninstall();
        }

        [MenuItem(MenuPath, true, MenuOrders.UnityEditor)]
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