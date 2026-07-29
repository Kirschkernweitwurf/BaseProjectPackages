using Base.CorePackage.Services;
using UnityEngine;

namespace Base.SaveSystemPackage.Unity.Capture
{
    /// <summary>
    /// Captures the screen and downscales it to a thumbnail.
    /// </summary>
    public sealed class ScreenCapturer : MonoBehaviour, IScreenshotCapturer
    {
        private const int NoDepthBuffer = 0;

#region Unity Callbacks
        private void Awake() => ServiceLocator.Register<IScreenshotCapturer>(this);

        private void OnDestroy() => ServiceLocator.Deregister<IScreenshotCapturer>();
#endregion

        /// <inheritdoc/>
        public async Awaitable<Texture2D> CaptureAsync(int maxWidth = IScreenshotCapturer.DefaultMaxWidth)
        {
            await Awaitable.EndOfFrameAsync();

            Texture2D full = ScreenCapture.CaptureScreenshotAsTexture();
            if (full.width <= maxWidth)
                return full;

            int height = Mathf.RoundToInt(full.height * (maxWidth / (float)full.width));
            Texture2D thumbnail = Downscale(full, maxWidth, height);

            Destroy(full);
            return thumbnail;
        }

        private static Texture2D Downscale(Texture2D source, int targetWidth, int targetHeight)
        {
            // Blitting through a mip pyramid instead of a plain downscale avoids aliasing on the
            // thumbnail, which is very visible at this size.
            bool linearProject = QualitySettings.activeColorSpace == ColorSpace.Linear;
            RenderTextureReadWrite readWrite = linearProject
                ? RenderTextureReadWrite.sRGB
                : RenderTextureReadWrite.Linear;

            RenderTextureDescriptor fullDescriptor =
                new(source.width, source.height, RenderTextureFormat.ARGB32, NoDepthBuffer)
                {
                    useMipMap = true,
                    autoGenerateMips = false,
                    sRGB = linearProject
                };

            RenderTexture pyramid = RenderTexture.GetTemporary(fullDescriptor);

            Graphics.Blit(source, pyramid);
            pyramid.GenerateMips();
            pyramid.filterMode = FilterMode.Trilinear;

            RenderTexture small = RenderTexture.GetTemporary(targetWidth, targetHeight, NoDepthBuffer,
                RenderTextureFormat.ARGB32, readWrite);

            Graphics.Blit(pyramid, small);

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = small;

            Texture2D result = new(targetWidth, targetHeight, TextureFormat.RGB24, false);
            result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
            result.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(pyramid);
            RenderTexture.ReleaseTemporary(small);

            return result;
        }
    }
}