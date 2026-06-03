#if UNITY_EDITOR
using UnityEditor;

namespace Base.UtilityPackage.Identification.Editor
{
    /// <summary>
    /// Central on/off switch for the Unique ID package.
    /// <para/>
    /// When disabled, none of the automatic validation runs:
    /// no checks on editor load, no checks when assets are imported,
    /// and no checks before a build. Turn it back on any time from the menu.
    /// <para/>
    /// Toggle it from: <b>Tools/Base Packages/Identifier/Enable Unique IDs</b>.
    /// </summary>
    public static class UniqueIdSettings
    {
        private const string MenuPath = "Tools/Base Packages/Identifier/Enable Unique IDs";

        private static string EnabledKey => $"Base.UniqueId.Enabled.{PlayerSettings.productGUID}";

        /// <summary>
        /// True if the package is allowed to run its automatic validation.
        /// Defaults to <c>true</c> (on).
        /// </summary>
        public static bool Enabled
        {
            get => EditorPrefs.GetBool(EnabledKey, true);
            private set => EditorPrefs.SetBool(EnabledKey, value);
        }

        [MenuItem(MenuPath, priority = 0)]
        private static void Toggle() => Enabled = !Enabled;

        [MenuItem(MenuPath, validate = true)]
        private static bool ToggleValidate()
        {
            Menu.SetChecked(MenuPath, Enabled);
            return true;
        }
    }
}
#endif