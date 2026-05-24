using System;

namespace Base.SaveSystemPackage.Model
{
    /// <summary>
    /// Immutable info stored alongside the save data and shown in a load/continue menu.
    /// </summary>
    public sealed class SaveMetadata
    {
        public string SlotId { get; }
        public string DisplayName { get; }
        public int SaveVersion { get; }
        public string AppVersion { get; }
        public DateTime CreatedUtc { get; }
        public DateTime LastSavedUtc { get; }
        public TimeSpan TotalPlayTime { get; }
        public bool HasScreenshot { get; }
        public int ScreenshotWidth { get; }
        public int ScreenshotHeight { get; }

        public SaveMetadata(string slotId, string displayName, int saveVersion, string appVersion,
            DateTime createdUtc, DateTime lastSavedUtc, TimeSpan totalPlayTime, bool hasScreenshot,
            int screenshotWidth, int screenshotHeight)
        {
            SlotId = slotId;
            DisplayName = displayName;
            SaveVersion = saveVersion;
            AppVersion = appVersion;
            CreatedUtc = createdUtc;
            LastSavedUtc = lastSavedUtc;
            TotalPlayTime = totalPlayTime;
            HasScreenshot = hasScreenshot;
            ScreenshotWidth = screenshotWidth;
            ScreenshotHeight = screenshotHeight;
        }

        /// <summary>
        /// Returns a copy with the given fields replaced. Pass only what changes; the rest is kept.
        /// </summary>
        public SaveMetadata With(string displayName = null, int? saveVersion = null, string appVersion = null,
            DateTime? lastSavedUtc = null, TimeSpan? totalPlayTime = null, bool? hasScreenshot = null,
            int? screenshotWidth = null, int? screenshotHeight = null)
        {
            return new SaveMetadata(
                SlotId,
                displayName ?? DisplayName,
                saveVersion ?? SaveVersion,
                appVersion ?? AppVersion,
                CreatedUtc,
                lastSavedUtc ?? LastSavedUtc,
                totalPlayTime ?? TotalPlayTime,
                hasScreenshot ?? HasScreenshot,
                screenshotWidth ?? ScreenshotWidth,
                screenshotHeight ?? ScreenshotHeight);
        }

        /// <summary>
        /// A fresh metadata for a brand-new save in the given slot.
        /// </summary>
        public static SaveMetadata CreateNew(string slotId, int saveVersion, string appVersion, DateTime nowUtc)
        {
            return new SaveMetadata
            (
                slotId,
                displayName: null,
                saveVersion,
                appVersion,
                createdUtc: nowUtc,
                lastSavedUtc: nowUtc,
                totalPlayTime: TimeSpan.Zero,
                hasScreenshot: false,
                screenshotWidth: 0,
                screenshotHeight: 0
            );
        }
    }
}