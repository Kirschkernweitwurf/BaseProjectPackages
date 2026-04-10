using System.IO;
using Systems.Input;
using Systems.Services;
using Systems.Shutdown;
using UnityEngine;
using UnityEngine.InputSystem;
using Utility.Logging;

namespace Managers
{
    /// <summary>
    /// Manages the creation and storage of screenshots on input.
    /// </summary>
    public class ScreenshotManager : MonoBehaviour, IShutdownHandler
    {
        private const string ScreenshotNameFormat = "{GameTitle}_{Width}x{Height}_{Time}";

        private static string ScreenshotDirectory => Path.Combine(Application.persistentDataPath, "Screenshots");

        public bool HasShutDown { get; private set; }

        private void Awake()
        {
            ShutdownManager.Register(this);

            if (!ServiceLocator.TryGet(out InputManager inputManager))
                return;

            inputManager.InputActions.Permanent.CreateSmallScreenshot.started += CreateSmallScreenshot;
            inputManager.InputActions.Permanent.CreateLargeScreenshot.started += CreateLargeScreenshot;
        }

        private void OnDestroy()
        {
            if (!HasShutDown)
                Shutdown();
        }

        public void Shutdown()
        {
            ShutdownManager.Deregister(this);

            HasShutDown = true;

            if (!ServiceLocator.TryGet(out InputManager inputManager))
                return;

            inputManager.InputActions.Permanent.CreateSmallScreenshot.started -= CreateSmallScreenshot;
            inputManager.InputActions.Permanent.CreateLargeScreenshot.started -= CreateLargeScreenshot;
        }

        private void CreateSmallScreenshot(InputAction.CallbackContext _) => CreateScreenshot(1);

        private void CreateLargeScreenshot(InputAction.CallbackContext _) => CreateScreenshot(4);

        /// <summary>
        /// Creates a screenshot with the specified resolution multiplier.
        /// Saves the screenshot to the designated directory with a formatted name.
        /// </summary>
        /// <param name="multiplier">Multiplier for the screenshot resolution.</param>
        private void CreateScreenshot(int multiplier)
        {
            if (!Directory.Exists(ScreenshotDirectory))
                Directory.CreateDirectory(ScreenshotDirectory);

            string screenshotName = ScreenshotNameFormat
                .Replace("{GameTitle}", Application.productName)
                .Replace("{Width}", (Screen.width * multiplier).ToString())
                .Replace("{Height}", (Screen.height * multiplier).ToString())
                .Replace("{Time}", System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff"));

            string filePath = Path.Combine(ScreenshotDirectory, $"{screenshotName}.png");

            ScreenCapture.CaptureScreenshot(filePath, multiplier);

            CustomLogger.Log($"Screenshot saved to {filePath}", this);
        }
    }
}