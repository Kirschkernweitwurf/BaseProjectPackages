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
        /// <summary>Identifies the slot the save belongs to.</summary>
        public string slotId;

        /// <summary>The name shown for the slot in a load menu.</summary>
        public string displayName;

        /// <summary>Format version of the saved data, which is what a migration keys off.</summary>
        public int saveVersion;

        /// <summary>Application version the save was written by, for diagnosing a bad load.</summary>
        public string appVersion;

        /// <summary>When the slot was first written, as UTC ticks.</summary>
        /// <remarks>
        /// Stored as ticks rather than as a <see cref="DateTime"/>, because JsonUtility writes a
        /// DateTime as an opaque struct that does not survive a round trip.
        /// </remarks>
        public long createdUtcTicks;

        /// <summary>When the slot was last written, as UTC ticks.</summary>
        public long lastSavedUtcTicks;

        /// <summary>Accumulated play time in seconds, for the same round trip reason as the ticks.</summary>
        public double totalPlaySeconds;

        /// <summary>Whether a screenshot was written next to the save.</summary>
        public bool hasScreenshot;

        /// <summary>Pixel width of the screenshot, so a slot list can lay out before decoding it.</summary>
        public int screenshotWidth;

        /// <summary>Pixel height of the screenshot.</summary>
        public int screenshotHeight;

        /// <summary>Flattens domain metadata into the shape written to disk.</summary>
        internal static SaveMetadataDto From(SaveMetadata metadata) => new()
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
        internal SaveMetadata ToDomain() => new(slotId,
            displayName,
            saveVersion,
            appVersion,
            new DateTime(createdUtcTicks, DateTimeKind.Utc),
            new DateTime(lastSavedUtcTicks, DateTimeKind.Utc),
            TimeSpan.FromSeconds(totalPlaySeconds),
            hasScreenshot,
            screenshotWidth,
            screenshotHeight);
    }
}