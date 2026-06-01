using Base.SettingsPackage.Display;
using UnityEngine;

namespace Base.SettingsPackage.Components
{
    /// <summary>
    /// Stores the Unity quality level index and applies it through <see cref="DisplaySettings.SetQualityLevel"/>,
    /// which preserves the current VSync count across the level change.
    /// </summary>
    public sealed class QualityLevelSetting : IntSettingComponent
    {
        [Header("Quality")]
        [Tooltip("Index into Unity's quality levels. Leave at -1 to use whatever Unity has set on first run.")]
        [SerializeField] private int defaultQualityLevel = -1;

        /// <inheritdoc/>
        public override string Key => "Quality";

        /// <inheritdoc/>
        protected override int DefaultValue =>
            defaultQualityLevel < 0 ? QualitySettings.GetQualityLevel() : defaultQualityLevel;

        /// <inheritdoc/>
        protected override void Apply(int level) => DisplaySettings.SetQualityLevel(level);
    }
}