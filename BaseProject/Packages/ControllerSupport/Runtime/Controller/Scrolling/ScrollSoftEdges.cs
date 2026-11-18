using Base.AttributesPackage;
using UnityEngine;
using UnityEngine.UI;

namespace Base.ControllerSupportPackage.Controller.Scrolling
{
    /// <summary>
    /// Fades the top and bottom edge of a scroll view once it has been scrolled, each only while more rows lie beyond
    /// it. At rest the first rows show crisp and whole, at the end the last ones do and the soft edges appear as soon
    /// as the content moves, to say there is more. A list that has to say it scrolls before anyone moves it fades its
    /// bottom already at rest.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ScrollRect), typeof(RectMask2D))]
    public sealed class ScrollSoftEdges : MonoBehaviour
    {
        private const float Bottom = 0f;
        private const float Top = 1f;

        // Normalized positions this close to an end count as at that end, so a rounding error never shows the fade.
        private const float Tolerance = 0.001f;

        [Tooltip("The scroll view. Auto-assigned from the same GameObject.")]
        [Required] [GetComponent] [SerializeField] private ScrollRect scroll;

        [Tooltip("The mask whose edges fade. Auto-assigned from the same GameObject.")]
        [Required] [GetComponent] [SerializeField] private RectMask2D mask;

        [Min(0)]
        [Tooltip("Pixels of the fade at the top and bottom edge once scrolled.")]
        [SerializeField] private int softness = 24;

        [Tooltip("Fades the bottom edge already at rest, so the list says it scrolls before anyone moves it. Off: "
            + "the edges fade only once scrolled. Either way an edge fades only while more rows lie beyond it.")]
        [SerializeField] private bool isFadingTowardMore;

        private Vector4 _padding;

#region Unity Callbacks
        private void Awake() => _padding = mask.padding;

        private void OnEnable()
        {
            scroll.onValueChanged.AddListener(OnScrolled);
            Apply();
        }

        // Rows built or swapped after the view woke change its height without a scroll event, so a list that fades
        // toward more checks every frame. It only touches the mask when the fade changes.
        private void LateUpdate()
        {
            if (isFadingTowardMore)
                Apply();
        }

        private void OnDisable() => scroll.onValueChanged.RemoveListener(OnScrolled);
#endregion

        private void OnScrolled(Vector2 position) => Apply();

        private void Apply()
        {
            bool canScroll = CanScroll();
            float position = scroll.verticalNormalizedPosition;
            bool hasMoreAbove = canScroll
                && position < Top - Tolerance;
            bool hasMoreBelow = canScroll
                && position > Bottom + Tolerance;

            // At rest only a list that fades toward more softens its bottom. Scrolled to the end, nothing fades there,
            // so the last row reads whole.
            bool isBottomFading = hasMoreBelow
                && (isFadingTowardMore || hasMoreAbove);

            FadeEdges(isTopFading: hasMoreAbove, isBottomFading);
        }

        // The mask fades both edges alike, so an edge that must stay crisp pushes its fade out past the view, where it
        // softens nothing.
        private void FadeEdges(bool isTopFading, bool isBottomFading)
        {
            Vector2Int edge = Vector2Int.zero;
            Vector4 padding = _padding;

            if (isTopFading
                || isBottomFading)
            {
                edge = new Vector2Int(0, softness);
                float bottom = isBottomFading
                    ? 0f
                    : -softness;
                float top = isTopFading
                    ? 0f
                    : -softness;
                padding += new Vector4(0f, bottom, 0f, top);
            }

            if (mask.softness != edge)
                mask.softness = edge;

            if (mask.padding != padding)
                mask.padding = padding;
        }

        // Content that fits reports a position at either end, which must not count as scrolled.
        private bool CanScroll()
        {
            RectTransform viewport = scroll.viewport != null
                ? scroll.viewport
                : (RectTransform)scroll.transform;

            return scroll.content.rect.height * scroll.content.localScale.y > viewport.rect.height;
        }
    }
}