using Base.AttributePackage;
using Base.SettingsPackage.Display;
using Base.ToolPackage.Identification;
using UnityEngine;

namespace Base.SettingsPackage.Components
{
    /// <summary>
    /// Stores the Unity quality level index and applies it through <see cref="DisplaySettings.SetQualityLevel"/>,
    /// which preserves the current VSync count across the level change.
    /// </summary>
    public sealed class QualityLevelSetting : IntSettingComponent
    {
        private const int UseCurrentLevel = -1;

        [Title("Quality")]
        [Tooltip("Index into Unity's quality levels. Leave negative to use whatever Unity has set on first run.")]
        [SerializeField] private int defaultQualityLevel = UseCurrentLevel;

        /// <inheritdoc/>
        public override PersistentKey Key => new("Quality");

        /// <inheritdoc/>
        protected override int DefaultValue => defaultQualityLevel <= UseCurrentLevel
            ? QualitySettings.GetQualityLevel()
            : defaultQualityLevel;

        /// <inheritdoc/>
        protected override void Apply(int level) => DisplaySettings.SetQualityLevel(level);
    }
}