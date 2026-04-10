using System.Globalization;
using TMPro;
using UnityEngine;
using Utility;

namespace UI.Utility
{
    /// <summary>
    /// A simple FPS counter that displays the current frames per second in a UI TextMeshPro component.
    /// </summary>
    public class FpsCounter : MonoBehaviour
    {
        [SerializeField] private bool showInReleaseBuilds;
        [SerializeField] private TMP_Text fpsText;

        private float _deltaTime;

        private void Awake()
        {
            if (Platform.IsRelease && !showInReleaseBuilds)
                gameObject.SetActive(false);
        }

        private void Update()
        {
            _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
            float fps = 1.0f / _deltaTime;
            fpsText.text = Mathf.Ceil(fps).ToString(CultureInfo.InvariantCulture) + " FPS";
        }
    }
}