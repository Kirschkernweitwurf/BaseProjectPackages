using Base.SaveSystemPackage.Model;
using Base.SaveSystemPackage.Unity.Capture;
using Base.SaveSystemPackage.Unity.Playtime;
using Base.ServicePackage;
using UnityEngine;

namespace Base.SaveSystemPackage.Unity.Composition
{
    /// <summary>
    /// Builds a <see cref="SaveRequest"/> from whatever optional services the scene happens to have.
    /// Every caller that writes a save wants the same thumbnail and play time stamped into it, so the
    /// two lookups and the texture handling live here rather than in each button and timer.
    /// </summary>
    public static class SaveRequestFactory
    {
        /// <summary>
        /// Collects a thumbnail and the current play time, if anything provides them, and packs them
        /// into a request.
        /// </summary>
        /// <param name="slotId">The slot the save is written to.</param>
        /// <param name="displayName">Name to store, or <c>null</c> to keep the slot's existing one.</param>
        /// <param name="captureScreenshot">False to skip the thumbnail even when a capturer exists.</param>
        /// <returns>The request, ready to hand to a save.</returns>
        public static async Awaitable<SaveRequest> CreateAsync(string slotId, string displayName = null,
            bool captureScreenshot = true)
        {
            ScreenshotData? screenshot = captureScreenshot
                ? await CaptureAsync()
                : null;

            // Optional: a project without a tracker simply stores no play time.
            double? playtimeSeconds = ServiceLocator.TryGetOptional(out IPlaytimeProvider playtime)
                ? playtime.TotalSeconds
                : null;

            return new SaveRequest(slotId, displayName, playtimeSeconds, screenshot);
        }

        private static async Awaitable<ScreenshotData?> CaptureAsync()
        {
            // Thumbnails are opt-in: without a capturer in the scene a save simply has no image.
            if (!ServiceLocator.TryGetOptional(out IScreenshotCapturer capturer))
                return null;

            Texture2D texture = await capturer.CaptureAsync();
            if (texture == null)
                return null;

            ScreenshotData screenshot = new(texture.EncodeToPNG(), texture.width, texture.height);
            Object.Destroy(texture);

            return screenshot;
        }
    }
}