namespace Base.SaveSystemPackage.Model
{
    /// <summary>
    /// Data needed to write a save. The slot id is required, the rest is optional.
    /// </summary>
    public readonly struct SaveRequest
    {
        /// <summary>The slot to write to. Use a slot provider to obtain or allocate one.</summary>
        public string SlotId { get; }

        /// <summary>New display name, or <c>null</c> to keep whatever the slot already had.</summary>
        public string DisplayName { get; }

        /// <summary>Total play time to stamp, or <c>null</c> to keep the existing value.</summary>
        public double? PlaytimeSeconds { get; }

        /// <summary>Optional thumbnail. Already encoded to PNG by the caller.</summary>
        public ScreenshotData? Screenshot { get; }

        /// <param name="slotId">The slot to write to.</param>
        /// <param name="displayName">New display name, or <c>null</c> to keep the existing one.</param>
        /// <param name="playtimeSeconds">Total play time to stamp, or <c>null</c> to keep the existing one.</param>
        /// <param name="screenshot">Optional thumbnail.</param>
        public SaveRequest(string slotId, string displayName = null, double? playtimeSeconds = null,
            ScreenshotData? screenshot = null)
        {
            SlotId = slotId;
            DisplayName = displayName;
            PlaytimeSeconds = playtimeSeconds;
            Screenshot = screenshot;
        }
    }
}