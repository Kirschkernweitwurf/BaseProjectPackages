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
        [Tooltip("Padding in pixels kept between the selected element and the viewport edge.")]
        [SerializeField] private float padding = 16f;

        private readonly Vector3[] _worldCorners = new Vector3[4];

        private ScrollRect _scrollRect;
        private GameObject _lastSelected;

#region Unity Callbacks
        private void Awake() => _scrollRect = GetComponent<ScrollRect>();

        private void LateUpdate()
        {
            if (EventSystem.current == null || _scrollRect.content == null)
                return;

            GameObject current = EventSystem.current.currentSelectedGameObject;

            if (current == null || current == _lastSelected)
                return;

            if (!current.transform.IsChildOf(_scrollRect.content))
                return;

            _lastSelected = current;
            EnsureVisible(current.GetComponent<RectTransform>());
        }
#endregion

        private void EnsureVisible(RectTransform target)
        {
            if (target == null)
                return;

            RectTransform viewport = _scrollRect.viewport != null
                ? _scrollRect.viewport
                : (RectTransform)_scrollRect.transform;

            // Rebuild only this scroll view's content layout, not every canvas in the scene.
            LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);

            target.GetWorldCorners(_worldCorners);
            Vector2 min = viewport.InverseTransformPoint(_worldCorners[0]);
            Vector2 max = viewport.InverseTransformPoint(_worldCorners[2]);

            Rect view = viewport.rect;
            Vector2 delta = Vector2.zero;

            if (_scrollRect.vertical)
            {
                if (max.y > view.yMax - padding)
                    delta.y = max.y - (view.yMax - padding);
                else if (min.y < view.yMin + padding)
                    delta.y = min.y - (view.yMin + padding);
            }

            if (_scrollRect.horizontal)
            {
                if (max.x > view.xMax - padding)
                    delta.x = max.x - (view.xMax - padding);
                else if (min.x < view.xMin + padding)
                    delta.x = min.x - (view.xMin + padding);
            }

            if (delta == Vector2.zero)
                return;

            _scrollRect.velocity = Vector2.zero;
            _scrollRect.content.anchoredPosition -= delta;
        }
    }
}