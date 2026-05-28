using UnityEngine;

namespace Base.SettingsPackage.Base_Settings_Package.Scripts.Runtime
{
    /// <summary>
    /// Applies the display related settings: fullscreen, vertical sync and quality level.
    /// </summary>
    public sealed class DisplaySettingsApplier : SettingsApplier
    {
        protected override void OnInitialized(ISettingsService settings)
        {
            settings.Fullscreen.Subscribe(HandleFullscreenChanged);
            settings.VSync.Subscribe(HandleVSyncChanged);
            settings.QualityLevel.Subscribe(HandleQualityLevelChanged);
        }

        protected override void OnTeardown()
        {
            Settings.Fullscreen.Unsubscribe(HandleFullscreenChanged);
            Settings.VSync.Unsubscribe(HandleVSyncChanged);
            Settings.QualityLevel.Unsubscribe(HandleQualityLevelChanged);
        }

        private static void HandleFullscreenChanged(bool isFullscreen) => Screen.fullScreen = isFullscreen;

        private static void HandleVSyncChanged(bool isEnabled) => QualitySettings.vSyncCount = isEnabled ? 1 : 0;

        private static void HandleQualityLevelChanged(int level)
        {
            int maxLevel = QualitySettings.names.Length - 1;
            int clampedLevel = Mathf.Clamp(level, 0, maxLevel);
            QualitySettings.SetQualityLevel(clampedLevel, true);
        }
    }
}