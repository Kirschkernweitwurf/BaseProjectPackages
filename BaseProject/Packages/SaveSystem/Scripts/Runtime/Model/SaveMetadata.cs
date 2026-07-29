using System;

namespace Base.SaveSystemPackage.Model
{
    /// <summary>
    /// Immutable info stored alongside the save data and shown in a load or continue menu.
    /// </summary>
    public sealed class SaveMetadata
    {
        /// <summary>The slot this metadata belongs to.</summary>
        public string SlotId { get; }

        /// <summary>Name shown in a menu, or <c>null</c> when the slot was never named.</summary>
        public string DisplayName { get; }

        /// <summary>The schema version the save was written at.</summary>
        public int SaveVersion { get; }

        /// <summary>The application version that wrote the save.</summary>
        public string AppVersion { get; }

        /// <summary>When the slot was first written.</summary>
        public DateTime CreatedUtc { get; }

        /// <summary>When the slot was last written.</summary>
        public DateTime LastSavedUtc { get; }

        /// <summary>Total play time stamped into the save.</summary>
        public TimeSpan TotalPlayTime { get; }

        /// <summary>Whether a thumbnail was stored next to the data.</summary>
        public bool HasScreenshot { get; }

        /// <summary>Width of the stored thumbnail in pixels, or zero when there is none.</summary>
        public int ScreenshotWidth { get; }

        /// <summary>Height of the stored thumbnail in pixels, or zero when there is none.</summary>
        public int ScreenshotHeight { get; }

        /// <summary>
        /// Builds a complete metadata record. Prefer <see cref="CreateNew"/> and <see cref="With"/>
        /// over calling this directly.
        /// </summary>
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
        /// Fresh metadata for a brand new save in the given slot.
        /// </summary>
        /// <param name="slotId">The slot being created.</param>
        /// <param name="saveVersion">The current schema version.</param>
        /// <param name="appVersion">The current application version.</param>
        /// <param name="nowUtc">The moment the save is being created.</param>
        /// <returns>Metadata with no name, no play time and no screenshot.</returns>
        public static SaveMetadata CreateNew(string slotId, int saveVersion, string appVersion, DateTime nowUtc) => new(
            slotId,
            null,
            saveVersion,
            appVersion,
            nowUtc,
            nowUtc,
            TimeSpan.Zero,
            false,
            0,
            0);

        /// <summary>
        /// Returns a copy with the given fields replaced. Pass only what changes; the rest is kept.
        /// </summary>
        /// <returns>A new instance. This one is left untouched.</returns>
        public SaveMetadata With(string displayName = null, int? saveVersion = null, string appVersion = null,
            DateTime? lastSavedUtc = null, TimeSpan? totalPlayTime = null, bool? hasScreenshot = null,
            int? screenshotWidth = null, int? screenshotHeight = null) => new(SlotId,
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
}