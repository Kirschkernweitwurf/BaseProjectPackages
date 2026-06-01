using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.SettingsPackage.Display
{
    /// <summary>
    /// Reusable wrappers around Unity's display APIs. These are the universal apply operations every project
    /// needs; the consuming project calls them from its setting appliers. Quality level reapplies VSync because
    /// changing the quality level can overwrite it.
    /// </summary>
    public static class DisplaySettings
    {
        /// <summary>Sets the VSync count (0 disables it).</summary>
        public static void SetVSync(int count) => QualitySettings.vSyncCount = count;

        /// <summary>Sets the quality level and reapplies VSync so the level change does not clobber it.</summary>
        public static void SetQualityLevel(int level, bool applyExpensiveChanges = true)
        {
            int vSync = QualitySettings.vSyncCount;
            QualitySettings.SetQualityLevel(level, applyExpensiveChanges);
            QualitySettings.vSyncCount = vSync;
        }

        /// <summary>Applies a full screen mode while keeping the current resolution.</summary>
        public static void SetFullScreenMode(FullScreenMode mode)
        {
            Screen.SetResolution(Screen.width, Screen.height, mode);
        }

        /// <summary>Applies a resolution from a "{width}x{height}" label. Returns false on a malformed label.</summary>
        public static bool SetResolution(string label, FullScreenMode mode)
        {
            if (!ResolutionProvider.TryParse(label, out int width, out int height))
            {
                CustomLogger.LogError($"Invalid resolution label: '{label}'.", null);
                return false;
            }

            Screen.SetResolution(width, height, mode);
            return true;
        }
    }
}