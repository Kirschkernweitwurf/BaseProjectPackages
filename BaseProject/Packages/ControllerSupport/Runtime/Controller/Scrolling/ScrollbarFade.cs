using Base.AttributesPackage;
using UnityEngine;
using UnityEngine.UI;

namespace Base.ControllerSupportPackage.Controller.Scrolling
{
    /// <summary>
    /// Shows a scroll bar only while its list moves, like an overlay scroll bar on a phone: scrolling brings it in at
    /// once and a moment after the list rests it fades out again. Opening a list that moves to its top flashes it
    /// once, which tells that the list scrolls. Runs on unscaled time, because menus stop the time scale.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ScrollbarFade : MonoBehaviour
    {
        private const float Hidden = 0f;
        private const float Shown = 1f;

        [Required]
        [Tooltip("The scroll view whose movement shows the bar.")]
        [SerializeField] private ScrollRect scroll;

        [Required] [GetComponent]
        [Tooltip("Fades the bar.")]
        [SerializeField] private CanvasGroup group;

        [Min(0f)]
        [Tooltip("Seconds the bar stays after the list stopped moving.")]
        [SerializeField] private float holdSeconds = 0.8f;

        [Min(0f)]
        [Tooltip("Seconds the bar takes to fade out.")]
        [SerializeField] private float fadeSeconds = 0.3f;

        private float _lastScrollTime;

#region Unity Callbacks
        private void OnEnable()
        {
            group.alpha = Hidden;
            scroll.onValueChanged.AddListener(OnScrolled);
        }

        // Eases out like a quadratic tween: fast at first, gentle at the end.
        private void Update()
        {
            if (group.alpha <= Hidden)
                return;

            float faded = Time.unscaledTime - _lastScrollTime - holdSeconds;
            if (faded <= 0f)
                return;

            float progress = fadeSeconds <= 0f
                ? Shown
                : Mathf.Clamp01(faded / fadeSeconds);
            float left = Shown - progress;
            group.alpha = left * left;
        }

        private void OnDisable() => scroll.onValueChanged.RemoveListener(OnScrolled);
#endregion

        private void OnScrolled(Vector2 position)
        {
            _lastScrollTime = Time.unscaledTime;
            group.alpha = Shown;
        }
    }
}