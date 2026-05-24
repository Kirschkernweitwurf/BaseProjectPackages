using System;
using Base.SaveSystemPackage.Model;

namespace Base.SaveSystemPackage.Serialization.Wire
{
    /// <summary>
    /// Serialization shape for <see cref="SaveMetadata"/>. DTO = Data Transfer Object.
    /// </summary>
    [Serializable]
    internal sealed class SaveMetadataDto
    {
        public string slotId;
        public string displayName;
        public int saveVersion;
        public string appVersion;
        public long createdUtcTicks;
        public long lastSavedUtcTicks;
        public double totalPlaySeconds;
        public bool hasScreenshot;
        public int screenshotWidth;
        public int screenshotHeight;

        public static SaveMetadataDto From(SaveMetadata m) => new()
        {
            slotId = m.SlotId,
            displayName = m.DisplayName,
            saveVersion = m.SaveVersion,
            appVersion = m.AppVersion,
            createdUtcTicks = m.CreatedUtc.Ticks,
            lastSavedUtcTicks = m.LastSavedUtc.Ticks,
            totalPlaySeconds = m.TotalPlayTime.TotalSeconds,
            hasScreenshot = m.HasScreenshot,
            screenshotWidth = m.ScreenshotWidth,
            screenshotHeight = m.ScreenshotHeight
        };

        public SaveMetadata ToDomain() => new
        (
            slotId,
            displayName,
            saveVersion,
            appVersion,
            new DateTime(createdUtcTicks, DateTimeKind.Utc),
            new DateTime(lastSavedUtcTicks, DateTimeKind.Utc),
            TimeSpan.FromSeconds(totalPlaySeconds),
            hasScreenshot,
            screenshotWidth,
            screenshotHeight
        );
    }
}