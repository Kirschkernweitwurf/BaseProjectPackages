using System.Collections;
using Attributes;
using Systems.MenuManaging;
using UnityEngine;
using UnityEngine.UI;

namespace SceneManagement
{
    /// <summary>
    /// Handles the display and animation of the loading screen UI during scene transitions.
    /// Reacts to <see cref="SceneLoadEvents"/> to show progress and activity.
    /// </summary>
    public class LoadingScreen : Menu
    {
        [Header("Loading Screen References")]
        [Tooltip("The image used to display load progress via fill amount.")]
        [SerializeField] private Image progressImage;

        [Tooltip("The RectTransform that will spin while loading.")]
        [SerializeField] private RectTransform spinner;

        [Header("Animation Settings")]
        [Tooltip("Rotation speed for the spinner, in degrees per second.")]
        [SerializeField] private float spinnerRotationSpeed = 180f;

        [Tooltip("Optional smooth damping for progress fill updates.")]
        [SerializeField] private float fillSmoothSpeed = 5f;

        [Header("Minimum Show Time")]
        [Tooltip("If enabled, the loading screen will stay visible for at least this duration.")]
        [SerializeField] private bool hasMinimumShowTime = true;

        [Tooltip("Minimum time (in seconds) the loading screen must remain visible.")]
        [SerializeField] private float minimumShowTime = 1.0f;

        [Header("Scene Filtering")]
        [Tooltip("If non-empty, the loading screen will only show for these scenes.")]
        [SceneName, SerializeField] private string[] scenesToShowFor;

        private Coroutine _loadingRoutine;
        private Coroutine _closeRoutine;

        private float _targetFillAmount;
        private float _shownTime;

        private void OnEnable()
        {
            SceneLoadEvents.OnSceneLoadStarted += HandleLoadStarted;
            SceneLoadEvents.OnSceneLoadProgress += HandleLoadProgress;
            SceneLoadEvents.OnSceneLoadCompleted += HandleLoadCompleted;
        }

        private void OnDisable()
        {
            SceneLoadEvents.OnSceneLoadStarted -= HandleLoadStarted;
            SceneLoadEvents.OnSceneLoadProgress -= HandleLoadProgress;
            SceneLoadEvents.OnSceneLoadCompleted -= HandleLoadCompleted;
        }

        private void HandleLoadStarted(string sceneName)
        {
            if (!ShouldShowForScene(sceneName))
                return;

            _shownTime = 0f;
            _targetFillAmount = 0f;

            if (progressImage != null)
                progressImage.fillAmount = 0f;

            Open();

            StopLoadingRoutine();
            StopCloseRoutine();

            _loadingRoutine = StartCoroutine(LoadingRoutine());
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

            if (hasMinimumShowTime)
            {
                _closeRoutine = StartCoroutine(WaitAndClose());
            }
            else
            {
                StopLoadingRoutine();
                Close();
            }
        }

        private IEnumerator LoadingRoutine()
        {
            while (true)
            {
                _shownTime += Time.deltaTime;

                if (progressImage != null)
                {
                    progressImage.fillAmount = Mathf.Lerp(
                        progressImage.fillAmount,
                        _targetFillAmount,
                        Time.deltaTime * fillSmoothSpeed
                    );
                }

                if (spinner != null)
                    spinner.Rotate(0f, 0f, -spinnerRotationSpeed * Time.unscaledDeltaTime, Space.Self);

                yield return null;
            }
            // ReSharper disable once IteratorNeverReturns
        }

        private IEnumerator WaitAndClose()
        {
            float remainingTime = minimumShowTime - _shownTime;

            if (remainingTime > 0f)
                yield return new WaitForSecondsRealtime(remainingTime);

            StopLoadingRoutine();
            _closeRoutine = null;
            Close();
        }

        private bool ShouldShowForScene(string sceneName)
        {
            if (scenesToShowFor == null || scenesToShowFor.Length == 0)
                return true;

            foreach (string scene in scenesToShowFor)
            {
                if (scene.Equals(sceneName))
                    return true;
            }

            return false;
        }

        private void StopLoadingRoutine()
        {
            if (_loadingRoutine == null)
                return;

            StopCoroutine(_loadingRoutine);
            _loadingRoutine = null;
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