using Base.AttributePackage;
using Base.SettingsPackage.Display;
using Base.UtilityPackage.Identification;
using UnityEngine;

namespace Base.SettingsPackage.Components
{
    /// <summary>
    /// Stores an index into a curated list of <see cref="FullScreenMode"/> values and applies it through
    /// <see cref="DisplaySettings.SetFullScreenMode"/>. Should be placed earlier in the scene than
    /// <see cref="ResolutionSetting"/> so the active mode is set before resolution is applied.
    /// </summary>
    public sealed class FullScreenModeSetting : IntSettingComponent
    {
        [Title("Full Screen Mode")]
        [Tooltip("Modes exposed to the player, in the order they appear in the menu.")]
        [SerializeField] [NotNullOrEmpty]
        private FullScreenMode[] availableModes =
        {
            FullScreenMode.ExclusiveFullScreen,
            FullScreenMode.FullScreenWindow,
            FullScreenMode.Windowed
        };

        [SerializeField] private int defaultIndex = 1;

        /// <summary>The mode currently selected by the player.</summary>
        public FullScreenMode CurrentMode
        {
            get
            {
                int index = TypedSetting == null
                    ? defaultIndex
                    : TypedSetting.Value;

                return availableModes[ClampIndex(index)];
            }
        }

        /// <inheritdoc/>
        public override PersistentKey Key => new("FullScreen");

        /// <inheritdoc/>
        protected override int DefaultValue => ClampIndex(defaultIndex);

        /// <inheritdoc/>
        protected override void Apply(int index)
            => DisplaySettings.SetFullScreenMode(availableModes[ClampIndex(index)]);

        private int ClampIndex(int index) => Mathf.Clamp(index, 0, availableModes.Length - 1);
    }
}