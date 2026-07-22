using Base.AttributePackage;
using Base.UtilityPackage;
using TMPro;
using UnityEngine;

namespace Base.UIPackage.Utility
{
    /// <summary>
    /// A simple FPS counter that displays the current frames per second in a UI TextMeshPro component.
    /// </summary>
    public class FpsCounter : MonoBehaviour
    {
        private const float UpdateInterval = 0.5f;

        [SerializeField] private bool showInReleaseBuilds;
        [Required] [SerializeField] private TMP_Text fpsText;

        private float _deltaTime;
        private float _timer;
        private int _lastFps = -1;

#region Unity Callbacks
        private void Awake()
        {
            if (Platform.IsRelease && !showInReleaseBuilds)
                gameObject.SetActive(false);
        }

        private void Update()
        {
            _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;

            _timer += Time.unscaledDeltaTime;
            if (_timer < UpdateInterval)
                return;

            _timer = 0f;

            int fps = Mathf.CeilToInt(1f / _deltaTime);
            if (fps == _lastFps)
                return;

            _lastFps = fps;
            fpsText.text = $"{fps} FPS";
        }
#endregion
    }
}
