using Base.UtilityPackage.Logging;
using Base.UtilityPackage.Menus;
using UnityEditor;

namespace Base.UtilityPackage.Editor.Logging
{
    /// <summary>
    /// Owns the editor side of the <see cref="CustomLogHandler"/>: the persisted on/off toggle, the
    /// menu entry that flips it and applying it on every domain load.
    /// </summary>
    internal static class CustomLogHandlerMenu
    {
        private const string EnabledPrefKey = "Base.Logging.CustomLogHandler.Enabled";
        private const string MenuPath = "Tools/Base Packages/Unity Editor/Logging/Enable Custom Log Handler";

        private static bool IsEnabled
        {
            get => EditorPrefs.GetBool(EnabledPrefKey, false);
            set => EditorPrefs.SetBool(EnabledPrefKey, value);
        }

        /// <summary>Flips the toggle and applies it right away.</summary>
        [DynamicMenuItem(MenuPath, checkedMethod: nameof(IsHandlerEnabled))]
        private static void Toggle()
        {
            IsEnabled = !IsEnabled;

            Apply();
        }

        /// <summary>Reports the stored toggle so the Menu Manager can draw the check mark.</summary>
        /// <returns><c>true</c> while the handler is switched on.</returns>
        private static bool IsHandlerEnabled() => IsEnabled;

        [InitializeOnLoadMethod]
        private static void ApplyOnLoad() => Apply();

        private static void Apply()
        {
            if (IsEnabled)
                CustomLogHandlerBootstrap.Install();
            else
                CustomLogHandlerBootstrap.Uninstall();
        }
    }
}