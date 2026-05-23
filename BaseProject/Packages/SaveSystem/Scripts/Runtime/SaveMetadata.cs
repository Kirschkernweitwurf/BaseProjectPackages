using System;
using UnityEngine;

namespace Base.SaveSystemPackage
{
    /// <summary>
    /// Info that is saved next to your game data.
    /// Plain public fields so Unity's <see cref="JsonUtility"/> can serialize it.
    /// </summary>
    [Serializable]
    public sealed class 
        SaveMetadata
    {
        /// <summary>
        /// When the save was first created.
        /// </summary>
        public DateTime CreatedUtc => new(createdUtcTicks, DateTimeKind.Utc);

        /// <summary>
        /// When the save was last updated.
        /// </summary>
        public DateTime LastSavedUtc => new(lastSavedUtcTicks, DateTimeKind.Utc);

        /// <summary>
        /// Total play time in this save.
        /// </summary>
        public TimeSpan TotalPlayTime => TimeSpan.FromSeconds(totalPlaySeconds);

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
    }
}