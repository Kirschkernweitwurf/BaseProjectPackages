using Base.SettingsPackage.Display;
using UnityEngine;

namespace Base.SettingsPackage.Components
{
    /// <summary>
    /// Stores a "{width}x{height}" resolution label and applies it through
    /// <see cref="DisplaySettings.SetResolution"/>, using the active <see cref="Screen.fullScreenMode"/>.
    /// Should be placed later in the scene than <see cref="FullScreenModeSetting"/> so the mode is set first
    /// during initial load.
    /// </summary>
    public sealed class ResolutionSetting : StringSettingComponent
    {
        /// <inheritdoc/>
        public override string Key => "Resolution";

        /// <inheritdoc/>
        protected override string DefaultValue => ResolutionProvider.GetCurrentResolutionLabel();

        /// <inheritdoc/>
        protected override void Apply(string label) => DisplaySettings.SetResolution(label, Screen.fullScreenMode);
    }
}