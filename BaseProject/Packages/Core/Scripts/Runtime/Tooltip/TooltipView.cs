using Base.AttributePackage;
using System;
using System.Collections;
using Base.CorePackage.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Base.CorePackage.Tooltip
{
    /// <summary>
    /// Manages the visual representation of tooltips on the screen.
    /// Shows, hides and positions the tooltip so it never leaves the screen.
    /// </summary>
    [DisallowMultipleComponent]
    public class TooltipView : MonoBehaviour
    {
        private const int BottomLeftCorner = 0;
        private const int TopRightCorner = 2;
        private static readonly Vector2 TopLeftPivot = new(0f, 1f);

        [Header("Settings")]

        [Tooltip("Distance in pixels between the cursor and the tooltip.")]
        [SerializeField] private Vector2 screenOffset = new(15f, 15f);

        [Tooltip("Distance in pixels the tooltip keeps away from the screen edge.")]
        [Min(0f)] [SerializeField] private float edgeMargin = 8f;

        [Header("References")]

        [Tooltip("Content GameObject that contains the tooltip visuals.")]
        [Required] [SerializeField] private GameObject content;

        [Tooltip("Text element to display the tooltip message.")]
        [Required] [SerializeField] private TextMeshProUGUI textElement;

        [Tooltip("RectTransform of the tooltip for positioning (the Content rect).")]
        [Required] [SerializeField] private RectTransform tooltipRect;

        [Tooltip("Canvas the tooltip lives under. Auto-assigned from the parents when empty.")]
        [GetComponentInParent] [SerializeField] private Canvas canvas;

        private Coroutine _followRoutine;
        private Func<Vector2> _getScreenPosition;

#region Unity Callbacks
        private void Start()
        {
            // Fallback for instances created before the attribute could auto-assign.
            if (canvas == null)
                canvas = GetComponentInParent<Canvas>();

            ServiceLocator.Get<TooltipService>()?.SetView(this);
            Hide();
        }
#endregion

        /// <summary>
        /// Shows the tooltip with the specified data.
        /// </summary>
        public void Show(TooltipData data)
        {
            _getScreenPosition = data.GetScreenPosition;
            textElement.text = data.Message;
            content.SetActive(true);

            // Force the size fitter to apply now, so the first frame uses the real size.
            LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);

            if (_followRoutine != null)
                StopCoroutine(_followRoutine);

            _followRoutine = StartCoroutine(FollowPosition());
        }

        /// <summary>
        /// Hides the tooltip.
        /// </summary>
        public void Hide()
        {
            if (_followRoutine != null)
            {
                StopCoroutine(_followRoutine);
                _followRoutine = null;
            }

            content.SetActive(false);
            _getScreenPosition = null;
        }

        /// <summary>
        /// Updates the tooltip position every frame while it is visible.
        /// </summary>
        private IEnumerator FollowPosition()
        {
            RectTransform canvasRect = canvas.transform as RectTransform;
            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : canvas.worldCamera;

            tooltipRect.pivot = TopLeftPivot;

            while (content.activeSelf && _getScreenPosition != null)
            {
                PlaceTooltip(canvasRect, cam);
                yield return null;
            }

            _followRoutine = null;
        }

        /// <summary>
        /// Places the tooltip near the cursor.
        /// Prefers below-right of the cursor, flips to the other side if it would
        /// overflow, and finally clamps so it can never leave the screen.
        /// </summary>
        private void PlaceTooltip(RectTransform canvasRect, Camera cam)
        {
            Vector2 mouse = _getScreenPosition.Invoke();

            Vector3[] corners = new Vector3[4];
            tooltipRect.GetWorldCorners(corners);
            Vector2 bottomLeft = RectTransformUtility.WorldToScreenPoint(cam, corners[BottomLeftCorner]);
            Vector2 topRight = RectTransformUtility.WorldToScreenPoint(cam, corners[TopRightCorner]);
            float width = Mathf.Abs(topRight.x - bottomLeft.x);
            float height = Mathf.Abs(topRight.y - bottomLeft.y);

            float offsetX = Mathf.Abs(screenOffset.x);
            float offsetY = Mathf.Abs(screenOffset.y);

            float left = mouse.x + offsetX;
            if (left + width > Screen.width - edgeMargin)
                left = mouse.x - offsetX - width;

            float maxLeft = Mathf.Max(edgeMargin, Screen.width - edgeMargin - width);
            left = Mathf.Clamp(left, edgeMargin, maxLeft);

            float top = mouse.y - offsetY;
            if (top - height < edgeMargin)
                top = mouse.y + offsetY + height;

            float maxTop = Mathf.Max(edgeMargin + height, Screen.height - edgeMargin);
            top = Mathf.Clamp(top, edgeMargin + height, maxTop);

            Vector2 pivotScreen = new(left, top);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, pivotScreen, cam,
                out Vector2 localPoint);

            tooltipRect.localPosition = localPoint;
        }
    }
}