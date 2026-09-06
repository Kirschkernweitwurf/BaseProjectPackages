using Base.AttributesPackage;
using Base.UtilityPackage;
using TMPro;
using UnityEngine;

namespace Base.UIPackage.Utility
{
    /// <summary>
    /// A simple FPS counter that displays the current frames per second in a UI TextMeshPro component.
    /// </summary>
    public sealed class FpsCounter : MonoBehaviour
    {
        [SerializeField] private bool showInReleaseBuilds;
        [Required] [SerializeField] private TMP_Text fpsText;

        private readonly FpsSampler _sampler = new();

#region Unity Callbacks
        private void Awake()
        {
            if (Platform.IsRelease && !showInReleaseBuilds)
                gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_sampler.TryRead(Time.unscaledDeltaTime, out int fps))
                fpsText.text = $"{fps} FPS";
        }
#endregion
    }
}