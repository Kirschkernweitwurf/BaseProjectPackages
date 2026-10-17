using Base.UtilityPackage.Menus;
using UnityEditor;
using UnityEngine;

namespace Base.ToolsPackage.Editor.Identification
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
    /// Toggle it from: <b>Tools/Base Packages/Assets/Unique IDs/Unique ID Validation</b>.
    /// </summary>
    [FilePath("ProjectSettings/UniqueIdSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class UniqueIdSettings : ScriptableSingleton<UniqueIdSettings>
    {
        private const string MenuPath = "Tools/Base Packages/Assets/Unique IDs/Unique ID Validation";

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

        // The Menu Manager draws the check mark at the path the entry actually sits at, so it survives a move.
        [DynamicMenuItem(MenuPath, checkedMethod: nameof(IsValidationEnabled))]
        private static void Toggle() => Enabled = !Enabled;

        private static bool IsValidationEnabled() => Enabled;
    }
}