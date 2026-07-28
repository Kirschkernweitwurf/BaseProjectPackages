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

        /// <summary>Flattens domain metadata into the shape written to disk.</summary>
        public static SaveMetadataDto From(SaveMetadata metadata) => new()
        {
            slotId = metadata.SlotId,
            displayName = metadata.DisplayName,
            saveVersion = metadata.SaveVersion,
            appVersion = metadata.AppVersion,
            createdUtcTicks = metadata.CreatedUtc.Ticks,
            lastSavedUtcTicks = metadata.LastSavedUtc.Ticks,
            totalPlaySeconds = metadata.TotalPlayTime.TotalSeconds,
            hasScreenshot = metadata.HasScreenshot,
            screenshotWidth = metadata.ScreenshotWidth,
            screenshotHeight = metadata.ScreenshotHeight
        };

        /// <summary>Rebuilds the domain metadata from the stored shape.</summary>
        public SaveMetadata ToDomain() => new(slotId,
            displayName,
            saveVersion,
            appVersion,
            createdUtc: new DateTime(createdUtcTicks, DateTimeKind.Utc),
            lastSavedUtc: new DateTime(lastSavedUtcTicks, DateTimeKind.Utc),
            totalPlayTime: TimeSpan.FromSeconds(totalPlaySeconds),
            hasScreenshot,
            screenshotWidth,
            screenshotHeight);
    }
}