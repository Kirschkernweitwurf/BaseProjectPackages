using Base.SystemsCorePackage.Services;
using UnityEngine;

namespace Base.SaveSystemPackage
{
    /// <summary>
    /// Captures the screen and downscales it to a thumbnail.
    /// </summary>
    public sealed class ScreenCaptureCapturer : MonoBehaviour, IScreenshotCapturer
    {
        private void Awake() => ((IGameService)this).Register();

        private void OnDestroy() => ((IGameService)this).Deregister();

        /// <inheritdoc/>
        public async Awaitable<Texture2D> CaptureAsync(int maxWidth = 480)
        {
            await Awaitable.EndOfFrameAsync();

            Texture2D full = ScreenCapture.CaptureScreenshotAsTexture();
            if (full.width <= maxWidth)
                return full;

            int height = Mathf.RoundToInt(full.height * (maxWidth / (float)full.width));
            Texture2D thumb = Downscale(full, maxWidth, height);

            Destroy(full);
            return thumb;
        }

        private static Texture2D Downscale(Texture2D src, int targetWidth, int targetHeight)
        {
            bool linearProject = QualitySettings.activeColorSpace == ColorSpace.Linear;
            RenderTextureReadWrite rw = linearProject ? RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Linear;

            RenderTextureDescriptor fullDesc = new(src.width, src.height, RenderTextureFormat.ARGB32, 0)
            {
                useMipMap = true,
                autoGenerateMips = false,
                sRGB = linearProject
            };
            RenderTexture pyramid = RenderTexture.GetTemporary(fullDesc);

            Graphics.Blit(src, pyramid);
            pyramid.GenerateMips();
            pyramid.filterMode = FilterMode.Trilinear;

            RenderTexture small = RenderTexture.GetTemporary(
                targetWidth, targetHeight, 0, RenderTextureFormat.ARGB32, rw);

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