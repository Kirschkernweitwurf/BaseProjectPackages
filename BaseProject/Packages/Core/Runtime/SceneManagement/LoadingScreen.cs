using System;
using System.Collections;
using Base.AttributesPackage;
using Base.CorePackage.MenuManaging;
using UnityEngine;
using UnityEngine.UI;

namespace Base.CorePackage.SceneManagement
{
    /// <summary>
    /// Handles the display and animation of the loading screen UI during scene transitions.
    /// Reacts to <see cref="SceneLoadEvents"/> to show progress and activity.
    /// </summary>
    public class LoadingScreen : Menu
    {
        [Title("Loading Screen References")]
        [Tooltip("The image used to display load progress via fill amount.")]
        [Required] [SerializeField] private Image progressImage;

        [Tooltip("The RectTransform that will spin while loading.")]
        [Required] [SerializeField] private RectTransform spinner;

        [Title("Animation Settings")]
        [Tooltip("Rotation speed for the spinner, in degrees per second.")]
        [SerializeField] private float spinnerRotationSpeed = 180f;

        [Tooltip("Smoothing speed for progress fill updates.")]
        [Min(0f)] [SerializeField] private float fillSmoothSpeed = 5f;

        [Title("Minimum Show Time")]
        [Tooltip("If enabled, the loading screen will stay visible for at least this duration.")]
        [SerializeField] private bool hasMinimumShowTime = true;

        [Tooltip("Minimum time (in seconds) the loading screen must remain visible.")]
        [ShowIf(nameof(hasMinimumShowTime))] [Min(0f)] [SerializeField] private float minimumShowTime = 1f;

        [Title("Scene Filtering")]
        [Tooltip("If empty, the loading screen shows for every scene.")]
        [SceneName] [SerializeField] private string[] scenesToShowFor;

        private Coroutine _closeRoutine;

        private float _targetFillAmount;
        private float _shownTime;

#region Unity Callbacks
        private void OnEnable()
        {
            SceneLoadEvents.OnSceneLoadStarted += HandleLoadStarted;
            SceneLoadEvents.OnSceneLoadProgress += HandleLoadProgress;
            SceneLoadEvents.OnSceneLoadCompleted += HandleLoadCompleted;
        }

        // Unscaled time throughout, the loading screen has to keep animating while the game is paused.
        private void Update()
        {
            if (!IsOpen)
                return;

            _shownTime += Time.unscaledDeltaTime;

            progressImage.fillAmount = Mathf.Lerp(progressImage.fillAmount, _targetFillAmount,
                Time.unscaledDeltaTime * fillSmoothSpeed);

            spinner.Rotate(0f, 0f, -spinnerRotationSpeed * Time.unscaledDeltaTime, Space.Self);
        }

        private void OnDisable()
        {
            SceneLoadEvents.OnSceneLoadStarted -= HandleLoadStarted;
            SceneLoadEvents.OnSceneLoadProgress -= HandleLoadProgress;
            SceneLoadEvents.OnSceneLoadCompleted -= HandleLoadCompleted;

            // Unity already stopped the coroutine on disable, so only the stale handle is left to drop.
            _closeRoutine = null;
        }
#endregion

        private void HandleLoadStarted(string sceneName)
        {
            if (!ShouldShowForScene(sceneName))
                return;

            StopCloseRoutine();

            _shownTime = 0f;
            _targetFillAmount = 0f;
            progressImage.fillAmount = 0f;

            OpenIfClosed();
        }

        private void HandleLoadProgress(string sceneName, float progress)
        {
            if (!ShouldShowForScene(sceneName))
                return;

            _targetFillAmount = Mathf.Clamp01(progress);
        }

        private void HandleLoadCompleted(string sceneName, bool success)
        {
            if (!ShouldShowForScene(sceneName))
                return;

            _targetFillAmount = 1f;

            if (!hasMinimumShowTime)
            {
                CloseIfOpen();
                return;
            }

            _closeRoutine = StartCoroutine(WaitAndClose());
        }

        private IEnumerator WaitAndClose()
        {
            float remainingTime = minimumShowTime - _shownTime;

            if (remainingTime > 0f)
                yield return new WaitForSecondsRealtime(remainingTime);

            _closeRoutine = null;
            CloseIfOpen();
        }

        private bool ShouldShowForScene(string sceneName) => scenesToShowFor.Length == 0
            || Array.IndexOf(scenesToShowFor, sceneName) >= 0;

        // The base class warns when opening or closing twice, so only switch state when it actually changes.
        private void OpenIfClosed()
        {
            if (!IsOpen)
                Open();
        }

        private void CloseIfOpen()
        {
            if (IsOpen)
                Close();
        }

        private void StopCloseRoutine()
        {
            if (_closeRoutine == null)
                return;

            StopCoroutine(_closeRoutine);
            _closeRoutine = null;
        }
    }
}