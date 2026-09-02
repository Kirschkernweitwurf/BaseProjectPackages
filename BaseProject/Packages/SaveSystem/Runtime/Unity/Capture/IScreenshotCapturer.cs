using Base.ServicesPackage;
using UnityEngine;

namespace Base.SaveSystemPackage.Unity.Capture
{
    /// <summary>
    /// Grabs the current screen as a Texture2D for use as a save thumbnail.
    /// </summary>
    public interface IScreenshotCapturer : IGameService
    {
        /// <summary>Thumbnail width used when the caller does not ask for a specific one.</summary>
        public const int DefaultMaxWidth = 480;

        /// <summary>
        /// Capture a thumbnail. Must be awaited, since it waits for the end of the frame.
        /// </summary>
        /// <param name="maxWidth">
        /// Target width. The full screen is returned untouched if it is already this small or smaller.
        /// </param>
        /// <returns>The thumbnail. The caller owns it and has to destroy it.</returns>
        Awaitable<Texture2D> CaptureAsync(int maxWidth = DefaultMaxWidth);
    }
}