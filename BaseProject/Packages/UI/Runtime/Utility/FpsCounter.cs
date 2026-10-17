using Base.AttributesPackage;
using Base.UtilityPackage;
using TMPro;
using UnityEngine;

namespace Base.UIPackage.Utility
{
    /// <summary>
    /// A simple FPS counter that displays the current frames per second in a UI TextMeshPro component.
    /// </summary>
    /// <remarks>
    /// <see cref="isShown"/> hides it with one checkbox, also while playing, without removing the object from the
    /// scene, so it is just as quick to bring back.
    /// </remarks>
    public sealed class FpsCounter : MonoBehaviour
    {
        [Tooltip("Shows the counter. Off hides it, and it stops measuring until it is on again.")]
        [SerializeField] private bool isShown = true;

        [SerializeField] private bool showInReleaseBuilds;
        [Required] [SerializeField] private TMP_Text fpsText;

        private readonly FpsSampler _sampler = new();

#region Unity Callbacks
        private void OnValidate() => ApplyShown();

        private void Awake()
        {
            if (Platform.IsRelease && !showInReleaseBuilds)
                gameObject.SetActive(false);

            ApplyShown();
        }

        private void Update()
        {
            if (!isShown)
                return;

            if (_sampler.TryRead(Time.unscaledDeltaTime, out int fps))
                fpsText.text = $"{fps} FPS";
        }
#endregion

        // The text is hidden rather than the object, so the counter can be switched back on from the inspector.
        private void ApplyShown()
        {
            if (fpsText == null)
                return;

            fpsText.enabled = isShown;
        }
    }
}