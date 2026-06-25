#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Base.UtilityPackage.Editor.Identification
{
    /// <summary>
    /// Central on/off switch for the Unique ID package.
    /// <para/>
    /// When disabled, none of the automatic validation runs:
    /// no checks on editor load, no checks when assets are imported,
    /// and no checks before a build. Turn it back on any time from the menu.
    /// <para/>
    /// The value is saved to <c>ProjectSettings/UniqueIdSettings.asset</c>, so it is
    /// shared through source control and applies to the whole team.
    /// <para/>
    /// Toggle it from: <b>Tools/Base Packages/Identifier/Enable Unique IDs</b>.
    /// </summary>
    [FilePath("ProjectSettings/UniqueIdSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public sealed class UniqueIdSettings : ScriptableSingleton<UniqueIdSettings>
    {
        private const string MenuPath = "Tools/Base Packages/Identifier/Enable Unique IDs";
        private const int MenuPriority = -36;

        [SerializeField] private bool enabled = true;

        /// <summary>
        /// True if the package is allowed to run its automatic validation.
        /// Defaults to <c>true</c> (on).
        /// </summary>
        public static bool Enabled
        {
            get => instance.enabled;
            private set
            {
                if (instance.enabled == value)
                    return;

                instance.enabled = value;
                instance.Save(true);
            }
        }

        [MenuItem(MenuPath, priority = MenuPriority)]
        private static void Toggle() => Enabled = !Enabled;

        // Puts the checkmark next to the menu item when the package is on.
        [MenuItem(MenuPath, validate = true, priority = MenuPriority)]
        private static bool ToggleValidate()
        {
            Menu.SetChecked(MenuPath, Enabled);
            return true;
        }
    }
}
#endif