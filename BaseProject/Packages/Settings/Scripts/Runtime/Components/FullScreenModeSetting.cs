using Base.SettingsPackage.Display;
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
        [Header("Full Screen Mode")]
        [Tooltip("Modes exposed to the player, in the order they appear in the menu.")]
        [SerializeField]
        private FullScreenMode[] availableModes =
        {
            FullScreenMode.ExclusiveFullScreen,
            FullScreenMode.FullScreenWindow,
            FullScreenMode.Windowed
        };

        [SerializeField] private int defaultIndex = 1;

        /// <summary>The mode currently selected by the player.</summary>
        public FullScreenMode CurrentMode
            => availableModes[Mathf.Clamp(TypedSetting?.Value ?? defaultIndex, 0, availableModes.Length - 1)];

        /// <inheritdoc/>
        public override string Key => "FullScreen";

        /// <inheritdoc/>
        protected override int DefaultValue => Mathf.Clamp(defaultIndex, 0, availableModes.Length - 1);

        /// <inheritdoc/>
        protected override void Apply(int index)
        {
            int safeIndex = Mathf.Clamp(index, 0, availableModes.Length - 1);
            DisplaySettings.SetFullScreenMode(availableModes[safeIndex]);
        }
    }
}