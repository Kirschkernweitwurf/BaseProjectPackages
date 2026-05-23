using Base.SystemsCorePackage.Services;
using UnityEngine;

namespace Base.SaveSystemPackage
{
    /// <summary>
    /// Grabs the current screen as a Texture2D for use as a save thumbnail.
    /// </summary>
    public interface IScreenshotCapturer : IGameService
    {
        /// <summary>
        /// Capture a thumbnail. Must be awaited (it waits for end of frame).
        /// </summary>
        /// <param name="maxWidth">Target width. The full screen is returned untouched
        /// if it is already this small or smaller.</param>
        Awaitable<Texture2D> CaptureAsync(int maxWidth = 480);
    }
}