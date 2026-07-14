using Base.SettingsPackage.Display;
using Base.ToolPackage.Identification;
using UnityEngine;

namespace Base.SettingsPackage.Components
{
    /// <summary>
    /// Stores the VSync count and applies it through <see cref="DisplaySettings.SetVSync"/>.
    /// Should be placed earlier in the scene than <see cref="QualityLevelSetting"/> so VSync is loaded before
    /// the quality level, which can overwrite it during application.
    /// </summary>
    public sealed class VSyncSetting : IntSettingComponent
    {
        [Header("VSync")]

        [SerializeField] [Range(0, 4)] private int defaultVSyncCount = 1;

        /// <inheritdoc/>
        public override PersistentKey Key => new("VSync");

        /// <inheritdoc/>
        protected override int DefaultValue => defaultVSyncCount;

        /// <inheritdoc/>
        protected override void Apply(int value) => DisplaySettings.SetVSync(value);
    }
}