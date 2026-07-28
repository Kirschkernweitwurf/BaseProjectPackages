using Base.AttributePackage;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Base.ControllerSupport.Controller.Scrolling
{
    /// <summary>
    /// Keeps the selected child of a <see cref="ScrollRect"/> visible inside the viewport. uGUI does
    /// not do this for gamepad navigation, so long lists scroll the selection out of sight without it.
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    public sealed class ScrollIntoView : MonoBehaviour
    {
        private const int BottomLeftCorner = 0;
        private const int CornerCount = 4;
        private const int TopRightCorner = 2;

        private readonly Vector3[] _worldCorners = new Vector3[CornerCount];

        [Tooltip("The scroll view this component drives. Auto-assigned from the same GameObject.")]
        [GetComponent]
        [Required]
        [SerializeField] private ScrollRect scrollRect;

        [Tooltip("Padding in pixels kept between the selected element and the viewport edge.")]
        [Suffix("px")]
        [Min(0)]
        [SerializeField] private float padding = 16f;

        private GameObject _lastSelected;

#region Unity Callbacks
        private void Awake()
        {
            if (scrollRect.content != null)
                return;

            CustomLogger.LogError($"The {nameof(ScrollRect)} has no content assigned, nothing can be scrolled "
                + "into view.", this);

            enabled = false;
        }

        private void LateUpdate()
        {
            if (EventSystem.current == null)
                return;

            GameObject current = EventSystem.current.currentSelectedGameObject;

            if (current == null
                || current == _lastSelected)
                return;

            if (!current.transform.IsChildOf(scrollRect.content))
                return;

            _lastSelected = current;
            EnsureVisible(current.GetComponent<RectTransform>());
        }
#endregion

        private void EnsureVisible(RectTransform target)
        {
            if (target == null)
                return;

            RectTransform viewport = scrollRect.viewport != null
                ? scrollRect.viewport
                : (RectTransform)scrollRect.transform;

            // Rebuild only this scroll view's content layout, not every canvas in the scene.
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);

            target.GetWorldCorners(_worldCorners);
            Vector2 min = viewport.InverseTransformPoint(_worldCorners[BottomLeftCorner]);
            Vector2 max = viewport.InverseTransformPoint(_worldCorners[TopRightCorner]);

            Vector2 delta = ResolveDelta(viewport.rect, min, max);

            if (delta == Vector2.zero)
                return;

            scrollRect.velocity = Vector2.zero;
            scrollRect.content.anchoredPosition -= delta;
        }

        private Vector2 ResolveDelta(Rect view, Vector2 min, Vector2 max)
        {
            Vector2 delta = Vector2.zero;

            if (scrollRect.vertical)
            {
                if (max.y > view.yMax - padding)
                    delta.y = max.y - (view.yMax - padding);
                else if (min.y < view.yMin + padding)
                    delta.y = min.y - (view.yMin + padding);
            }

            if (!scrollRect.horizontal)
                return delta;

            if (max.x > view.xMax - padding)
                delta.x = max.x - (view.xMax - padding);
            else if (min.x < view.xMin + padding)
                delta.x = min.x - (view.xMin + padding);

            return delta;
        }
    }
}